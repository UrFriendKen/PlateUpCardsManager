using HarmonyLib;
using KitchenData;
using System.Collections.Generic;
using System.Linq;
using KitchenCardsManager.Helpers;
using System.Reflection;
using System;

namespace KitchenCardsManager.Patches
{
    [HarmonyPatch]
    internal class UnlockSet_Patch
    {
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> UnlockSet_TargetMethods()
        {
            IEnumerable<MethodBase> targetMethods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(AccessTools.GetTypesFromAssembly)
                .Where(type => typeof(IUnlockSet).IsAssignableFrom(type) &&
                        !type.IsAbstract &&
                        type != typeof(IUnlockSet))
                .SelectMany(type => type.GetMethods())
                .Where(method => method.ReturnType == typeof(IEnumerable<Unlock>) && method.Name.StartsWith("GetCardSet"))
                .Cast<MethodBase>();

            Main.LogInfo($"Number of methods in UnlockSet_Patch: {targetMethods.Count()}");

            foreach (MethodBase methodBase in targetMethods)
            {
                Main.LogInfo($"{methodBase.DeclaringType.FullName}.{methodBase.Name}");
            }

            return targetMethods;
        }

        [HarmonyPostfix]
        private static void UnlockSet_GetCardSet_Postfix(ref IEnumerable<Unlock> __result, UnlockRequest request)
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
