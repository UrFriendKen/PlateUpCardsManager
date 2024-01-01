using HarmonyLib;
using Kitchen;

namespace KitchenCardsManager.Patches
{
    [HarmonyPatch]
    static class BeginRestaurant_Patch
    {
        [HarmonyPatch(typeof(BeginRestaurant), "OnUpdate")]
        [HarmonyPostfix]
        static void OnUpdate_Postfix()
        {
            BeginRestaurantPatchController.ApplyAutoAddCards();
        }
    }
}
