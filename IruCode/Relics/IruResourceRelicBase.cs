using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Relics;

public abstract class IruResourceRelicBase : IruRelic
{
    private int _storedHyper;
    private int _storedHeat;
    private bool _tookDamageSinceLastTurn;

    public const int MaxHyper = 300;
    public const int MaxHeat = 999;

    public override RelicRarity Rarity =>
        RelicRarity.Starter;

    /*
     * Hyper and Heat should be displayed using
     * Iru's custom resource UI.
     */
    public override bool ShowCounter =>
        false;

    /*
     * Iru gains 10 Hyper at the start of her turn.
     */
    protected virtual int HyperPerTurn =>
        0;

    /*
     * Iru gains 10 Hyper whenever she plays a card.
     */
    protected virtual int HyperPerCard =>
        10;

    /*
     * Heat decays by 20% plus 10 at the start
     * of Iru's turn.
     */
    protected virtual int HeatDecayPercent =>
        20;

    protected virtual int FlatHeatDecay =>
        10;

    public int StoredHyper
    {
        get =>
            _storedHyper;

        set
        {
            AssertMutable();

            int newValue =
                Math.Clamp(
                    value,
                    0,
                    MaxHyper);

            if (_storedHyper == newValue)
                return;

            _storedHyper =
                newValue;

            UpdateDisplay();
        }
    }

    public int StoredHeat
    {
        get =>
            _storedHeat;

        set
        {
            AssertMutable();

            int newValue =
                Math.Clamp(
                    value,
                    0,
                    MaxHeat);

            if (_storedHeat == newValue)
                return;

            _storedHeat =
                newValue;

            UpdateDisplay();
        }
    }

    public bool TookDamageSinceLastTurn =>
        _tookDamageSinceLastTurn;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(
            "HyperPerTurn",
            HyperPerTurn),

        new DynamicVar(
            "HyperPerCard",
            HyperPerCard),

        new DynamicVar(
            "HeatDecayPercent",
            HeatDecayPercent),

        new DynamicVar(
            "FlatHeatDecay",
            FlatHeatDecay),

        new DynamicVar(
            "MaxHyper",
            MaxHyper),

        new DynamicVar(
            "MaxHeat",
            MaxHeat)
    ];

    public override Task BeforeCombatStart()
    {
        IruResourceManager.Reset(
            Owner);

        SetDamageTakenTracking(
            false);

        Status =
            RelicStatus.Normal;

        UpdateDisplay();

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(
        CombatRoom _)
    {
        IruResourceManager.Reset(
            Owner);

        SetDamageTakenTracking(
            false);

        Status =
            RelicStatus.Normal;

        UpdateDisplay();

        return Task.CompletedTask;
    }

    /*
     * At the start of Iru's turn:
     *
     * 1. Calculate Heat decay as 20% + 10.
     * 2. Remove that Heat.
     * 3. If Iru took no HP damage since her previous
     *    turn, gain Hyper equal to the Heat removed.
     * 4. Gain the normal 10 Hyper per turn.
     * 5. Reset damage tracking for the new turn.
     */
    public override Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != Owner.Creature.Side)
            return Task.CompletedTask;

        bool convertHeatToHyper =
            !TookDamageSinceLastTurn;

        int heatDecay =
            CalculateHeatDecay();

        IruResourceManager.CoolHeat(
            Owner,
            heatDecay,
            convertHeatToHyper);

        IruResourceManager.GainHyper(
            Owner,
            DynamicVars["HyperPerTurn"].IntValue);

        SetDamageTakenTracking(
            false);

        return Task.CompletedTask;
    }
    
    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner.Creature)
            return Task.CompletedTask;

        // Fully blocked damage does not count
        if (result.UnblockedDamage <= 0)
            return Task.CompletedTask;

        MarkDamageTaken();

        return Task.CompletedTask;
    }

    /*
     * Iru gains 10 Hyper whenever she finishes
     * playing one of her cards.
     */
    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        CardModel card =
            cardPlay.Card;

        if (card.Owner != Owner)
            return Task.CompletedTask;

        if (!CombatManager.Instance.IsInProgress)
            return Task.CompletedTask;

        int hyperGain =
            Math.Max(
                0,
                DynamicVars["HyperPerCard"].IntValue);

        IruResourceManager.GainHyper(
            Owner,
            hyperGain);

        return Task.CompletedTask;
    }

    /*
     * Heat decay is:
     *
     * Current Heat * 20 / 100 + 10
     *
     * The result cannot exceed Iru's current Heat.
     *
     * Examples:
     *
     * 10 Heat  -> lose 10
     * 25 Heat  -> lose 15
     * 50 Heat  -> lose 20
     * 100 Heat -> lose 30
     * 200 Heat -> lose 50
     */
    public int CalculateHeatDecay()
    {
        int currentHeat =
            IruResourceManager.GetHeat(
                Owner);

        if (currentHeat <= 0)
            return 0;

        int decayPercent =
            Math.Max(
                0,
                DynamicVars["HeatDecayPercent"].IntValue);

        int flatDecay =
            Math.Max(
                0,
                DynamicVars["FlatHeatDecay"].IntValue);

        int percentageDecay =
            currentHeat * decayPercent / 100;

        int totalDecay =
            percentageDecay + flatDecay;

        return Math.Clamp(
            totalDecay,
            0,
            currentHeat);
    }

    /*
     * Call this only when Iru actually loses HP.
     *
     * Fully blocked damage should not count.
     */
    public void MarkDamageTaken()
    {
        if (!CombatManager.Instance.IsInProgress)
            return;

        SetDamageTakenTracking(
            true);
    }

    public void SetDamageTakenTracking(
        bool tookDamage)
    {
        AssertMutable();

        if (_tookDamageSinceLastTurn ==
            tookDamage)
        {
            return;
        }

        _tookDamageSinceLastTurn =
            tookDamage;

        UpdateDisplay();
    }

    // -------------------------
    // Hyper
    // -------------------------

    public int GetHyper()
    {
        return IruResourceManager.GetHyper(
            Owner);
    }

    public void SetHyper(
        int amount)
    {
        IruResourceManager.SetHyper(
            Owner,
            amount);
    }

    public void GainHyper(
        int amount)
    {
        IruResourceManager.GainHyper(
            Owner,
            amount);
    }

    public int SpendHyper(
        int amount)
    {
        return IruResourceManager.SpendHyper(
            Owner,
            amount);
    }

    public int SpendAllHyper()
    {
        return IruResourceManager.SpendAllHyper(
            Owner);
    }

    public bool HasHyper(
        int amount)
    {
        return IruResourceManager.HasHyper(
            Owner,
            amount);
    }

    public bool IsHyperFull()
    {
        return IruResourceManager.IsHyperFull(
            Owner);
    }

    // -------------------------
    // Heat
    // -------------------------

    public int GetHeat()
    {
        return IruResourceManager.GetHeat(
            Owner);
    }

    public void SetHeat(
        int amount)
    {
        IruResourceManager.SetHeat(
            Owner,
            amount);
    }

    public void GainHeat(
        int amount)
    {
        IruResourceManager.GainHeat(
            Owner,
            amount);
    }

    /*
     * Spending Heat removes it and generates the
     * same amount of Hyper.
     *
     * The return value is the amount actually spent.
     */
    public int SpendHeat(
        int amount)
    {
        return IruResourceManager.SpendHeat(
            Owner,
            amount);
    }

    public int SpendAllHeat()
    {
        return IruResourceManager.SpendAllHeat(
            Owner);
    }

    /*
     * Losing Heat removes it without generating Hyper.
     */
    public int LoseHeat(
        int amount)
    {
        return IruResourceManager.LoseHeat(
            Owner,
            amount);
    }

    /*
     * Clears all Heat without generating Hyper.
     */
    public int ClearHeat()
    {
        return IruResourceManager.ClearHeat(
            Owner);
    }

    public bool HasHeat(
        int amount)
    {
        return IruResourceManager.HasHeat(
            Owner,
            amount);
    }

    public bool IsHeatFull()
    {
        return IruResourceManager.IsHeatFull(
            Owner);
    }

    // -------------------------
    // Damage multiplier
    // -------------------------

    /*
     * Heat increases damage received by Heat%.
     *
     * Examples:
     *
     * 0 Heat   = 1.00x damage
     * 10 Heat  = 1.10x damage
     * 50 Heat  = 1.50x damage
     * 100 Heat = 2.00x damage
     */
    public float GetHeatDamageMultiplier()
    {
        return 1f +
               GetHeat() / 100f;
    }
    
    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner.Creature)
            return 1m;

        if (dealer == Owner.Creature)
            return 1m;

        return 1m + (GetHeat() / 100m);
    }

    // -------------------------
    // UI
    // -------------------------

    public int GetHyperForUI()
    {
        return GetHyper();
    }

    public int GetMaxHyperForUI()
    {
        return MaxHyper;
    }

    public float GetHyperPercentageForUI()
    {
        return GetHyperForUI() /
               (float)MaxHyper;
    }

    public int GetHeatForUI()
    {
        return GetHeat();
    }

    public int GetMaxHeatForUI()
    {
        return MaxHeat;
    }

    public float GetHeatPercentageForUI()
    {
        return GetHeatForUI() /
               (float)MaxHeat;
    }

    public int GetHeatDecayForUI()
    {
        return CalculateHeatDecay();
    }

    private void UpdateDisplay()
    {
        bool active =
            StoredHyper >= MaxHyper;

        Status =
            active
                ? RelicStatus.Active
                : RelicStatus.Normal;

        InvokeDisplayAmountChanged();
    }
}