using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Logging;

namespace CloverMod
{
    [ModInitializer(nameof(Initialize))]
    public static class CloverModInitializer
    {
        public static void Initialize()
        {
            // 打patch（即修改游戏代码的功能）用
            // 传入参数随意，只要不和其他人撞车即可
            var harmony = new Harmony("holdyoung.clovermod");
            harmony.PatchAll();
            // 使得tscn可以加载自定义脚本
            Log.Debug("**************** CloverMod initialized ****************");
            ModHelper.AddModelToPool<RelicPoolModel, CloverRelic>();
           
        }
    }
}