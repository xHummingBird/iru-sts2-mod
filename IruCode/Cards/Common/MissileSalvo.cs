using Godot;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Cards.Common;

public class MissileSalvo() : IruCard(2, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(6, ValueProp.Move),
        new HitCountVar(2)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (Owner?.Character is not Character.Iru iru)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(DynamicVars.HitCount.IntValue)
                .FromCard(this, play)
                .Targeting(play.Target)
                .Execute(choiceContext);
        }
        else
        {
            AudioHelper.PlayRandomAttack();
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(DynamicVars.HitCount.IntValue)
                .FromCard(this, play)
                .Targeting(play.Target)
                .WithHitFx(null, "res://Iru/sfx/fire_impact (2).wav")
                .WithNoAttackerAnim()
                .WithWaitBeforeHit(0.35f, 0.35f)
                .WithHitVfxNode(target =>
                {
                    iru.PlayAnimation(
                        Owner.Creature,
                        "attack_2", true);

                    SfxCmd.Play(
                        "res://Iru/sfx/Rocket_and_missile_barrage_launch.wav");

                    _ = iru.PlayProjectile(
                        Owner.Creature,
                        target,
                        "res://Iru/scenes/vfx.tscn",
                        animationName: "missile_1",
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
        }

        CardModel? cardModel = PileType.Draw.GetPile(base.Owner).Cards.Where((CardModel c) => c.Type == CardType.Attack && !c.Keywords.Contains(CardKeyword.Unplayable)).ToList().StableShuffle(base.Owner.RunState.Rng.Shuffle)
            .FirstOrDefault();
        if (cardModel == null)
        {
            cardModel = PileType.Draw.GetPile(base.Owner).Cards.Where((CardModel c) => c.Type == CardType.Attack).ToList().StableShuffle(base.Owner.RunState.Rng.Shuffle)
                .FirstOrDefault();
        }
        if (cardModel != null)
        {
            if (IsUpgraded)
            {
                CardCmd.Upgrade(cardModel);
            }
            
            await CardCmd.AutoPlay(choiceContext, cardModel, null);
        }
    }
    
    protected override void OnUpgrade()
    {
        
    }
}