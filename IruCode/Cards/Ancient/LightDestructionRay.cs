using BaseLib.Utils;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using Iru.IruCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Ancient;

public class LightDestructionRay() : IruCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(45, ValueProp.Move),
        new PowerVar<VulnerablePower>(2),
        new PowerVar<WeakPower>(2)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var enemies = CombatState.HittableEnemies.ToList();
        var leftMostEnemy = enemies.First();
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Iru iru)
        {
            AudioHelper.PlayRandomAttack();
            await iru.DashTo(Owner.Creature, leftMostEnemy, 0.2F, 350F, false);
            iru.PlayAnimation(ownerCreature, "destruction_ray");
            SfxCmd.Play("res://Iru/sfx/Hyper_attack_activation.wav");
            await Task.Delay((int)(0.3f * 1000f));
            SfxCmd.Play("res://Iru/sfx/Destruction_ray_activation.wav");
            await Task.Delay((int)(0.2f * 1000f));
            SfxCmd.Play("res://Iru/sfx/Linear_ray_fire.wav");
            await Task.Delay((int)(0.2f * 1000f));
            foreach(var enemy in enemies)
                iru.PlayVfxOnTarget(
                    enemy,
                    "res://Iru/scenes/vfx.tscn",
                    "photon_hit"
                );
            await Task.Delay((int)(0.2f * 1000f));
            foreach(var enemy in enemies)
                iru.PlayVfxOnTarget(
                    enemy,
                    "res://Iru/scenes/vfx.tscn",
                    "photon_hit"
                );
            await Task.Delay((int)(0.2f * 1000f));
            foreach(var enemy in enemies)
                iru.PlayVfxOnTarget(
                    enemy,
                    "res://Iru/scenes/vfx.tscn",
                    "photon_hit"
                );
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null, "res://Iru/sfx/explode.wav")
                .Execute(choiceContext);
            await Task.Delay((int)(0.8f * 1000f));
            await iru.Retreat(Owner.Creature, duration: 0.2F);
        }
        else await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Iru/sfx/explode.wav")
            .Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(choiceContext, CombatState.HittableEnemies, DynamicVars.Weak.BaseValue, Owner.Creature,
            this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, CombatState.HittableEnemies, DynamicVars.Vulnerable.BaseValue,
            Owner.Creature, this);
        await PowerCmd.Remove<HyperReadyPower>(Owner.Creature);
        IruResourceManager.SpendHyper(Owner, 300);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(15);
    }
}