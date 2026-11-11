using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using Iru.IruCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Iru.IruCode.Cards.Rare;

public class BorrowedTime() : IruCard(0, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IruStaticHoverTips.Heat
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var heatAmount = IruResourceManager.GetHeat(Owner);
        await PowerCmd.Apply<NextTurnHeatPower>(choiceContext, Owner.Creature, heatAmount, Owner.Creature, this);
        await PowerCmd.Apply<NoHeatDamagePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}