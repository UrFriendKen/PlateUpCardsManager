using KitchenData;
using KitchenLib.Customs;
using System.Collections.Generic;
using KitchenLib.References;
using KitchenLib.Utils;

namespace KitchenCardsManager
{
    internal class CardsManagerCompositeUnlockPack : CustomCompositeUnlockPack
    {
        protected const int FRANCHISE_CARDS_PACK_ID = ModularUnlockPackReferences.FranchiseCardsPack;
        protected const int THEME_CARDS_PACK_ID = ModularUnlockPackReferences.ThemeCardsPack;

        public override string UniqueNameID { get { return Main.CARDS_MANAGER_COMPOSITE_UNLOCK_PACK_UNIQUENAMEID; } } // Setting the UniqueNameID ( Used to generate GDO ID )
        public override List<UnlockPack> Packs => new List<UnlockPack>()
        {
            (ModularUnlockPack)GDOUtils.GetExistingGDO(ModularUnlockPackReferences.FranchiseCardsPack),
            (ModularUnlockPack)GDOUtils.GetCustomGameDataObject<CardsManagerModularUnlockPack>().GameDataObject,
            (ModularUnlockPack)GDOUtils.GetExistingGDO(ModularUnlockPackReferences.ThemeCardsPack)
        };
        //{
        //    get
        //    {
        //        Main.LogInfo("------------------------------------------------------------------------------------------");
        //        Main.LogInfo("Creating All Cards CompositeUnlockPack");
        //        Main.LogInfo("");

        //        //List<int> packIDs = new List<int>()
        //        //{
        //        //    ModularUnlockPackReferences.FranchiseCardsPack,
        //        //    GDOUtils.GetCustomGameDataObject<CardsManagerModularUnlockPack>().ID,
        //        //    ModularUnlockPackReferences.ThemeCardsPack
        //        //};

        //        List<UnlockPack> packs = new List<UnlockPack>()
        //        {
        //            (ModularUnlockPack)GDOUtils.GetExistingGDO(ModularUnlockPackReferences.FranchiseCardsPack),
        //            (ModularUnlockPack)GDOUtils.GetCustomGameDataObject<CardsManagerModularUnlockPack>().GameDataObject,
        //            (ModularUnlockPack)GDOUtils.GetExistingGDO(ModularUnlockPackReferences.ThemeCardsPack)
        //        };

        //        //List<UnlockPack> packs = GameData.Main.Get<ModularUnlockPack>().Where(x => packIDs.Contains(x.ID)).Cast<UnlockPack>().ToList();
        //        foreach (ModularUnlockPack modularpack in packs)
        //        {
        //            Main.LogInfo($"Adding ModularUnlockPack - {modularpack.name} ({modularpack.ID})");
        //            LogSets(modularpack.Sets);
        //            LogFilters(modularpack.Filter);
        //            LogSorters(modularpack.Sorters);
        //            LogConditionalOptions(modularpack.ConditionalOptions);
        //            Main.LogInfo("");
        //            //packs.Add(modularpack);
        //        }
        //        Main.LogInfo($"Completed Pack. Number of modular packs = {packs.Count}");
        //        Main.LogInfo("------------------------------------------------------------------------------------------");
        //        return packs;
        //    }
        //}

        private void LogSets(List<IUnlockSet> sets)
        {
            Main.LogInfo("Sets:");
            for (int i = 0; i < sets.Count; i++)
            {
                Main.LogInfo($"{i + 1}: {sets[i].GetType()}");
                if (sets[i].GetType() == typeof(UnlockSetFixed))
                {
                    foreach (Unlock unlock in ((UnlockSetFixed)sets[i]).Unlocks)
                    {
                        Main.LogInfo($"{unlock.Name} ({unlock.ID})");
                    }
                }
                else if (sets[i].GetType() == typeof(UnlockSetAutomatic))
                {
                    Main.LogInfo("Uses all registered cards in the game (including Modded Cards).");
                }
            }
        }
        
        private void LogFilters(List<IUnlockFilter> filters)
        {
            Main.LogInfo("Filters:");
            for (int i = 0; i < filters.Count; i++)
            {
                Main.LogInfo($"{i + 1}: {filters[i].GetType()}");
                if (filters[i].GetType() == typeof(FilterBasic))
                {
                    FilterBasic filter = (FilterBasic)filters[i];
                    Main.LogInfo($"IgnoreUnlockability = {filter.IgnoreUnlockability}");
                    Main.LogInfo($"IgnoreFranchiseTier = {filter.IgnoreFranchiseTier}");
                    Main.LogInfo($"IgnoreDuplicateFilter = {filter.IgnoreDuplicateFilter}");
                    Main.LogInfo($"IgnoreRequirements = {filter.IgnoreRequirements}");
                    Main.LogInfo($"AllowBaseDishes = {filter.AllowBaseDishes}");
                }
                else if (filters[i].GetType() == typeof(FilterByType))
                {
                    FilterByType filter = (FilterByType)filters[i];
                    Main.LogInfo($"AllowIfOnList = {filter.AllowIfOnList}");
                    string allowedTypes = "Types = { ";
                    for (int j = 0; j < filter.Types.Count; j++)
                    {
                        allowedTypes += filter.Types[j].ToString();
                        if (j < filter.Types.Count - 1)
                        {
                            allowedTypes += ",";
                        }
                        allowedTypes += " ";
                    }
                    allowedTypes += "}";
                    Main.LogInfo(allowedTypes);
                }
            }
        }

        private void LogSorters(List<IUnlockSorter> sorters)
        {
            Main.LogInfo("Sorters:");
            for (int i = 0; i < sorters.Count; i++)
            {
                Main.LogInfo($"{i + 1}: {sorters[i].GetType()}");
                if (sorters[i].GetType() == typeof(UnlockSorterEncourageCard))
                {
                    UnlockSorterEncourageCard sorter = (UnlockSorterEncourageCard)sorters[i];
                    Main.LogInfo($"PriorityProbability = {sorter.PriorityProbability}");
                    string cards = "Cards = { ";
                    for (int j = 0; j < sorter.Cards.Count; j++)
                    {
                        Unlock unlock = sorter.Cards[j];
                        cards += $"{unlock.Name} ({unlock.ID})";
                        if (j < sorter.Cards.Count - 1)
                        {
                            cards += ",";
                        }
                        cards += " ";
                    }
                    cards += "}";
                    Main.LogInfo(cards);
                }
                else if (sorters[i].GetType() == typeof(UnlockSorterPriority))
                {
                    UnlockSorterPriority sorter = (UnlockSorterPriority)sorters[i];
                    Main.LogInfo($"PriorityProbability = {sorter.PriorityProbability}");
                    Main.LogInfo($"PrioritiseRequirements = {sorter.PrioritiseRequirements}");
                    string groups = "Groups = { ";
                    for (int j = 0; j < sorter.Groups.Count; j++)
                    {
                        groups += sorter.Groups[j].ToString();
                        if (j < sorter.Groups.Count - 1)
                        {
                            groups += ",";
                        }
                        groups += " ";
                    }
                    groups += "}";
                    Main.LogInfo(groups);

                    string dishTypes = "DishTypes = { ";
                    for (int j = 0; j < sorter.DishTypes.Count; j++)
                    {
                        dishTypes += sorter.DishTypes[j].ToString();
                        if (j < sorter.DishTypes.Count - 1)
                        {
                            dishTypes += ",";
                        }
                        dishTypes += " ";
                    }
                    dishTypes += "}";
                    Main.LogInfo(dishTypes);
                }
                else if (sorters[i].GetType() == typeof(UnlockSorterShuffle))
                {
                    Main.LogInfo("Shuffles card randomly using UnityEngine.Random.value.");
                }
            }
        }

        private void LogConditionalOptions(List<ConditionalOptions> conditionalOptions)
        {
            Main.LogInfo("Conditions:");
            for (int i = 0; i < conditionalOptions.Count; i++)
            {
                Main.LogInfo($"ConditionalOptions {i + 1}");
                
                Main.LogInfo($"Selector = {conditionalOptions[i].Selector.GetType()}");
                if (conditionalOptions[i].Selector.GetType() == typeof(UnlockSelectorGroupChoice))
                {
                    UnlockSelectorGroupChoice selector = (UnlockSelectorGroupChoice)conditionalOptions[i].Selector;
                    Main.LogInfo($"Group1 = {selector.Group1}");
                    Main.LogInfo($"Group2 = {selector.Group2}");
                }
                else if (conditionalOptions[i].Selector.GetType() == typeof(UnlockSelectorForcedCard))
                {
                    Main.LogInfo("Always returns first option in list of cards after sorting.");
                }

                Main.LogInfo($"Condition = {conditionalOptions[i].Condition.GetType()}");
                if (conditionalOptions[i].Condition.GetType() == typeof(UnlockConditionGuarantee))
                {
                    UnlockConditionGuarantee selector = (UnlockConditionGuarantee)conditionalOptions[i].Condition;
                    Main.LogInfo($"MinDay = {selector.MinDay}");
                    Main.LogInfo($"MinTier = {selector.MinTier}");
                    Main.LogInfo($"GuaranteedGroup = {selector.GuaranteedGroup}");
                }
                else if (conditionalOptions[i].Condition.GetType() == typeof(UnlockConditionOnce))
                {
                    UnlockConditionOnce selector = (UnlockConditionOnce)conditionalOptions[i].Condition;
                    Main.LogInfo($"Day = {selector.Day}");
                }
                else if (conditionalOptions[i].Condition.GetType() == typeof(UnlockConditionRegular))
                {
                    UnlockConditionRegular selector = (UnlockConditionRegular)conditionalOptions[i].Condition;
                    Main.LogInfo($"DayInterval = {selector.DayInterval}");
                    Main.LogInfo($"DayOffset = {selector.DayOffset}");
                    Main.LogInfo($"DayMin = {selector.DayMin}");
                    Main.LogInfo($"DayMax = {selector.DayMax}");
                    Main.LogInfo($"TierRequired = {selector.TierRequired}");
                }
            }
        }

        public override void OnRegister(GameDataObject gameDataObject)
        {
            Main.LogInfo("Registered: " + this.UniqueNameID);
        }
    }
}
