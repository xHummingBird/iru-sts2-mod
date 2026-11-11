using BaseLib.Utils;
using Godot;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using Iru.IruCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Common;

public class WarningShot() : IruCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(8, ValueProp.Move),
        new DynamicVar("Hyper", 20)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<HyperReadyPower>(),
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
        IruResourceManager.GainHyper(Owner, DynamicVars["Hyper"].IntValue);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["Hyper"].UpgradeValueBy(10);
    }
}