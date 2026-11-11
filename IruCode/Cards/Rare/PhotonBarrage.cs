using Godot;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Rare;

public class PhotonBarrage() : IruCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy), IHeatCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(7, ValueProp.Move),
        new DynamicVar("Heat", 50),
        new RepeatVar(4)
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
                .Targeting(play.Target)
                .Execute(choiceContext);
            return;
        }
        AudioHelper.PlayRandomAttack();
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCard(this, play)
            .Targeting(play.Target)
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
            DynamicVars["Heat"].IntValue);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}