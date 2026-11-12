using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Iru.IruCode.Powers;

public class AccelHyperPower : IruPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    private const string HyperGainKey = "HyperGain";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(HyperGainKey, 15)
    ];

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != base.Owner.Player)
        {
            return amount;
        }
        return amount + (decimal)base.Amount;
    }
    
    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Owner.Player)
            return;

        Flash();

        int hyperGain =
            DynamicVars[HyperGainKey]
                .IntValue * Amount;

        IruResourceManager.GainHyper(
            player,
            hyperGain);

        await player.Creature.CheckHyperReady(
            choiceContext,
            player.Creature,
            null);
    }
    
    public void IncrementHyperGain()
    {
        AssertMutable();

        DynamicVars[HyperGainKey]
            .BaseValue++;
    }
    
    public void UpdateHyperGain()
    {
        AssertMutable();

        DynamicVars[HyperGainKey].BaseValue = 15 * Amount;
    }
}