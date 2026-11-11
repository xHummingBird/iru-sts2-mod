using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;

namespace Iru.IruCode.Relics;

//Add 2 random cards that can gain heat
public class MissileLauncher : IruRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature) &&
            Owner.PlayerCombatState.TurnNumber <= 1)
        {
            IReadOnlyList<CardModel> heatCards =
                Owner.Character.CardPool
                    .GetUnlockedCards(
                        Owner.UnlockState,
                        Owner.RunState.CardMultiplayerConstraint)
                    .Where(c => c is IHeatCard)
                    .ToList();

            if (heatCards.Count > 0)
            {
                Flash();

                List<CardModel> cards = CardFactory
                    .GetDistinctForCombat(
                        Owner,
                        heatCards,
                        2,
                        Owner.RunState.Rng.CombatCardGeneration)
                    .ToList();

                await CardPileCmd.AddGeneratedCardsToCombat(
                    cards,
                    PileType.Hand,
                    Owner);
            }
        }
    }
}