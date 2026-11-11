using BaseLib.Utils;
using Godot;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Common;

public class PocketMissile() : IruCard(0, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy), IHeatCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(8, ValueProp.Move),
        new DynamicVar("Heat", 30)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IruStaticHoverTips.Heat
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Iru iru)
        {
            AudioHelper.PlayRandomAttack();
            float duration = iru.PlayAnimation(ownerCreature, "attack_2", true).total;
            
            await Task.Delay((int)(0.133f * 1000f));
            SfxCmd.Play("res://Iru/sfx/Rocket_and_missile_barrage_launch.wav");
            await iru.PlayProjectile(
                Owner.Creature,
                play.Target,
                "res://Iru/scenes/vfx.tscn",
                animationName: "missile_1",
                spawnOffset: new Vector2(100, -80),
                stopDistance: 110f,
                duration: 0.2F
            );
            iru.PlayVfxOnTarget(
                play.Target,
                "res://Iru/scenes/vfx.tscn",
                "photon_hit"
            );
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Iru/sfx/fire_impact (2).wav")
            .Execute(choiceContext);
        IruResourceManager.GainHeat(Owner, DynamicVars["Heat"].IntValue);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Heat"].UpgradeValueBy(-10);
    }
}