using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Iru.IruCode.Relics;

// Every time you play 3 Attacks in a single turn, deal 5 damage to ALL enemies
public class Spread : IruRelic
{
	private bool _isActivating;

	private int _attacksPlayedThisTurn;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override bool ShowCounter => CombatManager.Instance.IsInProgress;

	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return AttacksPlayedThisTurn % base.DynamicVars.Cards.IntValue;
			}
			return base.DynamicVars.Cards.IntValue;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => 
	[
		new CardsVar(3),
		new DamageVar(5m, ValueProp.Unpowered)
	];

	private bool IsActivating
	{
		get
		{
			return _isActivating;
		}
		set
		{
			AssertMutable();
			_isActivating = value;
			UpdateDisplay();
		}
	}

	private int AttacksPlayedThisTurn
	{
		get
		{
			return _attacksPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_attacksPlayedThisTurn = value;
			UpdateDisplay();
		}
	}

	private void UpdateDisplay()
	{
		if (IsActivating)
		{
			base.Status = RelicStatus.Normal;
		}
		else
		{
			int intValue = base.DynamicVars.Cards.IntValue;
			base.Status = ((AttacksPlayedThisTurn % intValue == intValue - 1) ? RelicStatus.Active : RelicStatus.Normal);
		}
		InvokeDisplayAmountChanged();
	}

	public override Task BeforeCombatStart()
	{
		AttacksPlayedThisTurn = 0;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}

	public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(base.Owner.Creature))
		{
			return Task.CompletedTask;
		}
		if (base.Owner.PlayerCombatState.TurnNumber == 1)
		{
			return Task.CompletedTask;
		}
		AttacksPlayedThisTurn = 0;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner && CombatManager.Instance.IsInProgress && cardPlay.Card.Type == CardType.Attack)
		{
			AttacksPlayedThisTurn++;
			int intValue = base.DynamicVars.Cards.IntValue;
			if (AttacksPlayedThisTurn % intValue == 0)
			{
				TaskHelper.RunSafely(DoActivateVisuals());
				await CreatureCmd.Damage(choiceContext, base.Owner.Creature.CombatState.HittableEnemies, base.DynamicVars.Damage, base.Owner.Creature);
			}
		}
	}

	private async Task DoActivateVisuals()
	{
		IsActivating = true;
		Flash();
		await Cmd.Wait(1f);
		IsActivating = false;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		base.Status = RelicStatus.Normal;
		IsActivating = false;
		return Task.CompletedTask;
	}

	public int GetAttacksPlayedForTest()
	{
		return AttacksPlayedThisTurn;
	}
}