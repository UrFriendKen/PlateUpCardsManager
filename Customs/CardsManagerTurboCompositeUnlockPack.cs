using KitchenData;
using KitchenLib.Customs;
using System.Collections.Generic;
using KitchenLib.References;
using KitchenLib.Utils;

namespace KitchenCardsManager.Customs
{
    internal class CardsManagerTurboCompositeUnlockPack : CustomCompositeUnlockPack
    {
        protected const int FRANCHISE_CARDS_PACK_ID = ModularUnlockPackReferences.FranchiseCardsPack;
        protected const int THEME_CARDS_PACK_ID = ModularUnlockPackReferences.ThemeCardsPack;

        public override string UniqueNameID { get { return Main.CARDS_MANAGER_TURBO_COMPOSITE_UNLOCK_PACK_UNIQUENAMEID; } } // Setting the UniqueNameID ( Used to generate GDO ID )
        public override List<UnlockPack> Packs => new List<UnlockPack>()
        {
            (ModularUnlockPack)GDOUtils.GetExistingGDO(ModularUnlockPackReferences.FranchiseCardsPack),
            (ModularUnlockPack)GDOUtils.GetExistingGDO(ModularUnlockPackReferences.ThemeCardsPack),
            (ModularUnlockPack)GDOUtils.GetCustomGameDataObject<CardsManagerTurboModularUnlockPack>().GameDataObject
        };

        public override void OnRegister(CompositeUnlockPack gameDataObject)
        {
            Main.LogInfo("Registered: " + this.UniqueNameID);
        }
    }
}
