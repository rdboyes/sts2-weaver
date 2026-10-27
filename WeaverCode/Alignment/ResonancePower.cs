using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Weaver.WeaverCode.Powers;

namespace Weaver.WeaverCode.Alignment;

/// <summary>Alignments are stronger by Amount. See <see cref="AlignCmd.Strength"/>.</summary>
public sealed class ResonancePower : WeaverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => "res://images/powers/focus_power.png";
    public override string CustomBigIconPath => "res://images/powers/focus_power.png";

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        AlignCmd.RefreshStrength(Owner);
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power == this) AlignCmd.RefreshStrength(Owner);
        return Task.CompletedTask;
    }
}
