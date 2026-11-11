using BaseLib.Utils;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Common;

public class Reload() : IruCard(1, CardType.Skill,
    CardRarity.Common, TargetType.Self), IHeatCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2),
        new DynamicVar("Heat", 30)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IruStaticHoverTips.Heat
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        AudioHelper.PlayRandomDefend();
        IruResourceManager.GainHeat(Owner, 30);
        foreach (CardModel item in await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.IntValue, base.Owner))
        {
            CardCmd.ApplySingleTurnRetain(item);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}