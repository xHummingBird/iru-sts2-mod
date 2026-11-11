using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Iru.IruCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Uncommon;

public class HighSpeedPhoton() : IruCard(
    0,
    CardType.Attack,
    CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => CanDrawCard;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            5,
            ValueProp.Move),

        new IntVar(
            "PlayMax",
            3),

        new CardsVar(1)
    ];

    private bool CanDrawCard
    {
        get
        {
            int cardsPlayed =
                CombatManager.Instance.History.CardPlaysFinished.Count(
                    e => e.HappenedThisTurn(CombatState) &&
                         e.CardPlay.Player == Owner);

            return cardsPlayed < DynamicVars["PlayMax"].IntValue;
        }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(
            play.Target);

        if (Owner?.Character is Character.Iru iru)
        {
            AudioHelper.PlayRandomAttack();

            iru.PlayAnimation(
                Owner.Creature,
                "attack",
                true);

            await Task.Delay(
                (int)(0.133f * 1000f));

            SfxCmd.Play(
                "res://Iru/sfx/Photon_rifle_fire.wav");

            await iru.PlayProjectile(
                Owner.Creature,
                play.Target,
                "res://Iru/scenes/vfx.tscn",
                animationName: "photon_rifle",
                spawnOffset: new Vector2(120, -80),
                stopDistance: 130f,
                duration: 0.1f
            );

            iru.PlayVfxOnTarget(
                play.Target,
                "res://Iru/scenes/vfx.tscn",
                "photon_hit"
            );
        }

        await DamageCmd.Attack(
                DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithNoAttackerAnim()
            .WithHitFx(null, null)
            .Execute(choiceContext);

        if (CanDrawCard)
        {
            await CardPileCmd.Draw(
                choiceContext,
                DynamicVars.Cards.BaseValue,
                Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2); // 5 -> 7
    }
}
