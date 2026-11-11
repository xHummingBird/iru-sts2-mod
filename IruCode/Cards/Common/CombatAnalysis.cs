using BaseLib.Utils;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using Iru.IruCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Common;

public class CombatAnalysis() : IruCard(0, CardType.Skill,
    CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new DynamicVar("Heat", 30)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IruStaticHoverTips.Heat
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        IEnumerable<CardModel> drawnCards =
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);

        CardModel? drawnCard =
            drawnCards.FirstOrDefault();

        if (drawnCard is not { Type: CardType.Attack })
        {
            IruResourceManager.GainHeat(Owner, DynamicVars["Heat"].IntValue);
            var heatAbsorption = Owner.Creature.GetPowerAmount<HeatAbsorptionPower>();
            var tomboy = Owner.Creature.GetPowerAmount<TomboyPower>();
            var heatEngine = Owner.Creature.GetPowerAmount<HeatEnginePower>();
            var conversionProtocol = Owner.Creature.GetPowerAmount<ConversionProtocolPower>();
        
            IruResourceManager.GainHyper(Owner, conversionProtocol);
            await CreatureCmd.GainBlock(Owner.Creature, heatAbsorption, ValueProp.Unpowered, null, false);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, heatEngine, Owner.Creature, null);
            await CardPileCmd.Draw(choiceContext, tomboy, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heat"].UpgradeValueBy(-10);
    }
}