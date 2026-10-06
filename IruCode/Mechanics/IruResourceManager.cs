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
            data = new ResourceData();

            _data[player] = data;
        }

        return data;
    }

    // -------------------------
    // Hyper
    // -------------------------

    public static int GetHyper(
        Player player)
    {
        IruResourceRelicBase? relic =
            GetRelic(player);

        return relic?.StoredHyper ?? 0;
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

        relic.StoredHyper = newValue;

        ResourceData data =
            GetData(player);

        data.OnHyperChanged?.Invoke(
            newValue);

        data.OnResourcesChanged?.Invoke(
            newValue,
            relic.StoredHeat);
    }

    public static int GainHyper(
        Player player,
        int amount)
    {
        if (amount <= 0)
            return 0;

        int current =
            GetHyper(player);

        int newValue =
            Math.Clamp(
                current + amount,
                0,
                IruResourceRelicBase.MaxHyper);

        int gained =
            newValue - current;

        if (gained <= 0)
            return 0;

        SetHyper(
            player,
            newValue);

        return gained;
    }

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
        int current =
            GetHyper(player);

        if (current <= 0)
            return 0;

        return SpendHyper(
            player,
            current);
    }

    public static int ClearHyper(
        Player player)
    {
        int current =
            GetHyper(player);

        if (current <= 0)
            return 0;

        SetHyper(
            player,
            0);

        return current;
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

    public static float GetHyperPercentage(
        Player player)
    {
        return GetHyper(player) /
               (float)IruResourceRelicBase.MaxHyper;
    }

    // -------------------------
    // Heat
    // -------------------------

    public static int GetHeat(
        Player player)
    {
        IruResourceRelicBase? relic =
            GetRelic(player);

        return relic?.StoredHeat ?? 0;
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

        relic.StoredHeat = newValue;

        ResourceData data =
            GetData(player);

        data.OnHeatChanged?.Invoke(
            newValue);

        data.OnResourcesChanged?.Invoke(
            relic.StoredHyper,
            newValue);
    }

    public static int GainHeat(
        Player player,
        int amount)
    {
        if (amount <= 0)
            return 0;

        int current =
            GetHeat(player);

        int newValue =
            Math.Clamp(
                current + amount,
                0,
                IruResourceRelicBase.MaxHeat);

        int gained =
            newValue - current;

        if (gained <= 0)
            return 0;

        SetHeat(
            player,
            newValue);

        return gained;
    }

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
     * Spending Heat converts the amount actually
     * removed into the same amount of Hyper.
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

    public static int SpendAllHeat(
        Player player)
    {
        int current =
            GetHeat(player);

        if (current <= 0)
            return 0;

        return SpendHeat(
            player,
            current);
    }

    /*
     * Clears Heat without converting it into Hyper.
     */
    public static int ClearHeat(
        Player player)
    {
        int current =
            GetHeat(player);

        if (current <= 0)
            return 0;

        SetHeat(
            player,
            0);

        return current;
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
        return GetHeat(player) >=
               IruResourceRelicBase.MaxHeat;
    }

    public static int GetMissingHeat(
        Player player)
    {
        return Math.Max(
            0,
            IruResourceRelicBase.MaxHeat -
            GetHeat(player));
    }

    public static float GetHeatPercentage(
        Player player)
    {
        return GetHeat(player) /
               (float)IruResourceRelicBase.MaxHeat;
    }

    // -------------------------
    // Turn-based Heat cooling
    // -------------------------

    /*
     * Removes Heat through automatic turn-based cooling.
     *
     * If convertToHyper is true, the amount actually
     * removed is also gained as Hyper.
     */
    public static int CoolHeat(
        Player player,
        int amount,
        bool convertToHyper)
    {
        if (amount <= 0)
            return 0;

        int cooled =
            LoseHeat(
                player,
                amount);

        if (cooled <= 0)
            return 0;

        if (convertToHyper)
        {
            GainHyper(
                player,
                cooled);
        }

        return cooled;
    }

    /*
     * Calculates Iru's default turn-based Heat loss:
     *
     * 20% of current Heat + 10.
     *
     * Integer division rounds the percentage portion down.
     */
    public static int CalculateHeatDecay(
        Player player)
    {
        int currentHeat =
            GetHeat(player);

        if (currentHeat <= 0)
            return 0;

        int percentageDecay =
            currentHeat * 20 / 100;

        int totalDecay =
            percentageDecay + 10;

        return Math.Clamp(
            totalDecay,
            0,
            currentHeat);
    }

    /*
     * Convenience method that calculates and applies
     * the default 20% + 10 Heat decay.
     */
    public static int CoolHeat(
        Player player,
        bool convertToHyper)
    {
        int amount =
            CalculateHeatDecay(
                player);

        return CoolHeat(
            player,
            amount,
            convertToHyper);
    }

    // -------------------------
    // Damage multiplier
    // -------------------------

    /*
     * Heat increases incoming damage by Heat%.
     *
     * 0 Heat   = 1.00x
     * 10 Heat  = 1.10x
     * 50 Heat  = 1.50x
     * 100 Heat = 2.00x
     */
    public static float GetHeatDamageMultiplier(
        Player player)
    {
        return 1f +
               GetHeat(player) / 100f;
    }

    // -------------------------
    // Combined resource changes
    // -------------------------

    /*
     * Sets both resources and invokes the combined
     * notification only once.
     */
    public static void SetResources(
        Player player,
        int hyper,
        int heat)
    {
        IruResourceRelicBase? relic =
            GetRelic(player);

        if (relic == null)
            return;

        int newHyper =
            Math.Clamp(
                hyper,
                0,
                IruResourceRelicBase.MaxHyper);

        int newHeat =
            Math.Clamp(
                heat,
                0,
                IruResourceRelicBase.MaxHeat);

        bool hyperChanged =
            relic.StoredHyper != newHyper;

        bool heatChanged =
            relic.StoredHeat != newHeat;

        if (!hyperChanged &&
            !heatChanged)
        {
            return;
        }

        relic.StoredHyper =
            newHyper;

        relic.StoredHeat =
            newHeat;

        ResourceData data =
            GetData(player);

        if (hyperChanged)
        {
            data.OnHyperChanged?.Invoke(
                newHyper);
        }

        if (heatChanged)
        {
            data.OnHeatChanged?.Invoke(
                newHeat);
        }

        data.OnResourcesChanged?.Invoke(
            newHyper,
            newHeat);
    }

    public static void Reset(
        Player player)
    {
        SetResources(
            player,
            0,
            0);
    }

    // -------------------------
    // UI
    // -------------------------

    public static ResourceData GetDataForUI(
        Player player)
    {
        return GetData(player);
    }

    public static void NotifyCurrentValues(
        Player player)
    {
        ResourceData data =
            GetData(player);

        int hyper =
            GetHyper(player);

        int heat =
            GetHeat(player);

        data.OnHyperChanged?.Invoke(
            hyper);

        data.OnHeatChanged?.Invoke(
            heat);

        data.OnResourcesChanged?.Invoke(
            hyper,
            heat);
    }

    // -------------------------
    // Cleanup
    // -------------------------

    public static void ClearPlayerData(
        Player player)
    {
        _data.Remove(
            player);
    }

    public static void ClearAllData()
    {
        _data.Clear();
    }
}