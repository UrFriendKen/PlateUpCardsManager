using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KitchenCardsManager.Helpers
{
    internal static class UnlockHelpers
    {
        internal static bool IsModded(int id)
        {
            return CustomGDO.GDOs.ContainsKey(id);
        }

        internal static bool IsModded(GameDataObject gdo)
        {
            return IsModded(gdo.ID);
        }

        internal static IEnumerable<Unlock> GetAllUnlocksEnumerable()
        {
            Dictionary<string, List<Unlock>> dict = new Dictionary<string, List<Unlock>>();
            foreach (Unlock unlock in GameData.Main.Get<Unlock>())
            {
                string group = GetCardGroup(unlock);
                if (!dict.ContainsKey(group))
                {
                    dict.Add(group, new List<Unlock>());
                }
                dict[group].Add(unlock);
            }
            return dict.Values.OrderByDescending(x => x.Count).SelectMany(x => x);
        }

        internal static bool IsValidUnlock (int id)
        {
            return GetAllUnlocksEnumerable().Select(x => x.ID).Contains(id);
        }

        internal static IEnumerable<Unlock> GetAllUnmoddedUnlocksEnumerable()
        {
            return GetAllUnlocksEnumerable().Where(x => !IsModded(x.ID));
        }

        internal static IEnumerable<Unlock> GetAllModdedUnlocksEnumerable()
        {
            return GetAllUnlocksEnumerable().Where(x => IsModded(x.ID));
            //Dictionary<string, List<Unlock>> unlocksByModName = new Dictionary<string, List<Unlock>>();

            //foreach (Unlock unlock in GetAllUnlocksEnumerable().Where(x => IsModded(x.ID)))
            //{
            //    string modName = CustomGDO.GDOs[unlock.ID].ModName;
            //    if (!unlocksByModName.ContainsKey(modName))
            //    {
            //        unlocksByModName.Add(modName, new List<Unlock>());
            //    }
            //    unlocksByModName[modName].Add(unlock);
            //}
            //return unlocksByModName.Values.OrderByDescending(x => x.Count).SelectMany(x => x);
        }

        internal static string GetCardGroup(Unlock unlock)
        {
            return $"{(IsModded(unlock)? CustomGDO.GDOs[unlock.ID].ModName : "Vanilla")}:{unlock.UnlockGroup}:{unlock.CardType}";
        }

        internal static bool GetEnabledState(Unlock unlock)
        {
            try
            {
                return PreferenceUtils.Get<KitchenLib.BoolPreference>(Main.MOD_GUID, unlock.ID.ToString()).Value;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        internal static void SetColor(UnlockCardElement card, Color color)
        {
            card.Card.material.SetColor(Shader.PropertyToID("_Title"), color);
        }

        internal static Color GetColor(UnlockCardElement card)
        {
            return card.Card.material.GetColor(Shader.PropertyToID("_Title"));
        }
    }
}
