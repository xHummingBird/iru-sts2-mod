using System.Collections.Generic;
using System.Threading.Tasks;
using Iru.IruCode.Mechanics;
using Iru.IruCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Powers;

public class NextTurnHeatPower : IruPower
{
    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(3m, ValueProp.Unpowered)
    ];

    public override async Task AfterSideTurnStartLate(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != Owner.Side)
            return;

        IruResourceManager.GainHeat(
            Owner.Player,
            Amount);
        
        var heatAbsorption = Owner.GetPowerAmount<HeatAbsorptionPower>();
        var tomboy = Owner.GetPowerAmount<TomboyPower>();
        var heatEngine = Owner.GetPowerAmount<HeatEnginePower>();
        var conversionProtocol = Owner.GetPowerAmount<ConversionProtocolPower>();
        
        if (Owner.Player.GetRelic<ElectromagneticTrapRelic>() != null)
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.CombatState.HittableEnemies, base.DynamicVars.Damage, base.Owner);
        
        await CreatureCmd.GainBlock(Owner, heatAbsorption, ValueProp.Unpowered, null, false);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, heatEngine, Owner, null);
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), tomboy, Owner.Player);
        IruResourceManager.GainHyper(Owner.Player, conversionProtocol);

        await PowerCmd.Remove(this);
    }
}