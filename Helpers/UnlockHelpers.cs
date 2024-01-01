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
            string group = IsModded(unlock, out string modName) ? modName : "Vanilla";
            switch (Main.PrefManager.Get<int>(Main.DISH_CARDS_GROUPING_ID))
            {
                case 1:
                    group += $":{(unlock is Dish dish ? dish.Type.ToString() : "UnlockCard")}";
                    break;
                default:
                    break;
            }
            group += $":{unlock.UnlockGroup}:{unlock.CardType}";
            return group;
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

        internal static bool GetRequirementsAutoAddState(Unlock unlock)
        {
            return Direct_GetRequirementsAutoAddState(unlock);
        }

        private static bool Direct_GetRequirementsAutoAddState(Unlock unlock, int depth = 0)
        {
            try
            {
                if (unlock == null || depth >= 10)
                    return false;
                foreach (Unlock requirement in unlock.Requires)
                {
                    if (!GetAutoAddState(requirement))
                        return false;
                    if (GetBlockedBysAutoAddState(requirement))
                        return false;
                    foreach (Unlock innerRequirement in requirement.Requires)
                    {
                        if (!Direct_GetRequirementsAutoAddState(innerRequirement, depth + 1))
                            return false;
                    }
                }
                return true;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        internal static bool GetBlockedBysAutoAddState(Unlock unlock)
        {
            try
            {
                if (unlock == null)
                    return false;
                foreach (Unlock blocker in unlock.BlockedBy)
                {
                    if (GetAutoAddState(blocker))
                        return true;
                }
                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        internal static bool GetAutoAddState(Unlock unlock)
        {
            try
            {
                return Main.PrefManager.Get<bool>(unlock.ID.ToString() + "_AutoAdd");
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        internal static bool ShouldBeAutoAdded(Unlock unlock)
        {
            return GetAutoAddState(unlock) &&
                (!Main.PrefManager.Get<bool>(Main.CARDS_MANAGER_ADD_REMOVE_VALIDITY_CHECKING) ||
                (GetDefaultEnabledState(unlock) &&
                GetRequirementsAutoAddState(unlock) &&
                !GetBlockedBysAutoAddState(unlock)));
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

        internal static bool GetDefaultEnabledState(Unlock unlock)
        {
            return unlock.IsUnlockable || Main.StartingUnlocks.Contains(unlock.ID);
        }

        internal static void SetAutoAddIndicator(UnlockCardState cardState, Unlock unlock)
        {
            cardState.UpdateAutoAdd(unlock);
        }
    }
}
