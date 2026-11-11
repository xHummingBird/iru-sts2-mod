using BaseLib.Utils;
using Iru.IruCode.Extensions;
using Iru.IruCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Uncommon;

public class ElectromagneticTrap() : IruCard(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
        [
            new BlockVar(12, ValueProp.Move),
            new PowerVar<ElectromagneticTrapPower>(3)
        ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        AudioHelper.PlayRandomDefend();
        await CommonActions.CardBlock(this, play);
        await PowerCmd.Apply<ElectromagneticTrapPower>(choiceContext, Owner.Creature, DynamicVars["ElectromagneticTrapPower"].IntValue,
            Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(3m);
        DynamicVars["ElectromagneticTrapPower"].UpgradeValueBy(3);
    }
}