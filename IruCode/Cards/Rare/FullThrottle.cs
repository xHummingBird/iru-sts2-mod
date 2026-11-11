using BaseLib.Utils;
using Iru.IruCode.Extensions;
using Iru.IruCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Rare;

public class FullThrottle() : IruCard(1, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(25, ValueProp.Move),
        new DynamicVar("Heat", 60)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IruStaticHoverTips.Heat
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        AudioHelper.PlayRandomDefend();

        await CommonActions.CardBlock(this, play);

        await PowerCmd.Apply<NextTurnHeatPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["Heat"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(7);
    }
}