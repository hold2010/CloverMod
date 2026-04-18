using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Logging;

namespace CloverMod;

public sealed class CloverRelic : RelicModel, IPoolModel
{
    // 是否正在播放激活动画
    private bool _isActivating;

    // 已经历的战斗次数
	private int _combatsSeen;

    // 触发效果所需的战斗间隔
	private const int _triggerInterval = 4;

    // 遗物稀有度：初始遗物
	public override RelicRarity Rarity => RelicRarity.Starter;

    // 解锁遗物
    public string EnergyColorName => "colorless";

    // 显示计数器
	public override bool ShowCounter => true;

    // UI显示数字
	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return CloverCombatsSeen % _triggerInterval;
			}
			return _triggerInterval;
		}
	}

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
			InvokeDisplayAmountChanged();
		}
	}

	[SavedProperty]
	public int CloverCombatsSeen
	{
		get
		{
			return _combatsSeen;
		}
		set
		{
			AssertMutable();
			_combatsSeen = value;
		}
	}

	private bool IsInTriggeringCombat
	{
		get
		{
			if (CloverCombatsSeen > 0)
			{
				return CloverCombatsSeen % _triggerInterval == 0;
			}
			return false;
		}
	}

    public override bool IsAllowed(IRunState runState)
	{
        // 遗物获取限制：第三章宝箱前可获得
		return RelicModel.IsBeforeAct3TreasureChest(runState);
	}

    /// <summary>
    /// 修改战斗结束后的卡牌奖励：替换其中一张为稀有卡
    /// </summary>
	public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> options, CardCreationOptions creationOptions)
	{
		if (base.Owner != player)
		{
			return false;
		}
		if (creationOptions.Source != CardCreationSource.Encounter)
		{
			return false;
		}
		if (!IsInTriggeringCombat)
		{
			return false;
		}
        // 构造一个只包含稀有卡的卡池，并且排除掉已经在选项里的卡
		IEnumerable<CardModel> customCardPool = from c in creationOptions.GetPossibleCards(player)
			where c.Rarity == CardRarity.Rare && options.TrueForAll((CardCreationResult o) => o.originalCard.Id != c.Id)
			select c;
        // 单稀有度卡池 → 强制用 CardRarityOddsType.Uniform
		CardCreationOptions options2 = new CardCreationOptions(customCardPool, CardCreationSource.Other, CardRarityOddsType.Uniform).WithFlags(CardCreationFlags.NoModifyHooks | CardCreationFlags.NoCardPoolModifications);
		// 从构造的卡池里随机选一张
        CardModel cardModel = CardFactory.CreateForReward(base.Owner, 1, options2).FirstOrDefault()?.Card;
		if (cardModel != null)
		{
			CardCreationResult cardCreationResult = new CardCreationResult(cardModel);
			cardCreationResult.ModifyCard(cardModel, this);
            // 从奖励列表中找最后一个【不是稀有】的卡牌，如果找不到就直接替换最后一个
	        CardCreationResult? target = options.LastOrDefault(c => c.originalCard.Rarity != CardRarity.Rare);
            int index = target != null ? options.IndexOf(target) : options.Count - 1;
            options[index] = cardCreationResult;
            Log.Info($"CloverRelic: Replaced card reward[{index+1}] with {cardModel.Title}");
        }
		return cardModel != null;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
        // 战斗结束后：计数+1 
		CloverCombatsSeen++;
        // 满足条件则播放激活动画
		if (IsInTriggeringCombat)
		{
			TaskHelper.RunSafely(DoActivateVisuals());
		}
        // 刷新UI图标的计数器
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	private async Task DoActivateVisuals()
	{
		IsActivating = true;
		Flash();
		await Cmd.Wait(1f);
		IsActivating = false;
	}
  
}