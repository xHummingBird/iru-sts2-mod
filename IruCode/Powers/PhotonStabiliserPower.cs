using MegaCrit.Sts2.Core.Entities.Powers;

namespace Iru.IruCode.Powers;


public class PhotonStabiliserPower: IruPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
}