using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Iru.IruCode.Extensions;

public static class IruAssets
{
    private static PackedScene? _iruScene;
    private static PackedScene? _vfxScene;

    private const string IruScenePath = "res://Iru/scenes/iru.tscn";
    private const string VfxPath = "res://Iru/scenes/vfx.tscn";
    
    public static bool IsIruInRun(IRunState runState)
    {
        return runState?.Players?.Any(
            p => p?.Character is Character.Iru
        ) ?? false;
    }
    

    public static PackedScene? IruScene
    {
        get
        {
            _iruScene = LoadOrReload(_iruScene, IruScenePath, "Iru scene");
            return _iruScene;
        }
    }

    public static PackedScene? VfxScene
    {
        get
        {
            _vfxScene = LoadOrReload(_vfxScene, VfxPath, "VFX");
            return _vfxScene;
        }
    }
    
    private static PackedScene? LoadOrReload(PackedScene? cachedScene, string path, string label)
    {
        if (cachedScene != null && GodotObject.IsInstanceValid(cachedScene))
            return cachedScene;

        GD.Print($"IruAssets: Loading {label} from {path}");

        var scene = GD.Load<PackedScene>(path);

        if (scene == null)
        {
            GD.PrintErr($"IruAssets: FAILED to load {label}: {path}");
            return null;
        }

        GD.Print($"IruAssets: Loaded {label}");
        return scene;
    }

    public static void EnsurePreloaded()
    {
        _ = IruScene;
        _ = VfxScene;

        GD.Print("IruAssets: EnsurePreloaded finished");
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterActEntered))]
public static class IruAfterActEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState)
    {
        // var player = runState?.Players?.FirstOrDefault();
        //
        // if (player?.Character is not Character.Iru)
        //     return;
        if (!IruAssets.IsIruInRun(runState))
            return;
        
        GD.Print("AfterActEntered: Iru detected → preloading");

        IruAssets.EnsurePreloaded();
    }
}


[HarmonyPatch(typeof(Hook), nameof(Hook.AfterRoomEntered))]
public static class IruAfterRoomEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState, AbstractRoom room)
    {
        // var player = runState?.Players?.FirstOrDefault();
        //
        // if (player?.Character is not Character.Iru)
        //     return;
        if (!IruAssets.IsIruInRun(runState))
            return;
        
        GD.Print($"AfterRoomEntered: Iru detected → preloading. Room = {room.GetType().Name}");

        IruAssets.EnsurePreloaded();
    }
}
