using BaseLib.Abstracts;
using Iru.IruCode.Extensions;
using Godot;

namespace Iru.IruCode.Character;

public class IruPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Iru.Color;
    

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}