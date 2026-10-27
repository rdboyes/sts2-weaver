using BaseLib.Abstracts;
using Weaver.WeaverCode.Extensions;
using Godot;

namespace Weaver.WeaverCode.Character;

public class WeaverRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => Weaver.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}