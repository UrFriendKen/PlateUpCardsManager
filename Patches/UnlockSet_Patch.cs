using HarmonyLib;
using KitchenData;
using System.Collections.Generic;
using System.Linq;
using KitchenCardsManager.Helpers;

namespace KitchenCardsManager.Patches
{
    [HarmonyPatch]
    internal class UnlockSet_Patch
    {
        [HarmonyPatch(typeof(UnlockSetAutomatic), nameof(UnlockSetAutomatic.GetCardSet))]
        [HarmonyPostfix]
        private static void UnlockSetAutomatic_GetCardSet_Postfix(ref IEnumerable<Unlock> __result, UnlockRequest request)
        {
            RunGetCardSet(ref __result, request);
        }

        [HarmonyPatch(typeof(UnlockSetFixed), nameof(UnlockSetFixed.GetCardSet))]
        [HarmonyPostfix]
        private static void UnlockSetFixed_GetCardSet_Postfix(ref IEnumerable<Unlock> __result, UnlockRequest request)
        {
            RunGetCardSet(ref __result, request);
        }

        private static void RunGetCardSet(ref IEnumerable<Unlock> __result, UnlockRequest request)
        {
            if (Main.BlacklistModeEnabled)
            {
                FilterWithBlacklist(ref __result);
            }
            FilterWithEnabledCardGroups(ref __result);
        }

        private static void FilterWithBlacklist(ref IEnumerable<Unlock> __result)
        {
            __result = __result.Where(unlock => UnlockHelpers.GetEnabledState(unlock));
        }

        private static void FilterWithEnabledCardGroups(ref IEnumerable<Unlock> __result)
        {
            switch (Main.PrefManager.Get<string>(Main.CARDS_MANAGER_CARD_GROUPS_ENABLED))
            {
                case "VANILLA":
                    __result = __result.Intersect(UnlockHelpers.GetAllUnmoddedUnlocksEnumerable());
                    break;
                case "MODDED":
                    __result = __result.Intersect(UnlockHelpers.GetAllModdedUnlocksEnumerable());
                    break;
                case "ALL":
                default:
                    break;
            }
        }
    }
}
