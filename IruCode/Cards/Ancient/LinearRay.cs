using BaseLib.Extensions;
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

public class LinearRay() : IruCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(15, ValueProp.Move),
        new PowerVar<WeakPower>(1)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        var ownerCreature = Owner?.Creature;
        var enemies = CombatState.HittableEnemies.ToList();
        var leftMostEnemy = enemies.First();
        if (ownerCreature != null && Owner?.Character is Character.Iru iru)
        {
            AudioHelper.PlayRandomAttack();
            await iru.DashTo(Owner.Creature, leftMostEnemy, 0.2F, 350F, false);
            float duration = iru.PlayAnimation(ownerCreature, "linear_ray", true).total;
            SfxCmd.Play("res://Iru/sfx/Hyper_attack_activation.wav");
            await Task.Delay((int)(0.4f * 1000f));
            SfxCmd.Play("res://Iru/sfx/Linear_ray_fire.wav");
            await Task.Delay((int)(0.3f * 1000f));
            foreach(var enemy in enemies)
                iru.PlayVfxOnTarget(
                    enemy,
                    "res://Iru/scenes/vfx.tscn",
                    "photon_hit"
                );
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null, "res://Iru/sfx/explode.wav")
                .Execute(choiceContext);
            await Task.Delay((int)(0.6f * 1000f));
            await iru.Retreat(Owner.Creature, duration: 0.2F);
        }
        else await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Iru/sfx/explode.wav")
            .Execute(choiceContext);

        await PowerCmd.Apply<WeakPower>(choiceContext, CombatState.HittableEnemies, DynamicVars.Weak.BaseValue, Owner.Creature,
            this);
        await PowerCmd.Remove<HyperReadyPower>(Owner.Creature);
        IruResourceManager.SpendHyper(Owner, 100);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
        DynamicVars.Weak.UpgradeValueBy(1);
    }
}