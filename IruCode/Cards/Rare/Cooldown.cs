using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using Iru.IruCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Iru.IruCode.Cards.Rare;

public class Cooldown() : IruCard(1, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<HyperReadyPower>(),
        IruStaticHoverTips.Heat
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hyperAmount = IruResourceManager.GetHeat(Owner) / 2;
        IruResourceManager.ClearHeat(Owner);
        IruResourceManager.GainHyper(Owner, hyperAmount);
    }
    
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}