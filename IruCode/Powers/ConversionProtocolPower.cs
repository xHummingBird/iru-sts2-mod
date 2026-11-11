using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Powers;

public class ConversionProtocolPower : IruPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    
    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        CardModel card =
            cardPlay.Card;

        if (card.Owner.Creature != Owner)
            return Task.CompletedTask;

        if (!CombatManager.Instance.IsInProgress)
            return Task.CompletedTask;

        if (card is not IHeatCard)
            return Task.CompletedTask;
        Flash();
        IruResourceManager.GainHyper(Owner.Player, Amount);
        
        return Task.CompletedTask;
    }
}