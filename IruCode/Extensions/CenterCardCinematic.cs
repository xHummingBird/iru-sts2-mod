using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Iru.IruCode.Extensions;

public static class CenterCardCinematic
{
    private static bool _isHidden = false;

    public static void Start(ulong netId)
    {
        if (!IsLocalPlayer(netId)) return;
        if (_isHidden) return;

        SetHidden(true);
        _isHidden = true;
    }

    public static void End(ulong netId)
    {
        if (!IsLocalPlayer(netId)) return;
        if (!_isHidden) return;

        SetHidden(false);
        _isHidden = false;
    }

    private static bool IsLocalPlayer(ulong netId)
    {
        return RunManager.Instance != null
               && RunManager.Instance.NetService != null
               && netId == RunManager.Instance.NetService.NetId;
    }

    private static void SetHidden(bool hidden)
    {
        var combatRoom = NCombatRoom.Instance;
        var playContainer = combatRoom?.Ui?.PlayContainer;

        if (combatRoom == null || playContainer == null)
            return;

        var localPlayerNode = combatRoom.CreatureNodes.FirstOrDefault(
            creature => creature.Entity.IsPlayer &&
                        LocalContext.IsMe(creature.Entity)
        );

        var stateDisplay = localPlayerNode?
            .GetNodeOrNull<NCreatureStateDisplay>("%HealthBar");

        var targetColor = hidden
            ? Colors.Transparent
            : Colors.White;

        var tween = playContainer.CreateTween()
            .SetParallel();

        tween.TweenProperty(
            playContainer,
            "modulate",
            targetColor,
            0.1f
        );

        if (stateDisplay != null)
        {
            tween.TweenProperty(
                stateDisplay,
                "modulate",
                targetColor,
                0.1f
            );
        }
    }
}