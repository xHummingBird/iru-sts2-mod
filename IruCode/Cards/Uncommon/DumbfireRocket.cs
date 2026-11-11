using Godot;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Uncommon;

public class DumbfireRocket() : IruCard(1, CardType.Attack,
    CardRarity.Uncommon, TargetType.RandomEnemy), IHeatCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(3, ValueProp.Move),
        new DynamicVar("Heat", 30),
        new RepeatVar(5)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IruStaticHoverTips.Heat
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (Owner?.Character is not Character.Iru iru)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(DynamicVars.Repeat.IntValue)
                .FromCard(this, play)
                .TargetingRandomOpponents(base.CombatState)
                .Execute(choiceContext);
            return;
        }
        AudioHelper.PlayRandomAttack();
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCard(this, play)
            .TargetingRandomOpponents(base.CombatState)
            .WithHitFx(null, "res://Iru/sfx/fire_impact (2).wav")
            .WithNoAttackerAnim()
            .WithWaitBeforeHit(0.35f, 0.35f)
            .WithHitVfxNode(target =>
            {
                iru.PlayAnimation(
                    Owner.Creature,
                    "attack_4", false);

                SfxCmd.Play(
                    "res://Iru/sfx/Rocket_and_missile_barrage_launch.wav");

                _ = iru.PlayProjectile(
                    Owner.Creature,
                    target,
                    "res://Iru/scenes/vfx.tscn",
                    animationName: "missile_2",
                    spawnOffset: new Vector2(100, -80),
                    stopDistance: 110f,
                    duration: 0.2f
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
            DynamicVars["Heat"].IntValue);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}