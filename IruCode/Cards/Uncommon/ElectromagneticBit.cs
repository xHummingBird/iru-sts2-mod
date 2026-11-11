using BaseLib.Utils;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Iru.IruCode.Cards.Uncommon;

public class ElectromagneticBit() : IruCard(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.AllEnemies), IHeatCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(2),
        new DynamicVar("Heat", 30)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<VulnerablePower>(),
        IruStaticHoverTips.Heat
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            CombatState.HittableEnemies,
            DynamicVars.Vulnerable.BaseValue,
            Owner.Creature,
            this);

        IruResourceManager.GainHeat(Owner, DynamicVars["Heat"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heat"].UpgradeValueBy(-10);
    }
}