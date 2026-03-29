using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace CloverMod;

public class CloverRelic : RelicModel, IPoolModel
{
    public override RelicRarity Rarity { get; } = RelicRarity.Starter;
    public string EnergyColorName { get; } = "colorless";
    public decimal DamageReceivedThisTurn { get; set; }
    public decimal MaxDamageReceivedEveryTurn { get; set; } = 50m;

    //获得遗物之后
    public override async Task AfterObtained()
    {
        
        Flash(); // 触发遗物图标闪烁
        await PlayerCmd.GainGold(999, base.Owner, false);

    }

  
}