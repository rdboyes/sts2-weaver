using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using Weaver.WeaverCode.Fuse;

namespace Weaver.WeaverCode.Powers;

/// <summary>All of your cards have Fuse for the rest of combat, including ones created later.</summary>
public sealed class MeltdownPower : WeaverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override string CustomPackedIconPath => "res://images/powers/corruption_power.png";
    public override string CustomBigIconPath => "res://images/powers/corruption_power.png";

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var piles = Owner.Player?.PlayerCombatState?.AllPiles;
        if (piles == null) return Task.CompletedTask;

        foreach (var card in piles.SelectMany(p => p.Cards).ToList()) AddFuse(card);
        return Task.CompletedTask;
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card.Owner.Creature == Owner) AddFuse(card);
        return Task.CompletedTask;
    }

    private static void AddFuse(CardModel card)
    {
        if (!FuseCmd.HasFuse(card)) card.AddKeyword(WeaverKeywords.Fuse);
    }
}
