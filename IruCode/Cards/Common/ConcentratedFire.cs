using BaseLib.Utils;
using Godot;
using Iru.IruCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Common;

public class ConcentratedFire() : IruCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(9, ValueProp.Move),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var vulPower = play.Target.GetPowerAmount<VulnerablePower>();
        var weakPower = play.Target.GetPowerAmount<WeakPower>();

        if (ownerCreature != null && Owner?.Character is Character.Iru iru)
        {
            AudioHelper.PlayRandomAttack();
            iru.PlayAnimation(ownerCreature, "attack_2", true);
            
            await Task.Delay((int)(0.133f * 1000f));
            SfxCmd.Play("res://Iru/sfx/Rocket_and_missile_barrage_launch.wav");
            await iru.PlayProjectile(
                Owner.Creature,
                play.Target,
                "res://Iru/scenes/vfx.tscn",
                animationName: "missile_2",
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
        await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, weakPower, Owner.Creature,
            this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, vulPower, Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}