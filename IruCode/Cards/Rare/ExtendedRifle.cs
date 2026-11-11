using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
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

public class ExtendedRifle() : IruCard(
    1,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(5m),

        new ExtraDamageVar(2m),

        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((card, _) =>
                IruResourceManager.GetHeat(card.Owner) / 15)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IruStaticHoverTips.Heat
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (Owner?.Character is Character.Iru iru &&
            play.Target != null)
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

        await CommonActions
            .CardAttack(
                this,
                play.Target)
            .WithHitFx(
                null,
                null)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(2m);
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}