using BaseLib.Abstracts;
using BaseLib.Extensions;
using Iru.IruCode.Extensions;
using Godot;

namespace Iru.IruCode.Powers;

public abstract class IruPower : CustomPowerModel
{
    //Loads from Iru/images/powers/your_power.png
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}