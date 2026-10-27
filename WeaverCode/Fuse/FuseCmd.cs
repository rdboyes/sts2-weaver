using BaseLib.Patches.Localization;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Weaver.WeaverCode.Alignment;

namespace Weaver.WeaverCode.Fuse;

public static class FuseCmd
{
    /// <summary>
    /// Fused copies of cards whose effects ride along on a card. These live on the combat copy of the card, so fusing
    /// never touches the deck and is gone when combat ends.
    /// </summary>
    private static readonly NotNullSpireField<CardModel, List<CardModel>> Payloads =
        new NotNullSpireField<CardModel, List<CardModel>>(() => [])
            .CopyOnClone((_, dst, payloads) => Payloads![dst] = [..payloads]);

    /// <summary>
    /// Alignment riders a fused copy was played with. Merged bonuses (e.g. Lightning's +1 damage) are baked into the
    /// copy's own numbers instead.
    /// </summary>
    private static readonly NotNullSpireField<CardModel, List<AlignmentPower>> Riders = new(() => []);

    private static readonly SpireField<CardModel, bool> PayloadFlag = new(() => false);

    private static readonly Func<CardModel, PlayerChoiceContext, CardPlay, Task> InvokeOnPlay =
        AccessTools.MethodDelegate<Func<CardModel, PlayerChoiceContext, CardPlay, Task>>(
            AccessTools.Method(typeof(CardModel), "OnPlay"), virtualCall: true);

    /// <summary>Called once from the mod initializer.</summary>
    public static void Initialize()
    {
        DescriptionOverrides.CustomizeDescription += AddEnhancementText;
    }

    public static bool HasFuse(CardModel card) => card.Keywords.Contains(WeaverKeywords.Fuse);

    public static bool IsPayload(CardModel card) => PayloadFlag[card];

    /// <summary>Resolve every effect fused onto the played card, using its target where possible.</summary>
    public static async Task PlayPayloads(PlayerChoiceContext choiceContext, CardPlay hostPlay)
    {
        var owner = hostPlay.Card.Owner.Creature;
        foreach (var payload in Payloads[hostPlay.Card].ToList())
        {
            if (owner.IsDead) return;

            var target = TargetFor(payload, hostPlay);
            if (payload.TargetType is TargetType.AnyEnemy && target == null) continue;

            var payloadPlay = new CardPlay
            {
                Card = payload,
                Target = target,
                ResultPile = hostPlay.ResultPile,
                Resources = hostPlay.Resources,
                IsAutoPlay = true,
                PlayIndex = 0,
                PlayCount = 1
            };
            await InvokeOnPlay(payload, choiceContext, payloadPlay);

            foreach (var alignment in Riders[payload])
            {
                if (owner.IsDead) return;
                await alignment.Rider(choiceContext, owner, payload, target);
            }
        }
    }

    /// <summary>
    /// Ask the player to pick a card in hand, then give it <paramref name="source"/>'s effects (and everything
    /// already fused onto it), its cost, and Fuse.
    /// </summary>
    public static async Task FuseIntoHand(PlayerChoiceContext choiceContext, CardModel source)
    {
        var prefs = new CardSelectorPrefs(new LocString("card_selection", "WEAVER-FUSE_PROMPT"), 1);
        var host = (await CardSelectCmd.FromHand(choiceContext, source.Owner, prefs, c => c != source, source))
            .FirstOrDefault();
        if (host == null) return;

        FuseOnto(source, host);
    }

    public static void FuseOnto(CardModel source, CardModel host)
    {
        var hostPayloads = Payloads[host];
        hostPayloads.Add(SnapshotPayload(source));
        hostPayloads.AddRange(Payloads[source]);

        if (!source.EnergyCost.CostsX)
        {
            // The cost as it stands when played, so alignment discounts (Plasma) carry over.
            int cost = source.EnergyCost.GetWithModifiers(CostModifiers.All);
            if (cost > 0) host.EnergyCost.AddThisCombat(cost);
        }

        host.AddKeyword(WeaverKeywords.Fuse);
    }

    /// <summary>A copy of the card's own effects, locked to the alignment it was played with.</summary>
    private static CardModel SnapshotPayload(CardModel source)
    {
        var payload = source.CreateClone();
        PayloadFlag[payload] = true;
        Payloads[payload] = [];
        payload.RemoveKeyword(WeaverKeywords.Fuse);

        var riders = Riders[payload];
        riders.Clear();
        foreach (var alignment in AlignmentPower.On(source.Owner.Creature))
        {
            if (alignment.MergesInto(payload)) alignment.BakeInto(payload);
            else if (alignment.HasRider) riders.Add(alignment);
        }
        return payload;
    }

    private static Creature? TargetFor(CardModel payload, CardPlay hostPlay)
    {
        if (payload.TargetType is not TargetType.AnyEnemy) return hostPlay.Target;

        var owner = payload.Owner.Creature;
        if (hostPlay.Target is { IsHittable: true } t && t.Side != owner.Side) return t;

        var enemies = owner.CombatState?.GetOpponentsOf(owner).Where(e => e.IsHittable).ToList();
        if (enemies == null || enemies.Count == 0) return null;
        return payload.Owner.RunState.Rng.CombatTargets.NextItem(enemies);
    }

    /// <summary>
    /// Adds alignment riders and fused effects to the card's main text, ahead of keyword lines like "Fuse.".
    /// </summary>
    private static void AddEnhancementText(CardModel card, Creature? target, ref string description)
    {
        if (!card.IsMutable) return;

        var lines = new List<string> { description };

        if (IsPayload(card))
        {
            lines.AddRange(Riders[card].Select(a => a.RiderText));
        }
        else if (CombatManager.Instance.IsInProgress && card.Pile is { IsCombatPile: true })
        {
            lines.AddRange(AlignmentPower.On(card.Owner.Creature)
                .Where(a => a.HasRider && !a.MergesInto(card))
                .Select(a => a.RiderText));
        }

        foreach (var payload in Payloads[card])
        {
            var loc = new LocString("card_selection", "WEAVER-FUSED_EFFECT");
            loc.Add("Title", payload.Title);
            loc.Add("Text", payload.GetDescriptionForPile(PileType.None).Replace('\n', ' '));
            lines.Add(loc.GetFormattedText());
        }

        description = string.Join('\n', lines.Where(l => !string.IsNullOrEmpty(l)));
    }
}
