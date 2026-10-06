using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using Iru.IruCode.Extensions;
using Iru.IruCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace Iru.IruCode.Mechanics;

public partial class IruResourceDisplay : Control
{
    public static IruResourceDisplay? Instance
    {
        get;
        private set;
    }

    private const string ScenePath =
        "res://Iru/scenes/heat_display.tscn";

    private const string HyperBlueTexturePath =
        "res://Iru/images/charui/hyper_blue.png";

    private const string HyperBlueHalfTexturePath =
        "res://Iru/images/charui/hyper_blue_half.png";

    private const string HyperGreenTexturePath =
        "res://Iru/images/charui/hyper_green.png";

    private const string HyperGreenHalfTexturePath =
        "res://Iru/images/charui/hyper_green_half.png";

    private const string HyperYellowTexturePath =
        "res://Iru/images/charui/hyper_yellow.png";

    private const string HyperYellowHalfTexturePath =
        "res://Iru/images/charui/hyper_yellow_half.png";

    private Control? _display;

    private TextureRect? _overlay;

    private TextureRect? _hyperContainer1;
    private TextureRect? _hyperContainer2;
    private TextureRect? _hyperContainer3;

    private TextureRect? _heat;

    private TextureRect? _hyper1;
    private TextureRect? _hyper2;
    private TextureRect? _hyper3;

    private Control? _hyperHover;
    private Control? _heatHover;

    private RichTextLabel? _heatLabel;

    private Texture2D? _hyperBlueTexture;
    private Texture2D? _hyperBlueHalfTexture;

    private Texture2D? _hyperGreenTexture;
    private Texture2D? _hyperGreenHalfTexture;

    private Texture2D? _hyperYellowTexture;
    private Texture2D? _hyperYellowHalfTexture;

    private Player? _player;

    private int _lastHyper =
        int.MinValue;

    private int _lastHeat =
        int.MinValue;

    private Tween? _hyper1Tween;
    private Tween? _hyper2Tween;
    private Tween? _hyper3Tween;

    private IHoverTip? _hyperHoverTip;
    private IHoverTip? _heatHoverTip;

    private Control? _activeHyperTip;

    private List<IruHoverTipText.TextTarget> _hyperTipTextTargets =
        [];

    private int _renderedTipHyper =
        int.MinValue;

    private int _renderedTipMaxHyper =
        int.MinValue;

    public override void _Ready()
    {
        Instance =
            this;

        Name =
            "IruResourceDisplay";

        MouseFilter =
            MouseFilterEnum.Pass;

        CallDeferred(
            nameof(Setup));
    }

    private async void Setup()
    {
        if (!IsInsideTree())
            return;

        /*
         * Wait until the local combat player exists.
         */
        for (int i = 0; i < 60; i++)
        {
            var state =
                CombatManager.Instance?
                    .DebugOnlyGetState();

            Player? player =
                state?.Players.FirstOrDefault(
                    currentPlayer =>
                        LocalContext.IsMe(currentPlayer));

            if (player != null)
            {
                if (player.Character is not Character.Iru)
                {
                    QueueFree();
                    return;
                }

                _player =
                    player;

                break;
            }

            SceneTree? tree =
                GetTree();

            if (tree == null)
                return;

            await ToSignal(
                tree,
                SceneTree.SignalName.ProcessFrame);
        }

        if (_player == null)
        {
            QueueFree();
            return;
        }

        PackedScene? scene =
            GD.Load<PackedScene>(
                ScenePath);

        if (scene == null)
        {
            GD.PushError(
                $"[Iru Resources] Failed to load {ScenePath}");

            QueueFree();
            return;
        }

        _display =
            scene.Instantiate<Control>();

        AddChild(
            _display);

        _display.MouseFilter =
            MouseFilterEnum.Pass;

        _display.SetAnchorsPreset(
            LayoutPreset.BottomLeft);

        /*
         * Same general location as Riku's HUD.
         *
         * Adjust these values after testing the actual
         * scene dimensions in combat.
         */
        _display.Position =
            new Vector2(
                -115f,
                -40f);

        GetSceneNodes();

        if (!ValidateRequiredNodes())
        {
            QueueFree();
            return;
        }

        LoadTextures();
        ConfigureStaticNodes();
        ConfigureHoverAreas();
        ConfigureHyperNodes();

        ResetDisplayState();
        UpdateDisplay();
    }

    private void GetSceneNodes()
    {
        if (_display == null)
            return;

        _overlay =
            _display.GetNodeOrNull<TextureRect>(
                "Overlay");

        _hyperContainer1 =
            _display.GetNodeOrNull<TextureRect>(
                "hyper_container");

        _hyperContainer2 =
            _display.GetNodeOrNull<TextureRect>(
                "hyper_container2");

        _hyperContainer3 =
            _display.GetNodeOrNull<TextureRect>(
                "hyper_container3");

        _heat =
            _display.GetNodeOrNull<TextureRect>(
                "heat");

        _hyper1 =
            _display.GetNodeOrNull<TextureRect>(
                "hyper1");

        _hyper2 =
            _display.GetNodeOrNull<TextureRect>(
                "hyper2");

        _hyper3 =
            _display.GetNodeOrNull<TextureRect>(
                "hyper3");

        _hyperHover =
            _display.GetNodeOrNull<Control>(
                "hyper_hover");

        _heatHover =
            _display.GetNodeOrNull<Control>(
                "heat_hover");

        _heatLabel =
            _display.GetNodeOrNull<RichTextLabel>(
                "RichTextLabel");
    }

    private bool ValidateRequiredNodes()
    {
        if (_display == null)
            return false;

        bool valid =
            _overlay != null &&
            _hyperContainer1 != null &&
            _hyperContainer2 != null &&
            _hyperContainer3 != null &&
            _heat != null &&
            _hyper1 != null &&
            _hyper2 != null &&
            _hyper3 != null &&
            _hyperHover != null &&
            _heatHover != null &&
            _heatLabel != null;

        if (!valid)
        {
            GD.PushError(
                "[Iru Resources] heat_display.tscn must contain: " +
                "Overlay, hyper_container, hyper_container2, " +
                "hyper_container3, heat, hyper1, hyper2, hyper3, " +
                "hyper_hover, heat_hover, and RichTextLabel.");
        }

        return valid;
    }

    private void LoadTextures()
    {
        _hyperBlueTexture =
            GD.Load<Texture2D>(
                HyperBlueTexturePath);

        _hyperBlueHalfTexture =
            GD.Load<Texture2D>(
                HyperBlueHalfTexturePath);

        _hyperGreenTexture =
            GD.Load<Texture2D>(
                HyperGreenTexturePath);

        _hyperGreenHalfTexture =
            GD.Load<Texture2D>(
                HyperGreenHalfTexturePath);

        _hyperYellowTexture =
            GD.Load<Texture2D>(
                HyperYellowTexturePath);

        _hyperYellowHalfTexture =
            GD.Load<Texture2D>(
                HyperYellowHalfTexturePath);

        if (_hyperBlueTexture == null)
        {
            GD.PushError(
                $"[Iru Resources] Failed to load {HyperBlueTexturePath}");
        }

        if (_hyperBlueHalfTexture == null)
        {
            GD.PushError(
                $"[Iru Resources] Failed to load {HyperBlueHalfTexturePath}");
        }

        if (_hyperGreenTexture == null)
        {
            GD.PushError(
                $"[Iru Resources] Failed to load {HyperGreenTexturePath}");
        }

        if (_hyperGreenHalfTexture == null)
        {
            GD.PushError(
                $"[Iru Resources] Failed to load {HyperGreenHalfTexturePath}");
        }

        if (_hyperYellowTexture == null)
        {
            GD.PushError(
                $"[Iru Resources] Failed to load {HyperYellowTexturePath}");
        }

        if (_hyperYellowHalfTexture == null)
        {
            GD.PushError(
                $"[Iru Resources] Failed to load {HyperYellowHalfTexturePath}");
        }
    }

    private void ConfigureStaticNodes()
    {
        /*
         * These nodes are always visible.
         */
        if (_overlay != null)
            _overlay.Visible = true;

        if (_hyperContainer1 != null)
            _hyperContainer1.Visible = true;

        if (_hyperContainer2 != null)
            _hyperContainer2.Visible = true;

        if (_hyperContainer3 != null)
            _hyperContainer3.Visible = true;

        if (_heat != null)
            _heat.Visible = true;

        if (_heatLabel != null)
        {
            _heatLabel.Visible = true;
            _heatLabel.MouseFilter = MouseFilterEnum.Ignore;
        }
        
        var font = GD.Load<Font>(
            "res://themes/kreon_bold_shared.tres");

        if (font != null)
        {
            _heatLabel.AddThemeFontOverride(
                "font",
                font);

            _heatLabel.AddThemeFontOverride(
                "normal_font",
                font);
        }

        _heatLabel.AddThemeColorOverride(
            "default_color",
            Colors.White);

        _heatLabel.AddThemeColorOverride(
            "font_outline_color",
            new Color(0.2f, 0.2f, 0.2f));

        _heatLabel.AddThemeConstantOverride(
            "outline_size",
            7);

        _heatLabel.AddThemeFontSizeOverride(
            "normal_font_size",
            20);
        
        _heatLabel.Position += new Vector2(-15, 15);
    }

    private void ConfigureHyperNodes()
    {
        if (_hyper1 != null)
        {
            _hyper1.MouseFilter =
                MouseFilterEnum.Ignore;

            _hyper1.PivotOffset =
                _hyper1.Size / 2f;
        }

        if (_hyper2 != null)
        {
            _hyper2.MouseFilter =
                MouseFilterEnum.Ignore;

            _hyper2.PivotOffset =
                _hyper2.Size / 2f;
        }

        if (_hyper3 != null)
        {
            _hyper3.MouseFilter =
                MouseFilterEnum.Ignore;

            _hyper3.PivotOffset =
                _hyper3.Size / 2f;
        }
    }

    private void ConfigureHoverAreas()
    {
        /*
         * These should be transparent Control nodes
         * covering their corresponding display regions.
         */
        if (_hyperHover != null)
        {
            _hyperHover.MouseFilter =
                MouseFilterEnum.Stop;

            _hyperHover.MouseEntered +=
                OnHyperHovered;

            _hyperHover.MouseExited +=
                OnHyperUnhovered;
        }

        if (_heatHover != null)
        {
            _heatHover.MouseFilter =
                MouseFilterEnum.Stop;

            _heatHover.MouseEntered +=
                OnHeatHovered;

            _heatHover.MouseExited +=
                OnHeatUnhovered;
        }

        _hyperHoverTip =
            IruStaticHoverTips.Hyper;

        _heatHoverTip =
            IruStaticHoverTips.Heat;
    }

    public override void _Process(
        double delta)
    {
        UpdateDisplay();
        RenderHyperTipValues();
    }

    private void UpdateDisplay()
    {
        if (_player == null)
            return;

        if (_hyper1 == null ||
            _hyper2 == null ||
            _hyper3 == null ||
            _heatLabel == null)
        {
            return;
        }

        IruResourceRelicBase? relic =
            _player.Relics
                .OfType<IruResourceRelicBase>()
                .FirstOrDefault();

        if (relic == null)
        {
            SetHyperDisplay(
                0);

            SetHeatDisplay(
                0);

            return;
        }

        int hyper =
            Mathf.Clamp(
                relic.GetHyperForUI(),
                0,
                IruResourceRelicBase.MaxHyper);

        int heat =
            Mathf.Clamp(
                relic.GetHeatForUI(),
                0,
                IruResourceRelicBase.MaxHeat);

        if (hyper != _lastHyper)
        {
            SetHyperDisplay(
                hyper);
        }

        if (heat != _lastHeat)
        {
            SetHeatDisplay(
                heat);
        }
    }

   private void SetHyperDisplay(
    int hyper)
{
    int previousHyper =
        _lastHyper;

    _lastHyper =
        Mathf.Clamp(
            hyper,
            0,
            IruResourceRelicBase.MaxHyper);

    if (_lastHyper < 50)
    {
        SetHyperOrb(
            _hyper1!,
            false,
            null,
            ref _hyper1Tween);

        SetHyperOrb(
            _hyper2!,
            false,
            null,
            ref _hyper2Tween);

        SetHyperOrb(
            _hyper3!,
            false,
            null,
            ref _hyper3Tween);
    }
    else if (_lastHyper < 100)
    {
        SetHyperOrb(
            _hyper1!,
            true,
            _hyperBlueHalfTexture,
            ref _hyper1Tween);

        SetHyperOrb(
            _hyper2!,
            false,
            null,
            ref _hyper2Tween);

        SetHyperOrb(
            _hyper3!,
            false,
            null,
            ref _hyper3Tween);
    }
    else if (_lastHyper < 150)
    {
        SetHyperOrb(
            _hyper1!,
            true,
            _hyperBlueTexture,
            ref _hyper1Tween);

        SetHyperOrb(
            _hyper2!,
            false,
            null,
            ref _hyper2Tween);

        SetHyperOrb(
            _hyper3!,
            false,
            null,
            ref _hyper3Tween);
    }
    else if (_lastHyper < 200)
    {
        SetHyperOrb(
            _hyper1!,
            true,
            _hyperBlueTexture,
            ref _hyper1Tween);

        SetHyperOrb(
            _hyper2!,
            true,
            _hyperGreenHalfTexture,
            ref _hyper2Tween);

        SetHyperOrb(
            _hyper3!,
            false,
            null,
            ref _hyper3Tween);
    }
    else if (_lastHyper < 250)
    {
        /*
         * At 200 Hyper, both active orbs become green.
         */
        SetHyperOrb(
            _hyper1!,
            true,
            _hyperGreenTexture,
            ref _hyper1Tween);

        SetHyperOrb(
            _hyper2!,
            true,
            _hyperGreenTexture,
            ref _hyper2Tween);

        SetHyperOrb(
            _hyper3!,
            false,
            null,
            ref _hyper3Tween);
    }
    else if (_lastHyper < 300)
    {
        SetHyperOrb(
            _hyper1!,
            true,
            _hyperGreenTexture,
            ref _hyper1Tween);

        SetHyperOrb(
            _hyper2!,
            true,
            _hyperGreenTexture,
            ref _hyper2Tween);

        SetHyperOrb(
            _hyper3!,
            true,
            _hyperYellowHalfTexture,
            ref _hyper3Tween);
    }
    else
    {
        /*
         * At 300 Hyper, all three orbs become yellow.
         */
        SetHyperOrb(
            _hyper1!,
            true,
            _hyperYellowTexture,
            ref _hyper1Tween);

        SetHyperOrb(
            _hyper2!,
            true,
            _hyperYellowTexture,
            ref _hyper2Tween);

        SetHyperOrb(
            _hyper3!,
            true,
            _hyperYellowTexture,
            ref _hyper3Tween);
    }

    PulseCrossedThresholds(
        previousHyper,
        _lastHyper);
}
   
private void PulseCrossedThresholds(
    int previousHyper,
    int currentHyper)
{
    if (previousHyper < 0)
        return;

    if (CrossedThreshold(
            previousHyper,
            currentHyper,
            50))
    {
        PulseHyperOrb(
            _hyper1!,
            ref _hyper1Tween);

        return;
    }

    if (CrossedThreshold(
            previousHyper,
            currentHyper,
            100))
    {
        PulseHyperOrb(
            _hyper1!,
            ref _hyper1Tween);

        return;
    }

    if (CrossedThreshold(
            previousHyper,
            currentHyper,
            150))
    {
        PulseHyperOrb(
            _hyper2!,
            ref _hyper2Tween);

        return;
    }

    if (CrossedThreshold(
            previousHyper,
            currentHyper,
            200))
    {
        PulseHyperOrb(
            _hyper1!,
            ref _hyper1Tween);

        PulseHyperOrb(
            _hyper2!,
            ref _hyper2Tween);

        return;
    }

    if (CrossedThreshold(
            previousHyper,
            currentHyper,
            250))
    {
        PulseHyperOrb(
            _hyper3!,
            ref _hyper3Tween);

        return;
    }

    if (CrossedThreshold(
            previousHyper,
            currentHyper,
            300))
    {
        PulseHyperOrb(
            _hyper1!,
            ref _hyper1Tween);

        PulseHyperOrb(
            _hyper2!,
            ref _hyper2Tween);

        PulseHyperOrb(
            _hyper3!,
            ref _hyper3Tween);
    }
}

private static bool CrossedThreshold(
    int previousHyper,
    int currentHyper,
    int threshold)
{
    return previousHyper < threshold &&
           currentHyper >= threshold;
}


    private static void SetHyperOrb(
        TextureRect orb,
        bool visible,
        Texture2D? texture,
        ref Tween? tween)
    {
        if (!visible)
        {
            tween?.Kill();
            tween = null;

            orb.Visible = false;
            orb.Scale = Vector2.One;

            return;
        }

        orb.Texture =
            texture;

        orb.Visible =
            true;
    }

    private void PulseHyperOrb(
        TextureRect orb,
        ref Tween? tween)
    {
        orb.PivotOffset =
            orb.Size / 2f;

        orb.Scale =
            Vector2.One;

        tween =
            CreateTween();

        tween.TweenProperty(
                orb,
                "scale",
                new Vector2(
                    1.22f,
                    1.22f),
                0.06f)
            .SetEase(
                Tween.EaseType.Out);

        tween.TweenProperty(
                orb,
                "scale",
                Vector2.One,
                0.18f)
            .SetTrans(
                Tween.TransitionType.Back)
            .SetEase(
                Tween.EaseType.Out);
    }

    private void SetHeatDisplay(
        int heat)
    {
        _lastHeat =
            Mathf.Clamp(
                heat,
                0,
                IruResourceRelicBase.MaxHeat);

        if (_heatLabel == null)
            return;
        
        _heatLabel.Text =
            $"[center]{_lastHeat}%[/center]";
    }

    // -------------------------
    // Hyper hover tip
    // -------------------------

    private void OnHyperHovered()
    {
        if (_hyperHover == null ||
            _hyperHoverTip == null)
        {
            return;
        }

        NHoverTipSet.Clear();

        Control? tip =
            NHoverTipSet.CreateAndShow(
                _hyperHover,
                _hyperHoverTip);

        if (tip == null)
            return;

        tip.GlobalPosition =
            _hyperHover.GlobalPosition +
            new Vector2(
                40f,
                -440f);

        tip.MouseFilter =
            MouseFilterEnum.Ignore;

        TrackHyperTip(
            tip);
    }

    private void OnHyperUnhovered()
    {
        if (_hyperHover != null)
        {
            NHoverTipSet.Remove(
                _hyperHover);
        }

        ForgetHyperTip();
    }

    private void TrackHyperTip(
        Control tip)
    {
        _activeHyperTip =
            tip;

        _hyperTipTextTargets =
            IruHoverTipText.CollectTargets(
                tip);

        _renderedTipHyper =
            int.MinValue;

        _renderedTipMaxHyper =
            int.MinValue;

        RenderHyperTipValues();

        /*
         * The hover-tip label may be populated on the
         * frame after the hover tip is created.
         */
        Callable.From(() =>
        {
            if (_activeHyperTip != tip)
                return;

            if (!IsInstanceValid(tip))
                return;

            if (_hyperTipTextTargets.Count == 0)
            {
                _hyperTipTextTargets =
                    IruHoverTipText.CollectTargets(
                        tip);
            }

            RenderHyperTipValues();
        }).CallDeferred();
    }

    private void RenderHyperTipValues()
    {
        if (_activeHyperTip == null)
            return;

        if (!IsInstanceValid(_activeHyperTip))
        {
            ForgetHyperTip();
            return;
        }

        if (_hyperTipTextTargets.Count == 0)
            return;

        IruResourceRelicBase? relic =
            _player?.Relics
                .OfType<IruResourceRelicBase>()
                .FirstOrDefault();

        if (relic == null)
            return;

        int hyper =
            relic.GetHyperForUI();

        int maxHyper =
            relic.GetMaxHyperForUI();

        if (hyper == _renderedTipHyper &&
            maxHyper == _renderedTipMaxHyper)
        {
            return;
        }

        _renderedTipHyper =
            hyper;

        _renderedTipMaxHyper =
            maxHyper;

        IruHoverTipText.RenderHyper(
            _hyperTipTextTargets,
            hyper,
            maxHyper);
    }

    private void ForgetHyperTip()
    {
        _activeHyperTip =
            null;

        _hyperTipTextTargets =
            [];

        _renderedTipHyper =
            int.MinValue;

        _renderedTipMaxHyper =
            int.MinValue;
    }

    // -------------------------
    // Heat hover tip
    // -------------------------

    private void OnHeatHovered()
    {
        if (_heatHover == null ||
            _heatHoverTip == null)
        {
            return;
        }

        NHoverTipSet.Clear();

        Control? tip =
            NHoverTipSet.CreateAndShow(
                _heatHover,
                _heatHoverTip);

        if (tip == null)
            return;

        tip.GlobalPosition =
            _heatHover.GlobalPosition +
            new Vector2(
                40f,
                -440f);

        tip.MouseFilter =
            MouseFilterEnum.Ignore;
    }

    private void OnHeatUnhovered()
    {
        if (_heatHover != null)
        {
            NHoverTipSet.Remove(
                _heatHover);
        }
    }

    // -------------------------
    // Reset and cleanup
    // -------------------------

    private void ResetDisplayState()
    {
        _lastHyper =
            int.MinValue;

        _lastHeat =
            int.MinValue;

        _hyper1Tween?.Kill();
        _hyper2Tween?.Kill();
        _hyper3Tween?.Kill();

        _hyper1Tween =
            null;

        _hyper2Tween =
            null;

        _hyper3Tween =
            null;

        if (_hyper1 != null)
        {
            _hyper1.Visible = false;
            _hyper1.Scale = Vector2.One;
        }

        if (_hyper2 != null)
        {
            _hyper2.Visible = false;
            _hyper2.Scale = Vector2.One;
        }

        if (_hyper3 != null)
        {
            _hyper3.Visible = false;
            _hyper3.Scale = Vector2.One;
        }

        if (_heatLabel != null)
        {
            _heatLabel.Text =
                "0%";
        }
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance =
                null;
        }

        _hyper1Tween?.Kill();
        _hyper2Tween?.Kill();
        _hyper3Tween?.Kill();

        _hyper1Tween =
            null;

        _hyper2Tween =
            null;

        _hyper3Tween =
            null;

        if (_hyperHover != null)
        {
            _hyperHover.MouseEntered -=
                OnHyperHovered;

            _hyperHover.MouseExited -=
                OnHyperUnhovered;

            NHoverTipSet.Remove(
                _hyperHover);
        }

        if (_heatHover != null)
        {
            _heatHover.MouseEntered -=
                OnHeatHovered;

            _heatHover.MouseExited -=
                OnHeatUnhovered;

            NHoverTipSet.Remove(
                _heatHover);
        }

        ForgetHyperTip();

        _hyperHoverTip =
            null;

        _heatHoverTip =
            null;

        _player =
            null;

        _overlay =
            null;

        _hyperContainer1 =
            null;

        _hyperContainer2 =
            null;

        _hyperContainer3 =
            null;

        _heat =
            null;

        _hyper1 =
            null;

        _hyper2 =
            null;

        _hyper3 =
            null;

        _hyperHover =
            null;

        _heatHover =
            null;

        _heatLabel =
            null;

        _hyperBlueTexture =
            null;

        _hyperBlueHalfTexture =
            null;

        _hyperGreenTexture =
            null;

        _hyperGreenHalfTexture =
            null;

        _hyperYellowTexture =
            null;

        _hyperYellowHalfTexture =
            null;

        _display =
            null;
    }
}

[HarmonyPatch(
    typeof(NEnergyCounter),
    nameof(NEnergyCounter._Ready))]
public static class IruResourceDisplayOverlayPatch
{
    public static void Postfix(
        NEnergyCounter __instance)
    {
        if (__instance == null)
            return;

        if (!GodotObject.IsInstanceValid(
                __instance))
        {
            return;
        }

        var state =
            CombatManager.Instance?
                .DebugOnlyGetState();

        Player? player =
            state?.Players.FirstOrDefault(
                currentPlayer =>
                    LocalContext.IsMe(currentPlayer));

        if (player?.Character is not Character.Iru)
            return;

        if (__instance.GetNodeOrNull<IruResourceDisplay>(
                "IruResourceDisplay") != null)
        {
            return;
        }

        __instance.AddChild(
            new IruResourceDisplay
            {
                Name =
                    "IruResourceDisplay"
            });
    }
}