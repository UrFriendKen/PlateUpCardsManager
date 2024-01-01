using Kitchen;
using KitchenCardsManager.Helpers;
using KitchenData;
using KitchenMods;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace KitchenCardsManager
{
    internal class BeginRestaurantPatchController : GameSystemBase, IModSystem
    {
        private class RequirementOrderComparer : IComparer<Unlock>
        {
            public int Compare(Unlock x, Unlock y)
            {
                int val = (x.Requires.Contains(y) ? 1 : 0) - (y.Requires.Contains(x) ? 1 : 0);
                return val;
            }
        }

        static BeginRestaurantPatchController _instance;

        protected override void Initialise()
        {
            base.Initialise();
            _instance = this;
        }

        protected override void OnUpdate()
        {
        }

        public static void ApplyAutoAddCards()
        {
            if (_instance == null || !_instance.GetComponentOfSingletonHolder<CItemLayoutMap, SSelectedLayoutPedestal>(out CItemLayoutMap itemLayoutMap))
                return;

            DynamicBuffer<CStartingItem> startingItems = _instance.GetBuffer<CStartingItem>(itemLayoutMap.Layout);
            HashSet<int> startingItemIDs = new HashSet<int>();
            for (int i = 0; i < startingItems.Length; i++)
            {
                startingItemIDs.Add(startingItems[i].ID);
            }

            List<Unlock> startingUnlocksToAdd = UnlockHelpers.GetAllUnlocksEnumerable().Where(unlock => !startingItemIDs.Contains(unlock.ID) && UnlockHelpers.ShouldBeAutoAdded(unlock)).ToList();
            startingUnlocksToAdd.Sort(new RequirementOrderComparer());

            foreach (Unlock unlock in startingUnlocksToAdd)
            {
                startingItems.Add(unlock.ID);
            }
        }
    }
}
