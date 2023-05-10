using Kitchen;
using KitchenLib.Preferences;

namespace KitchenCardsManager
{
    internal class ResetModeOnNewRun : RestaurantInitialisationSystem
    {
        protected override void OnUpdate()
        {
            if (Main.PrefManager.Get<int>(Main.CARDS_MANAGER_RESET_MODE_PREFERENCE_ID) == 1)
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
            if (Main.PrefManager.Get<int>(Main.CARDS_MANAGER_RESET_MODE_PREFERENCE_ID) == 2)
            {
                Main.ResetModeToVanilla();
            }
        }
    }
}

