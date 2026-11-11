using BaseLib.Utils;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Rare;

public class Overheat() : IruCard(0, CardType.Skill,
    CardRarity.Rare, TargetType.Self), IHeatCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(2),
        new DynamicVar("Heat", 50),
        new CardsVar(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IruStaticHoverTips.Heat
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        AudioHelper.PlayRandomDefend();

        await PlayerCmd.GainEnergy(2, Owner);
        IruResourceManager.GainHeat(Owner, DynamicVars["Heat"].IntValue);
        await CardPileCmd.Draw(choiceContext, 2, Owner);

    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heat"].UpgradeValueBy(-15);
    }
}