using MegaCrit.Sts2.Core.Entities.Powers;

namespace Iru.IruCode.Powers;

public class NoHeatDamagePower : IruPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;
}