using System;
using System.Collections.Generic;
using System.Linq;
using Iru.IruCode.Relics;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Iru.IruCode.Mechanics;

public static class IruResourceManager
{
    public sealed class ResourceData
    {
        public Action<int>? OnHyperChanged;
        public Action<int>? OnHeatChanged;

        /*
         * Invoked whenever either Hyper or Heat changes.
         *
         * Parameters:
         * 1. Current Hyper
         * 2. Current Heat
         */
        public Action<int, int>? OnResourcesChanged;
    }

    private static readonly Dictionary<Player, ResourceData> _data =
        new();

    private static IruResourceRelicBase? GetRelic(
        Player player)
    {
        return player.Relics
            .OfType<IruResourceRelicBase>()
            .FirstOrDefault();
    }

    private static ResourceData GetData(
        Player player)
    {
        if (!_data.TryGetValue(
                player,
                out ResourceData? data))
        {
            data =
                new ResourceData();

            _data[player] =
                data;
        }

        return data;
    }

    private static void NotifyResourcesChanged(
        Player player)
    {
        ResourceData data =
            GetData(player);

        data.OnResourcesChanged?.Invoke(
            GetHyper(player),
            GetHeat(player));
    }

    // -------------------------
    // Hyper
    // -------------------------

    public static int GetHyper(
        Player player)
    {
        return GetRelic(player)?.StoredHyper ?? 0;
    }

    public static void SetHyper(
        Player player,
        int value)
    {
        IruResourceRelicBase? relic =
            GetRelic(player);

        if (relic == null)
            return;

        int newValue =
            Math.Clamp(
                value,
                0,
                IruResourceRelicBase.MaxHyper);

        if (relic.StoredHyper == newValue)
            return;

        relic.StoredHyper =
            newValue;

        ResourceData data =
            GetData(player);

        data.OnHyperChanged?.Invoke(
            newValue);

        data.OnResourcesChanged?.Invoke(
            newValue,
            relic.StoredHeat);
    }

    public static void GainHyper(
        Player player,
        int amount)
    {
        if (amount <= 0)
            return;

        SetHyper(
            player,
            GetHyper(player) + amount);
    }

    /*
     * Removes up to the requested amount of Hyper.
     *
     * Returns the amount actually spent.
     *
     * For example, if Iru has 20 Hyper and attempts
     * to spend 50, this removes and returns 20.
     */
    public static int SpendHyper(
        Player player,
        int amount)
    {
        if (amount <= 0)
            return 0;

        int current =
            GetHyper(player);

        int spent =
            Math.Min(
                current,
                amount);

        if (spent <= 0)
            return 0;

        SetHyper(
            player,
            current - spent);

        return spent;
    }

    public static int SpendAllHyper(
        Player player)
    {
        return SpendHyper(
            player,
            GetHyper(player));
    }

    public static bool HasHyper(
        Player player,
        int amount)
    {
        if (amount <= 0)
            return true;

        return GetHyper(player) >= amount;
    }

    public static bool IsHyperFull(
        Player player)
    {
        return GetHyper(player) >=
               IruResourceRelicBase.MaxHyper;
    }

    public static int GetMissingHyper(
        Player player)
    {
        return Math.Max(
            0,
            IruResourceRelicBase.MaxHyper -
            GetHyper(player));
    }

    // -------------------------
    // Heat
    // -------------------------

    public static int GetHeat(
        Player player)
    {
        return GetRelic(player)?.StoredHeat ?? 0;
    }

    public static void SetHeat(
        Player player,
        int value)
    {
        IruResourceRelicBase? relic =
            GetRelic(player);

        if (relic == null)
            return;

        int newValue =
            Math.Clamp(
                value,
                0,
                IruResourceRelicBase.MaxHeat);

        if (relic.StoredHeat == newValue)
            return;

        relic.StoredHeat =
            newValue;

        ResourceData data =
            GetData(player);

        data.OnHeatChanged?.Invoke(
            newValue);

        data.OnResourcesChanged?.Invoke(
            relic.StoredHyper,
            newValue);
    }

    public static void GainHeat(
        Player player,
        int amount)
    {
        if (amount <= 0)
            return;

        SetHeat(
            player,
            GetHeat(player) + amount);
    }

    /*
     * Removes Heat without generating Hyper.
     *
     * Use this for:
     * - Heat decay that should not convert
     * - cleansing effects
     * - penalties
     * - effects that explicitly say "Lose Heat"
     *
     * Returns the amount of Heat actually removed.
     */
    public static int LoseHeat(
        Player player,
        int amount)
    {
        if (amount <= 0)
            return 0;

        int current =
            GetHeat(player);

        int lost =
            Math.Min(
                current,
                amount);

        if (lost <= 0)
            return 0;

        SetHeat(
            player,
            current - lost);

        return lost;
    }

    /*
     * Spends Heat and converts the amount actually
     * spent into the same amount of Hyper.
     *
     * For example:
     *
     * Iru has 30 Heat.
     * An effect attempts to spend 50 Heat.
     * 30 Heat is spent.
     * 30 Hyper is gained.
     *
     * Returns the amount of Heat actually spent.
     */
    public static int SpendHeat(
        Player player,
        int amount)
    {
        if (amount <= 0)
            return 0;

        int spent =
            LoseHeat(
                player,
                amount);

        if (spent <= 0)
            return 0;

        GainHyper(
            player,
            spent);

        return spent;
    }

    /*
     * Spends all current Heat and converts the full
     * amount spent into Hyper.
     */
    public static int SpendAllHeat(
        Player player)
    {
        return SpendHeat(
            player,
            GetHeat(player));
    }

    /*
     * Removes all current Heat without converting
     * it into Hyper.
     */
    public static int ClearHeat(
        Player player)
    {
        return LoseHeat(
            player,
            GetHeat(player));
    }

    public static bool HasHeat(
        Player player,
        int amount)
    {
        if (amount <= 0)
            return true;

        return GetHeat(player) >= amount;
    }

    public static bool IsHeatFull(
        Player player)
    {
        return GetHeat