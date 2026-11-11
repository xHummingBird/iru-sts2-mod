using BaseLib.Utils;
using Iru.IruCode.Extensions;
using Iru.IruCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Iru.IruCode.Cards.Rare;

public class HeatAbsorption() : IruCard(
    1,
    CardType.Power,
    CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<HeatAbsorptionPower>(4)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IruStaticHoverTips.Heat
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<HeatAbsorptionPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["HeatAbsorptionPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HeatAbsorptionPower"]
            .UpgradeValueBy(2);
    }
}