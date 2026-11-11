using System.Threading.Tasks;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Iru.IruCode.Powers;

public class EnergyReservePower : IruPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
            return Task.CompletedTask;

        int energy =
            Owner.Player.PlayerCombatState.Energy;

        if (energy <= 0)
            return Task.CompletedTask;

        Flash();

        IruResourceManager.LoseHeat(
            Owner.Player,
            Amount * energy);

        return Task.CompletedTask;
    }
}