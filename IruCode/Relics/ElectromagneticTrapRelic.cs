using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Relics;

//Whenever you gain Heat, deal 3 damage to ALL enemies

public class ElectromagneticTrapRelic : IruRelic
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(3m, ValueProp.Unpowered)
    ];
    
    public override RelicRarity Rarity => RelicRarity.Common;
    
    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        CardModel card =
            cardPlay.Card;

        if (card.Owner != Owner)
            return;

        if (!CombatManager.Instance.IsInProgress)
            return;

        if (card is not IHeatCard)
            return;
        Flash();
        
        await CreatureCmd.Damage(choiceContext, base.Owner.Creature.CombatState.HittableEnemies, base.DynamicVars.Damage, base.Owner.Creature);
    }
}