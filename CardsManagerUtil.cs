using KitchenCardsManager.Helpers;
using KitchenData;
using System.Collections.Generic;

namespace KitchenCardsManager
{
    public static class CardsManagerUtil
    {
        internal struct UnlockPageData
        {
            internal string Name;
            internal Dictionary<int, List<int>> Order;
            internal Dictionary<int, List<Unlock>> Rows;

            internal int RowCount
            {
                get
                {
                    return Order.Count;
                }
            }

            internal UnlockPageData(string name)
            {
                Name = name;
                Order = new Dictionary<int, List<int>>();
                Rows = new Dictionary<int, List<Unlock>>();
            }

            internal void Add(int row_index, Unlock unlock, int order)
            {
                if (!Rows.ContainsKey(row_index))
                {
                    Rows.Add(row_index, new List<Unlock>());
                    Order.Add(row_index, new List<int>());
                }

                if (!Rows[row_index].Contains(unlock))
                {
                    Order[row_index].Add(order);
                    Rows[row_index].Add(unlock);
                    Main.LogInfo($"Unlock {unlock.name} ({unlock.ID}) successfully registered.");
                }
                else
                {
                    Main.LogInfo($"Unlock {unlock.name} ({unlock.ID}) already registered.");
                }
            }
        }

        private static Dictionary<string, int> _registeredPageNames = new Dictionary<string, int>();
        private static List<UnlockPageData> _registeredUnlockPages = new List<UnlockPageData>();

        private static Dictionary<int, Unlock> ModdedUnlockDict = new Dictionary<int, Unlock>();

        private static void RegisterUnlock(string page_guid, int row_index, Unlock unlock, int unlock_order = 0)
        {
            if (!_registeredPageNames.TryGetValue(page_guid, out int i))
            {
                _registeredPageNames.Add(page_guid, _registeredPageNames.Count);
                _registeredUnlockPages.Add(new UnlockPageData(page_guid));
                i = _registeredPageNames.Count - 1;
            }
            _registeredUnlockPages[i].Add(row_index, unlock, unlock_order);
        }

        public static void RegisterUnlock(string page_guid, int row_index, int moddedUnlockID, int unlock_order = 0)
        {
            if (!ModdedUnlockDict.ContainsKey(moddedUnlockID))
            {
                ModdedUnlockDict.Clear();
                foreach (Unlock unlock in UnlockHelpers.GetAllModdedUnlocksEnumerable())
                {
                    ModdedUnlockDict.Add(unlock.ID, unlock);
                }
            }

            if (ModdedUnlockDict.TryGetValue(moddedUnlockID, out Unlock moddedUnlock))
            {
                RegisterUnlock(page_guid, row_index, moddedUnlock, unlock_order);
            }
            else
            {
                Main.LogInfo($"Modded unlock with ID {moddedUnlockID} not found!");
            }
        }

        internal static IEnumerable<UnlockPageData> GetRegisteredPages()
        {
            return _registeredUnlockPages;
        }
    }
}
