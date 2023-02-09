using Kitchen;
using Unity.Entities;
using KitchenLib.Utils;
using KitchenData;
using static Kitchen.CreateCardSetsFromSetting;

namespace KitchenCardsManager
{
    internal class EnableAllUnlockPacks : RestaurantSystem
    {

        bool? prevWhiteListMode = null;

        protected override void Initialise()
        {
            base.Initialise();
        }

        protected override void OnUpdate()
        {
            if (!Main.WhitelistModeEnabled)
            {
                if (RequireEntity<SLayout>(out var comp) && Require(comp, out CSetting cSetting) && GameData.Main.TryGet<RestaurantSetting>(cSetting.RestaurantSetting, out var output))
                {
                    if (RequireEntity<SSettingUnlockPack>(out Entity entity))
                    {
                        if (output.UnlockPack == null)
                        {
                            EntityManager.DestroyEntity(entity);
                        }
                        else
                        {
                            Set(new CUnlockPack
                            {
                                ActiveUnlockPack = output.UnlockPack.ID
                            });
                        }
                    }
                }
            }
            else if (GetOrDefault<SDay>().Day > 0)
            {
                if (!RequireEntity<SSettingUnlockPack>(out Entity entity))
                {
                    entity = base.EntityManager.CreateEntity(typeof(SSettingUnlockPack), typeof(CUnlockPack));
                }
                Set(entity, new CUnlockPack
                {
                    ActiveUnlockPack = GDOUtils.GetCustomGameDataObject<CardsManagerCompositeUnlockPack>().ID
                });
            }

            if (!prevWhiteListMode.HasValue || Main.WhitelistModeEnabled != prevWhiteListMode)
            {
                string whiteListModeStr = Main.WhitelistModeEnabled ? "Enabled" : "Disabled";
                Main.LogInfo($"Whitelist Mode {whiteListModeStr}");
                int activeUnlockPack = GetOrDefault<CUnlockPack>().ActiveUnlockPack;
                if (activeUnlockPack != 0)
                {
                    string packid = activeUnlockPack.ToString() + ((activeUnlockPack == GDOUtils.GetCustomGameDataObject<CardsManagerCompositeUnlockPack>().ID) ? " (CardsManagerCompositeUnlockPack)" : "");
                    Main.LogInfo($"activeUnlockPack = {packid}");
                }
                else
                {
                    Main.LogInfo("No Unlock Pack for this Setting");
                }
                prevWhiteListMode = Main.WhitelistModeEnabled;
            }
        }
    }
}
