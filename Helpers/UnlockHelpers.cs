using CodelessModInterop;
using Kitchen;
using KitchenData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KitchenCardsManager.Helpers
{
    internal static class UnlockHelpers
    {
        internal static bool IsModded(int id, out string modName)
        {
            return ModdedResourceRegistry.TryGetModdedGDO(id, out GameDataObject _, out modName);
        }

        internal static bool IsModded(GameDataObject gdo, out string modName)
        {
            return IsModded(gdo.ID, out modName);
        }

        internal static IEnumerable<Unlock> GetAllUnlocksEnumerable()
        {
            Dictionary<string, List<Unlock>> dict = new Dictionary<string, List<Unlock>>();
            foreach (Unlock unlock in GameData.Main.Get<Unlock>())
            {
                if (unlock is null)
                {
                    Main.LogInfo("Unlock is null! Skipping");
                    continue;
                }
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
            return GetAllUnlocksEnumerable().Where(x => !IsModded(x.ID, out string _));
        }

        internal static IEnumerable<Unlock> GetAllModdedUnlocksEnumerable()
        {
            return GetAllUnlocksEnumerable().Where(x => IsModded(x.ID, out string _));
        }

        internal static string GetCardGroup(Unlock unlock)
        {
            return $"{(IsModded(unlock, out string modName)? modName : "Vanilla")}:{unlock.UnlockGroup}:{unlock.CardType}";
        }

        internal static bool GetEnabledState(Unlock unlock)
        {
            try
            {
                return Main.PrefManager.Get<bool>(unlock.ID.ToString());
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

        internal static bool IsFranchiseCard(int unlockID)
        {
            return GetAllUnlocksEnumerable().Where(x => x.ID == unlockID).First().CardType == CardType.FranchiseTier;
        }
    }
}
