using System.Collections.Generic;
using System.Threading.Tasks;
using Iru.IruCode.Cards.Uncommon;
using Iru.IruCode.Extensions;
using Iru.IruCode.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Powers;

public sealed class HeavyArtilleryPower : IruPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Owner.Player)
            return;

        Flash();
        IruResourceManager.GainHeat(
            player,
            Amount*10);
      
        var heatAbsorption = Owner.GetPowerAmount<HeatAbsorptionPower>();
        var tomboy = Owner.GetPowerAmount<TomboyPower>();
        var heatEngine = Owner.GetPowerAmount<HeatEnginePower>();
        var conversionProtocol = Owner.GetPowerAmount<ConversionProtocolPower>();
        
        await CreatureCmd.GainBlock(Owner, heatAbsorption, ValueProp.Unpowered, null, false);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, heatEngine, Owner, null);
        await CardPileCmd.Draw(choiceContext, tomboy, Owner.Player);
        IruResourceManager.GainHyper(Owner.Player, conversionProtocol);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
    }
}