using Iru.IruCode.Cards.Ancient;
using Iru.IruCode.Mechanics;
using Iru.IruCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Iru.IruCode.Powers;

public class HyperReadyPower : IruPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;
    
    public override async Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        var existingCard = Owner.Player.PlayerCombatState?
            .AllCards
            .OfType<HyperAttack>()
            .FirstOrDefault();

        if (existingCard == null)
        {
            var card =
                CombatState.CreateCard<HyperAttack>(
                    Owner.Player);

            if (Owner.Player.GetRelic<PhotonBuster>() != null)
            {
                CardCmd.Upgrade(card);
            }

            await CardPileCmd.AddGeneratedCardToCombat(
                card,
                PileType.Hand,
                Owner.Player);
        }
        else if (existingCard.Pile?.Type != PileType.Hand)
        {
            await CardPileCmd.Add(
                existingCard,
                PileType.Hand);
        }
    }
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != base.Owner.Side)
            return;

        var player = Owner.Player;
        var playerState = player.PlayerCombatState;

        if (playerState == null)
            return;
        
        if (playerState.AllCards.OfType<HyperAttack>().All(c => c.Pile?.Type != PileType.Hand))
        {
            var cards = playerState.AllCards
                .OfType<HyperAttack>()
                .Where(c =>
                    c.Pile == null ||
                    c.Pile.Type != PileType.Hand);

            await CardPileCmd.Add(
                cards,
                PileType.Hand);
        }
    }
}