using BaseLib.Utils;
using Iru.IruCode.Extensions;
using Iru.IruCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Iru.IruCode.Cards.Rare;

public class RainbowCircle() : IruCard(1, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<IntangiblePower>(1),
        new DynamicVar("Heat", 100)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<IntangiblePower>(),
        IruStaticHoverTips.Heat
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<IntangiblePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["IntangiblePower"].BaseValue,
            Owner.Creature,
            this);

        await PowerCmd.Apply<NextTurnHeatPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["Heat"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heat"].UpgradeValueBy(-30);
    }
}