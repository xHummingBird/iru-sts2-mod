using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Rare;

public class PiercingShot() : IruCard(
    2,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AllEnemies), IHeatCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(22, ValueProp.Move),
        new DynamicVar("Heat", 40)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IruStaticHoverTips.Heat
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        Creature? backEnemy =
            CombatState.HittableEnemies.LastOrDefault();

        if (Owner?.Character is Character.Iru iru &&
            backEnemy != null)
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
                backEnemy,
                "res://Iru/scenes/vfx.tscn",
                animationName: "photon_rifle",
                spawnOffset: new Vector2(120, -80),
                stopDistance: 130f,
                duration: 0.1f
            );
            
            foreach (var enemy in CombatState.HittableEnemies)
                iru.PlayVfxOnTarget(
                    enemy,
                    "res://Iru/scenes/vfx.tscn",
                    "photon_hit"
                );
        }

        await DamageCmd.Attack(
                DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithNoAttackerAnim()
            .WithHitFx(null, null)
            .Execute(choiceContext);

        IruResourceManager.GainHeat(
            Owner,
            DynamicVars["Heat"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);
    }
}