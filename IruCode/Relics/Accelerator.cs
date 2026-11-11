using System.Threading.Tasks;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace Iru.IruCode.Relics;

// Start combat with 50 Hyper
public class Accelerator : IruRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    public override Task BeforeCombatStart()
    {
        IruResourceManager.GainHyper(Owner, 50);

        return Task.CompletedTask;
    }
}