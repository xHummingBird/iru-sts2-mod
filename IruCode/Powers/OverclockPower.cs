using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Iru.IruCode.Powers;

public class OverclockPower : IruPower
{
    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override async Task AfterSideTurnStartLate(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        await PowerCmd.Remove(this);
    }
    
    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        CardModel card =
            cardPlay.Card;

        if (card.Owner != Owner.Player)
            return Task.CompletedTask;

        if (!CombatManager.Instance.IsInProgress)
            return Task.CompletedTask;

        if (card.Type is not CardType.Attack)
            return Task.CompletedTask;

        IruResourceManager.GainHeat(Owner.Player, Amount);
        return Task.CompletedTask;
    }
    
}