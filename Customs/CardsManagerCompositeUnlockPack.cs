using KitchenData;
using KitchenLib.Customs;
using System.Collections.Generic;
using KitchenLib.Utils;

namespace KitchenCardsManager.Customs
{
    internal class CardsManagerCompositeUnlockPack : CustomCompositeUnlockPack
    {
        protected const int FRANCHISE_CARDS_PACK_ID = 1355831133;   // Franchise Cards Pack
        protected const int THEME_CARDS_PACK_ID = 786043106;    // Theme Cards Pack

        public override string UniqueNameID { get { return Main.CARDS_MANAGER_COMPOSITE_UNLOCK_PACK_UNIQUENAMEID; } } // Setting the UniqueNameID ( Used to generate GDO ID )
        public override List<UnlockPack> Packs => new List<UnlockPack>()
        {
            (ModularUnlockPack)GDOUtils.GetExistingGDO(FRANCHISE_CARDS_PACK_ID),
            (ModularUnlockPack)GDOUtils.GetExistingGDO(THEME_CARDS_PACK_ID),
            (ModularUnlockPack)GDOUtils.GetCustomGameDataObject<CardsManagerModularUnlockPack>().GameDataObject,
        };

        public override void OnRegister(CompositeUnlockPack gameDataObject)
        {
            Main.LogInfo("Registered: " + this.UniqueNameID);
        }
    }
}
