using Godot;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using Iru.IruCode.Powers;
using Iru.IruCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Uncommon;

public class Rapidfire() : IruCard(0, CardType.Attack,
    CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override bool HasEnergyCostX => true;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar("Heat", 20)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        int hitCount = ResolveEnergyXValue();

        if (hitCount <= 0)
            return;

        if (Owner?.Character is not Character.Iru iru)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(hitCount)
                .FromCard(this, play)
                .TargetingAllOpponents(base.CombatState)
                .Execute(choiceContext);
            return;
        }
        AudioHelper.PlayRandomAttack();
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this, play)
            .TargetingAllOpponents(base.CombatState)
            .WithNoAttackerAnim()
            .WithWaitBeforeHit(0.25f, 0.25f)
            .WithHitVfxNode(target =>
            {
                iru.PlayAnimation(
                    Owner.Creature,
                    "attack", true);

                SfxCmd.Play(
                    "res://Iru/sfx/Photon_rifle_fire.wav");

                _ = iru.PlayProjectile(
                    Owner.Creature,
                    target,
                    "res://Iru/scenes/vfx.tscn",
                    animationName: "photon_rifle",
                    spawnOffset: new Vector2(120, -80),
                    stopDistance: 130f,
                    duration: 0.1f
                );

                iru.PlayVfxOnTarget(
                    target,
                    "res://Iru/scenes/vfx.tscn",
                    "photon_hit"
                );

                return null;
            })
            .Execute(choiceContext);

        IruResourceManager.GainHeat(
            Owner,
            DynamicVars["Heat"].IntValue * hitCount);
        var heatAbsorption = Owner.Creature.GetPowerAmount<HeatAbsorptionPower>();
        var tomboy = Owner.Creature.GetPowerAmount<TomboyPower>();
        var heatEngine = Owner.Creature.GetPowerAmount<HeatEnginePower>();
        var conversionProtocol = Owner.Creature.GetPowerAmount<ConversionProtocolPower>();

        if (Owner.GetRelic<ElectromagneticTrapRelic>() != null)
        {
            foreach (var enemy in CombatState.HittableEnemies)
                await CreatureCmd.Damage(choiceContext, enemy, 3m, ValueProp.Unpowered, base.Owner.Creature);
        }

        IruResourceManager.GainHyper(Owner, conversionProtocol);
        await CreatureCmd.GainBlock(Owner.Creature, heatAbsorption, ValueProp.Unpowered, null, false);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, heatEngine, Owner.Creature, null);
        await CardPileCmd.Draw(choiceContext, tomboy, Owner);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["Heat"].UpgradeValueBy(-5);
    }
}