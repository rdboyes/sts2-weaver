using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Weaver.WeaverCode.Powers;

/// <summary>Draw Amount cards whenever you apply Doom to an enemy.</summary>
public sealed class IllOmenPower : WeaverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => "res://images/powers/foregone_conclusion_power.png";
    public override string CustomBigIconPath => "res://images/powers/foregone_conclusion_power.png";

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not DoomPower || amount <= 0 || applier != Owner || power.Owner.Side == Owner.Side) return;

        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player!);
    }
}
