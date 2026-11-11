using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Rare;

public class MissileBarrage() : IruCard(
    1,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AnyEnemy), IHeatCard
{
    private const string CalculatedHitsKey = "CalculatedHits";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move),
        new DynamicVar("Heat", 30),

        new CalculationBaseVar(0),
        new CalculationExtraVar(1),

        new CalculatedVar(CalculatedHitsKey)
            .WithMultiplier((card, _) =>
                CombatManager.Instance.History.CardPlaysFinished.Count(entry =>
                    entry.HappenedThisTurn(card.CombatState) &&
                    entry.CardPlay.Player == card.Owner &&
                    entry.CardPlay.Card.Type == CardType.Attack))
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IruStaticHoverTips.Heat
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        if (Owner?.Character is not Character.Iru iru)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount((int)((CalculatedVar)base.DynamicVars["CalculatedHits"]).Calculate(play.Target))
                .FromCard(this, play)
                .Targeting(play.Target)
                .Execute(choiceContext);

            return;
        }

        AudioHelper.PlayRandomAttack();

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount((int)((CalculatedVar)base.DynamicVars["CalculatedHits"]).Calculate(play.Target))
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(null, "res://Iru/sfx/fire_impact (2).wav")
            .WithNoAttackerAnim()
            .WithWaitBeforeHit(0.35f, 0.35f)
            .WithHitVfxNode(target =>
            {
                iru.PlayAnimation(
                    Owner.Creature,
                    "attack_4",
                    false);

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
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}