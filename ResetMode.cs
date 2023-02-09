using Kitchen;
using KitchenLib.Utils;

namespace KitchenCardsManager
{
    internal class ResetModeOnNewRun : RestaurantInitialisationSystem
    {
        protected override void OnUpdate()
        {
            if (PreferenceUtils.Get<KitchenLib.IntPreference>(Main.MOD_GUID, Main.CARDS_MANAGER_RESET_MODE_PREFERENCE_ID).Value == 1)
            {
                Main.LogInfo("ResetModeOnNewRun Enabled");
                Main.ResetModeToVanilla();
            }
        }
    }

    internal class ResetModeOnFranchiseEnter : FranchiseFirstFrameSystem
    {
        protected override void OnUpdate()
        {
            Main.LogInfo("ResetModeOnFranchiseEnter Enabled");
            if (PreferenceUtils.Get<KitchenLib.IntPreference>(Main.MOD_GUID, Main.CARDS_MANAGER_RESET_MODE_PREFERENCE_ID).Value == 2)
            {
                Main.ResetModeToVanilla();
            }
        }
    }
}

