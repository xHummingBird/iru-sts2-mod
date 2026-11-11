using BaseLib.Extensions;
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
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Ancient;

public class GenocideBomb() : IruCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(30m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((card, _) =>
                IruResourceManager.GetHeat(card.Owner) / 10)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        var ownerCreature = Owner?.Creature;
        var selfDamage = DynamicVars.CalculatedDamage.PreviewValue/5;
        if (ownerCreature != null && Owner?.Character is Character.Iru iru)
        {
            AudioHelper.PlayRandomAttack();
            float duration = iru.PlayAnimation(ownerCreature, "attack_2", true).total;
            SfxCmd.Play("res://Iru/sfx/Hyper_attack_activation.wav");
            await Task.Delay((int)(0.133f * 1000f));
            SfxCmd.Play("res://Iru/sfx/Rocket_and_missile_barrage_launch.wav");
            await iru.PlayProjectile(
                Owner.Creature,
                play.Target,
                "res://Iru/scenes/vfx.tscn",
                animationName: "missile_3",
                spawnOffset: new Vector2(100, -80),
                stopDistance: 110f,
                duration: 0.4F
            );
            iru.PlayVfxOnTarget(
                play.Target,
                "res://Iru/scenes/vfx.tscn",
                "genocide_bomb"
            );
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Iru/sfx/meteostrike_3.wav")
            .Execute(choiceContext);
        await DamageCmd.Attack(selfDamage).WithHitCount(1).FromCard(this, play).Targeting(Owner.Creature).WithValueProp(ValueProp.Unpowered).Execute(choiceContext);
        await PowerCmd.Remove<HyperReadyPower>(Owner.Creature);
        IruResourceManager.SpendHyper(Owner, 200);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(10);
    }
}