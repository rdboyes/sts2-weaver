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
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Weaver.WeaverCode.Alignment;

namespace Weaver.WeaverCode.Fuse;

public static class FuseCmd
{
    /// <summary>
    /// What has been fused onto a card beyond its own numbers. Lives on the combat copy of the card, so fusing never
    /// touches the deck and is gone when combat ends.
    /// </summary>
    private static readonly NotNullSpireField<CardModel, FusedEffects> Fused =
        new NotNullSpireField<CardModel, FusedEffects>(() => new FusedEffects())
            .CopyOnClone((_, dst, fused) => Fused![dst] = fused.Copy());

    private static readonly SpireField<CardModel, bool> PayloadFlag = new(() => false);

    private static readonly Func<CardModel, PlayerChoiceContext, CardPlay, Task> InvokeOnPlay =
        AccessTools.MethodDelegate<Func<CardModel, PlayerChoiceContext, CardPlay, Task>>(
            AccessTools.Method(typeof(CardModel), "OnPlay"), virtualCall: true);

    /// <summary>Order effects are resolved and listed in.</summary>
    private static readonly FuseStat[] StatOrder =
        [FuseStat.Damage, FuseStat.DamageAll, FuseStat.Block, FuseStat.Plating, FuseStat.Doom, FuseStat.Draw, FuseStat.AlignmentSlots];

    /// <summary>Called once from the mod initializer.</summary>
    public static void Initialize()
    {
        DescriptionOverrides.CustomizeDescription += AddEnhancementText;
    }

    public static bool HasFuse(CardModel card) => card.Keywords.Contains(WeaverKeywords.Fuse);

    /// <summary>Hidden copies of non-<see cref="IFusable"/> cards, played as part of a fused card.</summary>
    public static bool IsPayload(CardModel card) => PayloadFlag[card];

    /// <summary>Resolve everything fused onto the played card, after its own effects.</summary>
    public static async Task PlayFused(PlayerChoiceContext choiceContext, CardPlay hostPlay)
    {
        var host = hostPlay.Card;
        var fused = Fused[host];
        if (fused.IsEmpty) return;

        var owner = host.Owner.Creature;
        Creature? target = null;
        Creature? Target() => target is { IsHittable: true } ? target : target = EnemyTarget(owner, hostPlay.Target);

        foreach (var stat in StatOrder)
        {
            decimal amount = fused[stat];
            if (amount <= 0) continue;
            if (owner.IsDead) return;

            switch (stat)
            {
                case FuseStat.Damage when Target() is { } enemy:
                    VfxCmd.PlayOnCreature(enemy, "vfx/vfx_attack_slash");
                    await CreatureCmd.Damage(choiceContext, enemy, amount, ValueProp.Move, owner);
                    break;
                case FuseStat.DamageAll:
                    var enemies = owner.CombatState?.GetOpponentsOf(owner).Where(e => e.IsHittable).ToList();
                    if (enemies is { Count: > 0 })
                        await CreatureCmd.Damage(choiceContext, enemies, amount, ValueProp.Unpowered, owner);
                    break;
                case FuseStat.Block:
                    await CreatureCmd.GainBlock(owner, amount, ValueProp.Move, null);
                    break;
                case FuseStat.Plating:
                    await PowerCmd.Apply<PlatingPower>(choiceContext, owner, amount, owner, null);
                    break;
                case FuseStat.Doom when Target() is { } enemy:
                    await PowerCmd.Apply<DoomPower>(choiceContext, enemy, amount, owner, null);
                    break;
                case FuseStat.Draw:
                    await CardPileCmd.Draw(choiceContext, amount, host.Owner);
                    break;
                case FuseStat.AlignmentSlots:
                    await AlignCmd.AddSlots(choiceContext, owner, (int)amount, host);
                    break;
            }
        }

        foreach (var align in fused.Aligns.ToList())
        {
            if (owner.IsDead) return;
            await align.Apply(choiceContext, owner, host);
        }

        foreach (var payload in fused.Others.ToList())
        {
            if (owner.IsDead) return;

            var payloadTarget = payload.TargetType is TargetType.AnyEnemy ? Target() : hostPlay.Target;
            if (payload.TargetType is TargetType.AnyEnemy && payloadTarget == null) continue;

            await InvokeOnPlay(payload, choiceContext, new CardPlay
            {
                Card = payload,
                Target = payloadTarget,
                ResultPile = hostPlay.ResultPile,
                Resources = hostPlay.Resources,
                IsAutoPlay = true,
                PlayIndex = 0,
                PlayCount = 1
            });
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
        Absorb(host, EffectsOf(source));

        if (!source.EnergyCost.CostsX)
        {
            // The cost as it stands when played, so alignment discounts (Plasma) carry over.
            int cost = source.EnergyCost.GetWithModifiers(CostModifiers.All);
            if (cost > 0) host.EnergyCost.AddThisCombat(cost);
        }

        host.AddKeyword(WeaverKeywords.Fuse);
    }

    /// <summary>
    /// Everything <paramref name="source"/> does when played right now: its own effects, the bonuses from the
    /// alignments it is played under, and whatever was already fused onto it.
    /// </summary>
    private static FusedEffects EffectsOf(CardModel source)
    {
        var effects = new FusedEffects();
        if (source is IFusable fusable)
        {
            foreach (var (stat, dynamicVar) in fusable.FuseStats) effects.Add(stat, dynamicVar.BaseValue);
            foreach (var align in fusable.FuseAligns) effects.AddAlign(align);
        }
        else
        {
            var payload = source.CreateClone();
            PayloadFlag[payload] = true;
            Fused[payload] = new FusedEffects();
            payload.RemoveKeyword(WeaverKeywords.Fuse);
            effects.Others.Add(payload);
        }

        foreach (var alignment in AlignmentPower.On(source.Owner.Creature)) alignment.AddFusedBonus(effects);

        effects.AddAll(Fused[source]);
        return effects;
    }

    /// <summary>Fold effects into the host's own numbers where it has them, and keep the rest alongside.</summary>
    private static void Absorb(CardModel host, FusedEffects effects)
    {
        var hostVars = new Dictionary<FuseStat, DynamicVar>();
        var hostAligns = new HashSet<Type>();
        if (host is IFusable fusable)
        {
            foreach (var (stat, dynamicVar) in fusable.FuseStats) hostVars.TryAdd(stat, dynamicVar);
            foreach (var align in fusable.FuseAligns) hostAligns.Add(align.Alignment);
        }

        var fused = Fused[host];
        foreach (var (stat, amount) in effects.Stats)
        {
            if (hostVars.TryGetValue(stat, out var dynamicVar)) dynamicVar.BaseValue += amount;
            else fused.Add(stat, amount);
        }

        foreach (var align in effects.Aligns.Where(a => !hostAligns.Contains(a.Alignment))) fused.AddAlign(align);
        fused.Others.AddRange(effects.Others);
    }

    private static Creature? EnemyTarget(Creature owner, Creature? preferred)
    {
        if (preferred is { IsHittable: true } && preferred.Side != owner.Side) return preferred;

        var enemies = owner.CombatState?.GetOpponentsOf(owner).Where(e => e.IsHittable).ToList();
        if (enemies == null || enemies.Count == 0) return null;
        return owner.Player!.RunState.Rng.CombatTargets.NextItem(enemies);
    }

    /// <summary>
    /// Adds alignment riders and a single combined "Fused:" line to the card's main text, ahead of keyword lines
    /// like "Fuse.".
    /// </summary>
    private static void AddEnhancementText(CardModel card, Creature? target, ref string description)
    {
        if (!card.IsMutable || IsPayload(card)) return;

        var lines = new List<string> { description };

        if (CombatManager.Instance.IsInProgress && card.Pile is { IsCombatPile: true })
        {
            lines.AddRange(AlignmentPower.On(card.Owner.Creature)
                .Where(a => a.HasRider && !a.MergesInto(card))
                .Select(a => a.RiderText));
        }

        var fused = Fused[card];
        if (!fused.IsEmpty)
        {
            var parts = new List<string>();
            foreach (var stat in StatOrder)
            {
                if (fused[stat] <= 0) continue;
                var loc = new LocString("card_selection", $"WEAVER-FUSED_{stat.ToString().ToUpperInvariant()}");
                loc.Add("Amount", fused[stat]);
                parts.Add(loc.GetFormattedText());
            }
            foreach (var align in fused.Aligns)
            {
                var loc = new LocString("card_selection", "WEAVER-FUSED_ALIGN");
                loc.Add("Alignment", align.ShortName);
                parts.Add(loc.GetFormattedText());
            }
            foreach (var payload in fused.Others)
            {
                var loc = new LocString("card_selection", "WEAVER-FUSED_PLAY");
                loc.Add("Title", payload.Title);
                parts.Add(loc.GetFormattedText());
            }

            var header = new LocString("card_selection", "WEAVER-FUSED_HEADER").GetFormattedText();
            lines.Add(header + " " + string.Join(" ", parts));
        }

        description = string.Join('\n', lines.Where(l => !string.IsNullOrEmpty(l)));
    }
}
