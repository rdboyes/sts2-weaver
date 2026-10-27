using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using Weaver.WeaverCode.Alignment;

namespace Weaver.WeaverCode.Powers;

/// <summary>Gain Amount Block whenever you change your alignment.</summary>
public sealed class AdaptiveWeavePower : WeaverPower, IAfterAlignmentChanged
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => "res://images/powers/adaptable_power.png";
    public override string CustomBigIconPath => "res://images/powers/adaptable_power.png";

    public async Task AfterAlignmentChanged(PlayerChoiceContext choiceContext, AlignmentPower newAlignment)
    {
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
