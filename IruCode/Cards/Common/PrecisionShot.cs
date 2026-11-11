using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Iru.IruCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Common;

public class PrecisionShot() : IruCard(
    1,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal =>
        CombatState?.HittableEnemies?.Count == 1;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            7,
            ValueProp.Move)
    ];

    private bool IsOnlyEnemyRemaining =>
        CombatState?.HittableEnemies?.Count == 1;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(
            play.Target);

        int hitCount =
            IsOnlyEnemyRemaining
                ? 2
                : 1;

        if (Owner?.Character is Character.Iru iru)
        {
            AudioHelper.PlayRandomAttack();

            await DamageCmd.Attack(
                    DynamicVars.Damage.BaseValue)
                .WithHitCount(hitCount)
                .FromCard(this, play)
                .Targeting(play.Target)
                .WithNoAttackerAnim()
                .WithWaitBeforeHit(0.15f, 0.15f)
                .WithHitVfxNode(target =>
                {
                    iru.PlayAnimation(
                        Owner.Creature,
                        "attack",
                        true);

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

            return;
        }

        await DamageCmd.Attack(
                DynamicVars.Damage.BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this, play)
            .Targeting(play.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}