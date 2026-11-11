using BaseLib.Extensions;
using Iru.IruCode.Mechanics;
using Iru.IruCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Iru.IruCode.Cards.Ancient;

public class HyperAttack() : IruCard(
    1,
    CardType.Skill,
    CardRarity.Ancient,
    TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal =>
        Owner.HasPower<HyperReadyPower>();

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        CardKeyword.Retain
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            List<IHoverTip> hoverTips =
            [
                HoverTipFactory.FromCard<LinearRay>()
            ];

            int hyper = IruResourceManager.GetHyper(Owner);

            if (hyper >= 200)
            {
                hoverTips.Add(
                    HoverTipFactory.FromCard<GenocideBomb>());
            }

            if (hyper >= 300)
            {
                hoverTips.Add(
                    HoverTipFactory.FromCard<LightDestructionRay>());
            }

            return hoverTips;
        }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        int hyper = IruResourceManager.GetHyper(Owner);

        List<CardModel> cards = [];

        var linearRay =
            CombatState.CreateCard<LinearRay>(Owner);

        if (IsUpgraded)
        {
            CardCmd.Upgrade(linearRay);
        }

        cards.Add(linearRay);

        if (hyper >= 200)
        {
            var genocideBomb =
                CombatState.CreateCard<GenocideBomb>(Owner);

            if (IsUpgraded)
            {
                CardCmd.Upgrade(genocideBomb);
            }

            cards.Add(genocideBomb);
        }

        if (hyper >= 300)
        {
            var lightDestructionRay =
                CombatState.CreateCard<LightDestructionRay>(Owner);

            if (IsUpgraded)
            {
                CardCmd.Upgrade(lightDestructionRay);
            }

            cards.Add(lightDestructionRay);
        }

        CardModel selectedFinisher =
            await CardSelectCmd.FromChooseACardScreen(
                choiceContext,
                cards,
                Owner,
                canSkip: false);

        if (selectedFinisher is GenocideBomb)
        {
            await CardCmd.AutoPlay(
                choiceContext,
                selectedFinisher,
                play.Target);
        }
        else
        {
            await CardCmd.AutoPlay(
                choiceContext,
                selectedFinisher,
                null);
        }
    }

    protected override void OnUpgrade()
    {
    }
}