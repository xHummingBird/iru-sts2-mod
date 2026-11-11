using MegaCrit.Sts2.Core.Entities.Relics;

namespace Iru.IruCode.Relics;

public class PhotonBuster : IruResourceRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    
    protected override int HyperPerTurn => 15;
}