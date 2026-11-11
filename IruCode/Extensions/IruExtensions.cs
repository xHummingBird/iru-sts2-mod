using Iru.IruCode.Mechanics;
using Iru.IruCode.Powers;
using Iru.IruCode.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Iru.IruCode.Extensions;

public static class IruExtensions
{
    public static async Task CheckHyperReady(
        this Creature creature,
        PlayerChoiceContext? context,
        Creature source,
        CardModel? card)
    {
        var player =
            creature.Player;

        if (player == null)
            return;

        IruResourceRelicBase? resourceRelic =
            player.Relics
                .OfType<IruResourceRelicBase>()
                .FirstOrDefault();

        if (resourceRelic == null)
            return;

        // Hyper Ready was already granted during this turn.
        if (resourceRelic.HyperReadyGrantedThisTurn)
            return;

        // The player already has Hyper Ready.
        if (creature.HasPower<HyperReadyPower>())
            return;

        if (!IruResourceManager.HasHyper(
                player,
                IruResourceRelicBase.HyperReadyThreshold))
        {
            return;
        }

        // Set before awaiting so another simultaneous check
        // cannot apply Hyper Ready twice.
        resourceRelic.MarkHyperReadyGranted();

        await PowerCmd.Apply<HyperReadyPower>(
            context,
            creature,
            1,
            source,
            card);
    }
}