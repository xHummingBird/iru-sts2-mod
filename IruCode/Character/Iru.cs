using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Iru.IruCode.Extensions;
using Godot;
using HarmonyLib;
using Iru.IruCode.Cards.Basic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.ValueProps;
using PhotonRifle = Iru.IruCode.Relics.PhotonRifle;

namespace Iru.IruCode.Character;


public class Iru : PlaceholderCharacterModel
{
    public const string CharacterId = "Iru";

    public static readonly Color Color = new("ffffff");
    
    private Vector2? _originalPosition;

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 72;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeIru>(),
        ModelDb.Card<StrikeIru>(),
        ModelDb.Card<StrikeIru>(),
        ModelDb.Card<StrikeIru>(),
        ModelDb.Card<StrikeIru>(),
        ModelDb.Card<DefendIru>(),
        ModelDb.Card<DefendIru>(),
        ModelDb.Card<DefendIru>(),
        ModelDb.Card<DefendIru>(),
        ModelDb.Card<DefendIru>(),
        ModelDb.Card<Cards.Basic.PhotonRifle>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<PhotonRifle>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<IruCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<IruRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<IruPotionPool>();

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets.
        These are just some of the simplest assets, given some placeholders to differentiate your character with.
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }
    
    public override CustomEnergyCounter? CustomEnergyCounter =>
        new CustomEnergyCounter(EnergyCounterPaths, new Color(0.2f, 0.2f, 0.2f), new Color(1f, 1f, 1f));
    
    private string EnergyCounterPaths(int i)
    {
        return i switch
        {
            1 => "charui/big_energy.png".ImagePath(),
            _ => "charui/blank.png".ImagePath()
        };
    }

    public override string CustomIconTexturePath => "character_icon_iru.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_iru.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_iru.png".CharacterUiPath();
    
    private const string CustomVisualScenePath = "res://Iru/scenes/iru.tscn";
    public override string CustomRestSiteAnimPath => "res://Iru/scenes/iru_rest_site.tscn";
    
    public override string CustomCharacterSelectBg => "res://Iru/images/charui/char_selection_bg_iru.tscn";
    public override string CustomMerchantAnimPath => "res://Iru/scenes/iru_merchant.tscn";
    public override string CharacterSelectSfx => "res://Iru/sounds/run_start.wav";
    
    public override NCreatureVisuals? CreateCustomVisuals()
    {
        // IruAssets.EnsurePreloaded();
        return NodeFactory<NCreatureVisuals>.CreateFromScene(CustomVisualScenePath);
    }
    
    // public override CreatureAnimator? GenerateAnimator(MegaSprite controller) => null;
    
    public (float total, float[] impacts) PlayAnimation(Creature creature, string trigger)
    {
        if (creature == null || string.IsNullOrEmpty(trigger))
            return (0f, Array.Empty<float>());

        var node = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (node?.Visuals == null)
            return (0f, Array.Empty<float>());

        var animPlayer = node.Visuals.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (animPlayer == null)
            return (0f, Array.Empty<float>());

        string godotTrigger = trigger.ToLowerInvariant() switch
        {
            "hit" => "hurt",
            "idle" => "idle",
            "attack" => "attack_iru",
            "dead" => "die",
            "die" => "die",
            _ => trigger
        };

        if (!animPlayer.HasAnimation(godotTrigger))
            return (0f, Array.Empty<float>());

        var anim = animPlayer.GetAnimation(godotTrigger);
        float totalLength = (float)anim.Length;

        animPlayer.Play(godotTrigger);
        
        if (godotTrigger != "idle" && godotTrigger != "die")
            animPlayer.Queue("idle");
        
        return (totalLength, Array.Empty<float>());
    } 
    
    public async Task DashTo(
        Creature player,
        Creature target,
        float durationSeconds = 0.3f,
        float distance = 200f,
        bool dashBehind = false,
        string? overrideAnim = null)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node == null || targetNode == null) return;

        if (!_originalPosition.HasValue)
            _originalPosition = node.GlobalPosition;
        
        PlayAnimation(player, overrideAnim ??"dash");
		
        bool playerIsLeftOfTarget = node.GlobalPosition.X < targetNode.GlobalPosition.X;
		
        Vector2 offsetDir = playerIsLeftOfTarget ? Vector2.Left : Vector2.Right;
		
        if (dashBehind)
            offsetDir = -offsetDir;

        Vector2 targetPos = targetNode.GlobalPosition + offsetDir * distance;

        var tween = node.CreateTween();
        tween.TweenProperty(node, "global_position", targetPos, durationSeconds)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        await node.ToSignal(tween, Tween.SignalName.Finished);
    }
    
    public async Task DashPast(
        Creature player,
        Creature target,
        string? attackAnim = null,
        float durationSeconds = 0.3f,
        float behindDistance = 200f,
        float overshoot = 0f)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node == null || targetNode == null) return;

        if (!_originalPosition.HasValue)
            _originalPosition = node.GlobalPosition;

        Vector2 frontDir = (player.Side == CombatSide.Player) ? Vector2.Left : Vector2.Right;
        Vector2 behindDir = -frontDir;

        Vector2 endPos = targetNode.GlobalPosition + behindDir * (behindDistance + overshoot);

        PlayAnimation(player, attackAnim);

        var tween = node.CreateTween();
        tween.TweenProperty(node, "global_position", endPos, durationSeconds)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        await node.ToSignal(tween, Tween.SignalName.Finished);
    }
    
    
    public async Task Retreat(
        Creature player,
        string? animation = "retreat",
        bool goIdle = true,
        float duration = 0.3f)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player);
        if (node == null || !_originalPosition.HasValue) return;

        if (!string.IsNullOrEmpty(animation))
            PlayAnimation(player, animation);

        var tween = node.CreateTween();
        tween.TweenProperty(node, "global_position", _originalPosition.Value, duration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.InOut);

        await node.ToSignal(tween, Tween.SignalName.Finished);

        _originalPosition = null;

        var visuals = node.Visuals.GetNodeOrNull<Node2D>("Visuals");
        if (visuals != null)
            visuals.Position = Vector2.Zero;

        if (goIdle)
            PlayAnimation(player, "idle");
    }
    
    public void DoScreenShake(ShakeStrength strength = ShakeStrength.Medium,
        ShakeDuration duration = ShakeDuration.Short)
    {
        NGame.Instance?.ScreenShake(strength, duration);
    }

    public Node2D PlayVfxOnTarget(Creature target, string path, string animName)
    {
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (targetNode?.Visuals == null)
            return null;

        var scene = GD.Load<PackedScene>(path);
        var vfx = scene.Instantiate<Node2D>();

        targetNode.Visuals.AddChild(vfx);
        vfx.Position = Vector2.Zero;

        var animPlayer = vfx.GetNode<AnimationPlayer>("AnimationPlayer");

        if (animPlayer.HasAnimation(animName))
            animPlayer.Play(animName);

        return vfx;
    }
    
    [HarmonyPatch(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))]
    public static class NCreatureSetTriggerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(NCreature __instance, string trigger)
        {
            // This ensures the engine's triggers automatically drive your AnimationPlayer.
            if (__instance.Entity?.Player?.Character is Iru character)
            {
                character.PlayAnimation(__instance.Entity, trigger);
                return false; // skip default skeletal animation path
            }
            return true;
        }
    }
    
    public async Task<Node2D?> PlayProjectile(
        Creature source,
        Creature target,
        string scenePath,
        string? animationName = null,
        Vector2? spawnOffset = null,
        float stopDistance = 0f,
        float duration = 0.3f,
        bool faceTarget = false)
    {
        var sourceNode = NCombatRoom.Instance?.GetCreatureNode(source);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);

        if (sourceNode == null || targetNode == null)
            return null;

        var scene = GD.Load<PackedScene>(scenePath);

        if (scene == null)
            return null;

        var projectile = scene.Instantiate<Node2D>();

        NCombatRoom.Instance.AddChild(projectile);

        Vector2 startPos =
            sourceNode.GlobalPosition +
            (spawnOffset ?? Vector2.Zero);

        Vector2 targetPos =
            targetNode.GlobalPosition;

        Vector2 direction =
            (targetPos - startPos).Normalized();

        Vector2 endPos =
            targetPos - direction * stopDistance;

        projectile.GlobalPosition = startPos;

        if (faceTarget)
            projectile.GlobalRotation = direction.Angle();

        var animPlayer =
            projectile.GetNodeOrNull<AnimationPlayer>(
                "AnimationPlayer");

        if (animPlayer != null &&
            !string.IsNullOrEmpty(animationName) &&
            animPlayer.HasAnimation(animationName))
        {
            animPlayer.Play(animationName);
        }

        var tween = projectile.CreateTween();

        tween.TweenProperty(
                projectile,
                "global_position",
                endPos,
                duration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        await projectile.ToSignal(
            tween,
            Tween.SignalName.Finished);

        return projectile;
    }

    [HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
    public static class IruStartDeathAnimPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NCreature __instance, ref float __result)
        {
            if (__instance.Entity?.Player?.Character is Iru character)
            {
                AudioHelper.PlayRandomGameover();
                character.PlayAnimation(__instance.Entity, "die");
                var animPlayer = __instance.Visuals.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
                __result = animPlayer?.GetAnimation("die")?.Length ?? 1.5f;
            }
        }
    }
    
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatVictory))]
    public static class IruVictoryAnimationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(IRunState runState, CombatState? combatState)
        {
            var creatures = combatState?.Creatures?.Where(c => c.IsPlayer);

            if (creatures == null)
                return;

            foreach (var creature in creatures)
            {
                if (creature.Player?.Character is not Iru)
                    continue;

                var node = NCombatRoom.Instance?.GetCreatureNode(creature);
                var animPlayer = node?.Visuals?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
                
                if (animPlayer == null)
                    continue;
                AudioHelper.PlayRandomVictory();
                animPlayer.Play("victory");
            }
        }
    }
    
   [HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageReceived))]
    public static class IruDamageAnimationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Creature target, DamageResult result, ValueProp props, Creature? dealer)
        {
            if (target.Player?.Character is not Iru character)
                return;
            
            if (dealer == null || dealer.Side != CombatSide.Enemy)
                return;
            
            if (props.HasFlag(ValueProp.SkipHurtAnim) || props.HasFlag(ValueProp.Unpowered))
                return;

            if (result.WasFullyBlocked && result.BlockedDamage > 0)
            {
                character.PlayAnimation(target, "block"); 
            }
            
            else if (result.UnblockedDamage > 0 && !target.IsDead)
            {
                character.PlayAnimation(target, "hit");
                if (target.CurrentHp < 20)
                {
                    AudioHelper.PlayRandomDamagedCritical();
                }
                else if (result.UnblockedDamage < 10)
                {
                    AudioHelper.PlayRandomDamaged();
                }
                else
                {
                    AudioHelper.PlayRandomDamagedHigh();
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromChooseACardScreen))]
    public static class IruCardSelectCmdFromChooseACardScreenPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            PlayerChoiceContext context,
            IReadOnlyList<CardModel> cards,
            Player player,
            bool canSkip,
            ref Task<CardModel?> __result)
        {
            __result = PatchedChoose(context, cards, player, canSkip);
            return false; 
        }
    
        private static async Task<CardModel?> PatchedChoose(
            PlayerChoiceContext context,
            IReadOnlyList<CardModel> cards,
            Player player,
            bool canSkip)
        {
            if (cards.Count > 5)
            {
                throw new ArgumentException("Only works with 5 or fewer cards", nameof(cards));
            }
    
            if (cards.Count == 0)
            {
                return null;
            }
    
            uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
    
            await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.None);
    
            CardModel? result;
			 
            if (LocalContext.IsMe(player))
            {
                NPlayerHand.Instance?.CancelAllCardPlay();
    
                var screen = NChooseACardSelectionScreen.ShowScreen(cards, canSkip);
    
                if (screen == null)
                {
                    await context.SignalPlayerChoiceEnded();
                    return null;
                }
				
                foreach (var card in cards)
                {
                    SaveManager.Instance.MarkCardAsSeen(card);
                }
    
                result = (await screen.CardsSelected()).FirstOrDefault();
    
                int index = cards.IndexOf(result);
                var choiceResult = PlayerChoiceResult.FromIndex(index);
    
                RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, choiceResult);
            }
            else
            {
                int index = (await RunManager.Instance.PlayerChoiceSynchronizer
                        .WaitForRemoteChoice(player, choiceId))
                    .AsIndex();
    
                result = index < 0 ? null : cards[index];
            }
    
            await context.SignalPlayerChoiceEnded();
    
            return result;
        }
    }
}