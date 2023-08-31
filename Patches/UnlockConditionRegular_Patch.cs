using HarmonyLib;
using KitchenData;

namespace KitchenCardsManager.Patches
{
    [HarmonyPatch]
    static class UnlockConditionRegular_Patch
    {
        [HarmonyPatch(typeof(UnlockConditionRegular), "ShouldProvide")]
        [HarmonyPrefix]
        static bool ShouldProvide_Prefix(ref int __state, ref int ___DayInterval)
        {
            int intervalPref = Main.PrefManager.Get<int>(Main.CARDS_MANAGER_CARD_DAY_INTERVAL);
            __state = -1;
            if (intervalPref < 0)
                return true;
            if (intervalPref == 0)
                return false;

            __state = ___DayInterval;
            ___DayInterval = intervalPref;
            return true;
        }

        [HarmonyPatch(typeof(UnlockConditionRegular), "ShouldProvide")]
        [HarmonyPostfix]
        static void ShouldProvide_Postfix(ref int __state, ref int ___DayInterval)
        {
            if (__state != -1)
                ___DayInterval = __state;
        }
    }
}
