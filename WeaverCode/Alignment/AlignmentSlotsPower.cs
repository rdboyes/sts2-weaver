using MegaCrit.Sts2.Core.Entities.Powers;
using Weaver.WeaverCode.Powers;

namespace Weaver.WeaverCode.Alignment;

/// <summary>Extra Alignment slots for this combat. See <see cref="AlignCmd.Slots"/>.</summary>
public sealed class AlignmentSlotsPower : WeaverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => "res://images/orbs/empty_orb.png";
    public override string CustomBigIconPath => "res://images/orbs/empty_orb.png";
}
