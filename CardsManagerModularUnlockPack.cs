﻿using KitchenData;
using KitchenLib.Customs;
using System.Collections.Generic;

namespace KitchenCardsManager
{
    internal class CardsManagerModularUnlockPack : CustomModularUnlockPack
    {
        public override string UniqueNameID { get { return Main.CARDS_MANAGER_MODULAR_UNLOCK_PACK_UNIQUENAMEID; } } // Setting the UniqueNameID ( Used to generate GDO ID )

        public override List<IUnlockSet> Sets
        {
            get
            {
                return new List<IUnlockSet>()
                {
                    new UnlockSetAutomatic()
                };
            }
        }

        public override List<IUnlockFilter> Filter
        {
            get
            {
                return new List<IUnlockFilter>()
                {
                    new FilterBasic()
                    {
                        IgnoreUnlockability = false,
                        IgnoreFranchiseTier = false,
                        IgnoreDuplicateFilter = false,
                        IgnoreRequirements = false,
                        AllowBaseDishes = true
                    },
                    new FilterByType()
                    {
                        AllowIfOnList = false,
                        Types = new List<CardType>()
                        {
                            CardType.FranchiseTier,
                            CardType.ThemeUnlock
                        }
                    }
                };
            }
        }

        public override List<IUnlockSorter> Sorters
        {
            get
            {
                return new List<IUnlockSorter>()
                {
                    new UnlockSorterShuffle(),
                    new UnlockSorterPriority()
                    {
                        PriorityProbability = 0.5f,
                        PrioritiseRequirements = true,
                        Groups = new List<UnlockGroup>() { },
                        DishTypes = new List<DishType>() { DishType.Main, DishType.Extra }
                    }
                };
            }
        }

        public override List<ConditionalOptions> ConditionalOptions
        {
            get
            {
                return new List<ConditionalOptions>()
                {
                    new ConditionalOptions()
                    {
                        Selector = new UnlockSelectorOtherPlusGroupChoice()
                        {
                            Group = UnlockGroup.Dish
                        },
                        Condition = new UnlockConditionRegular()
                        {
                            DayInterval = 3,
                            DayOffset = 0,
                            DayMin = 1,
                            DayMax = -1,
                            TierRequired = -1
                        }
                    }
                };
            }
        }
    }

    // Prioritizes first card not matching group, prioritizes second card matching group
    internal class UnlockSelectorOtherPlusGroupChoice : IUnlockSelector
    {
        internal UnlockGroup Group;

        public UnlockOptions GetOptions(List<Unlock> candidates, HashSet<int> current_cards, UnlockRequest request)
        {
            UnlockOptions result = default(UnlockOptions);
            for (int i = 0; i < candidates.Count; i++)
            {
                Unlock candidate = candidates[i];

                if (!(i == 0 && candidate.UnlockGroup == Group) && (result.Unlock1 == null || (candidate.UnlockGroup != Group && result.Unlock1.UnlockGroup == Group)))
                {
                    result.Unlock1 = candidate;
                }
                else if (result.Unlock2 == null || (candidate.UnlockGroup == Group && result.Unlock2.UnlockGroup != Group))
                {
                    result.Unlock2 = candidate;
                }
            }
            return result;
        }
    }
}
