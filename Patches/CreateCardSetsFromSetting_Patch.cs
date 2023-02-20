using HarmonyLib;
using Kitchen;
using KitchenCardsManager.Helpers;

namespace KitchenCardsManager.Patches
{
    [HarmonyPatch]
    internal static class CreateCardSetsFromSetting_Patch
    {
        [HarmonyPatch(typeof(CreateCardSetsFromSetting), "OnUpdate")]
        [HarmonyPrefix]
        public static bool OnUpdate_Prefix(ref CardsManagerStartingCardController.StartingUnlockData __state)
        {
            CardsManagerStartingCardController.StartingUnlockData data = CardsManagerStartingCardController.Output;
            __state = null;
            if (data != null)
            {
                if (!Main.BlacklistModeEnabled)
                {
                    Main.LogInfo("Skipping Starting Card Check due to Vanilla Mode");
                    return true;
                }

                string settingName = data.RestaurantSetting.Name;
                if (data.StartingUnlock == null)
                {
                    Main.LogInfo($"No StartingUnlock for setting ({settingName})");
                    return true;
                }

                string unlockName = data.StartingUnlock.Name;
                bool unlockEnabled = UnlockHelpers.GetEnabledState(data.StartingUnlock);
                if (unlockEnabled)
                {
                    Main.LogInfo($"StartingUnlock ({unlockName}) is enabled. Skipping.");
                    return true;
                }

                Main.LogInfo($"Removed StartingUnlock ({unlockName})");
                data.RestaurantSetting.StartingUnlock = null;
                __state = data;
                return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(CreateCardSetsFromSetting), "OnUpdate")]
        [HarmonyPostfix]
        public static void OnUpdate_Postfix(ref CardsManagerStartingCardController.StartingUnlockData __state)
        {
            if (__state == null)
            {
                return;
            }

            __state.RestaurantSetting.StartingUnlock = __state.StartingUnlock;
            Main.LogInfo($"Replace StartingUnlock with original value ({__state.StartingUnlock.Name})");
        }
    }
}
