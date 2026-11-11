using BaseLib.Utils;
using Godot;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Uncommon;

public class FireAndForget() : IruCard(0, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    private int PlayCountThisTurn =>
        CombatManager.Instance.History.CardPlaysFinished
            .Count(e =>
                e.HappenedThisTurn(base.CombatState) &&
                e.CardPlay.Player == base.Owner);
    
    protected override bool ShouldGlowGoldInternal => PlayCountThisTurn == 0;
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(6, ValueProp.Move)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var hitCount = 1;
        if (ShouldGlowGoldInternal)
            hitCount = 2;
            
        if (ownerCreature == null || Owner?.Character is not Character.Iru iru || play.Target == null)
        {
            await DamageCmd
                .Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(hitCount)
                .FromCard(this, play)
                .Targeting(play.Target).Execute(choiceContext);
            return;
        }
        AudioHelper.PlayRandomAttack();
        await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithNoAttackerAnim()
            .WithWaitBeforeHit(0.25f, 0.25f)
            .WithHitFx(null, null)
            .WithHitVfxNode(target =>
            {
                // This callback runs for each hit, so Iru fires once
                // for every hit resolved by WithHitCount.
                
                iru.PlayAnimation(
                    ownerCreature,
                    "attack", true);

                SfxCmd.Play(
                    "res://Iru/sfx/Photon_rifle_fire.wav");

                _ = iru.PlayProjectile(
                    ownerCreature,
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
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}