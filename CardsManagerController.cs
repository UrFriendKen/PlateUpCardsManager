using Kitchen;
using Kitchen.ShopBuilder;
using KitchenCardsManager.Customs;
using KitchenCardsManager.Helpers;
using KitchenData;
using KitchenLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenCardsManager
{
    internal class CardsManagerController : GameSystemBase
    {
        MethodInfo EntityManagerGetComponentData;

        internal static bool IsInKitchen { get => GameInfo.CurrentScene == SceneType.Kitchen; }

        internal static SceneType CurrentScene { get => GameInfo.CurrentScene; }
        internal static HashSet<int> CurrentUnlockIDs { get => GameInfo.AllCurrentCards.Select(x => x.CardID).ToHashSet(); }

        protected static Queue<int> UnlocksToAdd = new Queue<int>();

        protected static Queue<int> UnlocksToRemove = new Queue<int>();

        static HashSet<Type> SupportedEffects = new HashSet<Type>
        {
            typeof(ParameterEffect),
            typeof(StatusEffect),
            typeof(ThemeAddEffect),
            typeof(ShopEffect),
            typeof(StartBonusEffect),
            typeof(EnableGroupEffect),
            typeof(CustomerSpawnEffect),
            typeof(GlobalEffect)
        };
        
        static HashSet<Type> SupportedRemoveEffectTypes = new HashSet<Type>
        {
            typeof(CCabinetModifier),           // Confirmed functional
            typeof(CApplianceSpeedModifier),    // Confirmed functional
            typeof(CAppliesStatus),
            typeof(CTableModifier),             // Confirmed functional
            typeof(CQueueModifier)
        };
        static HashSet<Type> SupportedRemoveEffectConditions = new HashSet<Type>
        {
            typeof(CEffectAlways),  // Confirmed functional
            typeof(CEffectWhileBeingUsed),
            typeof(CEffectAtNight)  // Confirmed functional
        };

        EntityQuery ActiveUnlocks;
        EntityQuery ActiveDishes;
        EntityQuery GlobalEffects;
        EntityQuery BonusStaples;
        EntityQuery RemoveBlueprints;
        EntityQuery ShopDiscounts;
        EntityQuery Duplicators;
        EntityQuery RandomiseShopPrices;
        EntityQuery UpgradedShops;
        EntityQuery Refreshes;
        EntityQuery Rebuyables;
        EntityQuery CustomerTypes;
        EntityQuery SpawnModifiers;

        NativeArray<Entity> GlobalEffectEntities;
        HashSet<int> DestroyedGlobalEffects;

        NativeArray<Entity> BonusStapleEntities;
        HashSet<int> DestroyedBonusStaples;

        NativeArray<Entity> RemoveBlueprintEntities;
        HashSet<int> DestroyedRemoveBlueprints;

        NativeArray<Entity> ShopDiscountEntities;
        HashSet<int> DestroyedShopDiscounts;

        NativeArray<Entity> DuplicatorEntities;
        int DestroyedDuplicatorsCount;

        NativeArray<Entity> RandomiseShopPricesEntities;
        int DestroyedRandomiseShopPricesCount;

        NativeArray<Entity> UpgradedShopEntities;
        HashSet<int> DestroyedUpgradedShops;

        NativeArray<Entity> RefreshEntities;
        HashSet<int> DestroyedRefreshes;

        NativeArray<Entity> RebuyableEntities;
        HashSet<int> DestroyedRebuyables;

        NativeArray<Entity> CustomerTypeEntities;
        HashSet<int> DestroyedCustomerTypes;

        NativeArray<Entity> SpawnModifierEntities;
        HashSet<int> DestroyedSpawnModifiers;

        protected override void Initialise()
        {
            base.Initialise();
            EntityManagerGetComponentData = typeof(EntityManager).GetMethod("GetComponentData", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Entity) }, null);

            ActiveUnlocks = GetEntityQuery(typeof(CProgressionUnlock));
            ActiveDishes = GetEntityQuery(typeof(CMenuItem));

            GlobalEffects = GetEntityQuery(typeof(CAppliesEffect), typeof(CEffectRangeGlobal));
            DestroyedGlobalEffects = new HashSet<int>();

            BonusStaples = GetEntityQuery(typeof(CShopStaple));
            DestroyedBonusStaples = new HashSet<int>();

            RemoveBlueprints = GetEntityQuery(typeof(CRemovesShopBlueprint));
            DestroyedRemoveBlueprints = new HashSet<int>();

            ShopDiscounts = GetEntityQuery(typeof(CGrantsShopDiscount));
            DestroyedShopDiscounts = new HashSet<int>();

            Duplicators = GetEntityQuery(typeof(CBlueprintGrantDuplicator));
            DestroyedDuplicatorsCount = 0;

            RandomiseShopPrices = GetEntityQuery(typeof(CRandomiseShopPrices));
            DestroyedRandomiseShopPricesCount = 0;

            UpgradedShops = GetEntityQuery(typeof(CUpgradedShopChance));
            DestroyedUpgradedShops = new HashSet<int>();

            Refreshes = GetEntityQuery(typeof(CBlueprintRefreshChance));
            DestroyedRefreshes = new HashSet<int>();

            Rebuyables = GetEntityQuery(typeof(CBlueprintRebuyableChance));
            DestroyedRebuyables = new HashSet<int>();

            CustomerTypes = GetEntityQuery(typeof(CCustomerType), typeof(CCustomerSpawnDefinition));
            DestroyedCustomerTypes = new HashSet<int>();

            SpawnModifiers = GetEntityQuery(typeof(CCustomerSpawnModifier));
            DestroyedSpawnModifiers = new HashSet<int>();
        }

        protected virtual void BeforeRun()
        {
            GlobalEffectEntities = GlobalEffects.ToEntityArray(Allocator.Temp);
            DestroyedGlobalEffects.Clear();

            BonusStapleEntities = BonusStaples.ToEntityArray(Allocator.Temp);
            DestroyedBonusStaples.Clear();

            RemoveBlueprintEntities = RemoveBlueprints.ToEntityArray(Allocator.Temp);
            DestroyedRemoveBlueprints.Clear();

            ShopDiscountEntities = ShopDiscounts.ToEntityArray(Allocator.Temp);
            DestroyedShopDiscounts.Clear();

            DuplicatorEntities = Duplicators.ToEntityArray(Allocator.Temp);
            DestroyedDuplicatorsCount = 0;

            RandomiseShopPricesEntities = RandomiseShopPrices.ToEntityArray(Allocator.Temp);
            DestroyedRandomiseShopPricesCount = 0;

            UpgradedShopEntities = UpgradedShops.ToEntityArray(Allocator.Temp);
            DestroyedUpgradedShops.Clear();

            RefreshEntities = Refreshes.ToEntityArray(Allocator.Temp);
            DestroyedRefreshes.Clear();

            RebuyableEntities = Rebuyables.ToEntityArray(Allocator.Temp);
            DestroyedRebuyables.Clear();

            CustomerTypeEntities = CustomerTypes.ToEntityArray(Allocator.Temp);
            DestroyedCustomerTypes.Clear();

            SpawnModifierEntities = SpawnModifiers.ToEntityArray(Allocator.Temp);
            DestroyedSpawnModifiers.Clear();
        }

        protected virtual void AfterRun()
        {
            GlobalEffectEntities.Dispose();
            BonusStapleEntities.Dispose();
            RemoveBlueprintEntities.Dispose();
            ShopDiscountEntities.Dispose();
            DuplicatorEntities.Dispose();
            RandomiseShopPricesEntities.Dispose();
            UpgradedShopEntities.Dispose();
            RefreshEntities.Dispose();
            RebuyableEntities.Dispose();
            CustomerTypeEntities.Dispose();
            SpawnModifierEntities.Dispose();
        }

        protected override void OnUpdate()
        {
            BeforeRun();
            if (UnlocksToAdd.Count > 0)
            {
                int unlockID = UnlocksToAdd.Dequeue();
                if (UnlockHelpers.IsValidUnlock(unlockID))
                {
                    Entity entity = EntityManager.CreateEntity();
                    Set(entity, new CProgressionOption()
                    {
                        ID = unlockID,
                        FromFranchise = UnlockHelpers.IsFranchiseCard(unlockID)
                    });
                    Set<CSkipShowingRecipe>(entity);
                    Set<CProgressionOption.Selected>(entity);
                }
            }

            if (UnlocksToRemove.Count > 0)
            {
                int unlockID = UnlocksToRemove.Dequeue();
                if (UnlockHelpers.IsValidUnlock(unlockID))
                {
                    if (!TryRemoveActiveUnlock(unlockID))
                    {
                        Main.LogInfo($"Failed to remove unlock ({unlockID})");
                    }
                    else
                    {
                        Main.LogInfo($"Removed {GameData.Main.Get<Unlock>(unlockID).name} ({unlockID})");
                    }
                }
            }
            AfterRun();
        }

        private bool TryRemoveActiveUnlock(int unlockID)
        {
            if (!GameData.Main.TryGet(unlockID, out Unlock unlock))
                return false;

            using NativeArray<Entity> activeUnlocks = ActiveUnlocks.ToEntityArray(Allocator.Temp);
            using NativeArray<CProgressionUnlock> progressions = ActiveUnlocks.ToComponentDataArray<CProgressionUnlock>(Allocator.Temp);
            using NativeArray<Entity> activeDishes = ActiveDishes.ToEntityArray(Allocator.Temp);
            using NativeArray<CMenuItem> menuItems = ActiveDishes.ToComponentDataArray<CMenuItem>(Allocator.Temp);

            List<UnlockEffect> residualEffects = new List<UnlockEffect>();
            HashSet<int> menuItemsIndicesToDestroy = new HashSet<int>();
            for (int i = activeUnlocks.Length - 1; i > -1; i--)
            {
                Entity entity = activeUnlocks[i];
                CProgressionUnlock progression = progressions[i];
                if (!GameData.Main.TryGet(progression.ID, out Unlock unlock2))
                    continue;

                if (progression.ID != unlockID && !unlock2.Requires.Contains(unlock))
                {
                    continue;
                }

                if (unlock2 is UnlockCard unlockCard)
                {
                    residualEffects.AddRange(unlockCard.Effects);
                }
                else if (unlock2 is Dish dish)
                {
                    for (int j = 0; j < menuItems.Length; j++)
                    {
                        if (dish.ID != menuItems[i].SourceDish)
                            continue;
                        menuItemsIndicesToDestroy.Add(j);
                        break;
                    }
                }
                else
                {
                    Main.LogInfo($"Unknown Type derived from unlock ({unlock2.GetType()}). Skipping");
                    continue;
                }
                EntityManager.DestroyEntity(entity);
            }

            foreach(int i in menuItemsIndicesToDestroy)
            {
                EntityManager.DestroyEntity(activeDishes[i]);
            }

            UndoUnlockEffects(residualEffects);

            return true;
        }

        private void UndoUnlockEffects(List<UnlockEffect> unlockEffects)
        {
            foreach (UnlockEffect unlockEffect in unlockEffects)
            {
                Type unlockEffectType = unlockEffect.GetType();
                if (!(unlockEffect is ParameterEffect))
                {
                    if (!(unlockEffect is GlobalEffect))
                    {
                        if (!(unlockEffect is StatusEffect statusEffect))
                        {
                            if (!(unlockEffect is ThemeAddEffect themeAddEffect))
                            {
                                if (!(unlockEffect is ShopEffect effect))
                                {
                                    if (!(unlockEffect is StartBonusEffect effect2))
                                    {
                                        if (!(unlockEffect is EnableGroupEffect enableGroupEffect))
                                        {
                                            if (unlockEffect is CustomerSpawnEffect customerSpawnEffect)
                                            {
                                                UndoCustomerSpawnEffect(customerSpawnEffect);
                                            }
                                        }
                                        else
                                        {
                                            UndoEnableGroupEffect(enableGroupEffect);
                                        }
                                    }
                                    else
                                    {
                                        UndoStartBonusEffect(effect2);
                                    }
                                }
                                else
                                {
                                    UndoShopEffect(effect);
                                }
                            }
                            else
                            {
                                RemoveTheme(themeAddEffect.AddsTheme);
                            }
                        }
                        else
                        {
                            RemoveStatus(statusEffect.Status);
                        }
                    }
                    else
                    {
                        UndoGlobalEffect((GlobalEffect)unlockEffect);
                    }
                }
                else
                {
                    UndoParameterEffect((ParameterEffect)unlockEffect);
                }
            }
        }

        private bool UndoParameterEffect(ParameterEffect effect)
        {
            VariableParameterUnlockCard unlockCard = GDOUtils.GetCustomGameDataObject<VariableParameterUnlockCard>() as VariableParameterUnlockCard;
            unlockCard.UpdateParameterEffect(new KitchenParameters
            {
                CustomersPerHour = -effect.Parameters.CustomersPerHour,
                CustomersPerHourReduction = -effect.Parameters.CustomersPerHourReduction,
                MaximumGroupSize = -effect.Parameters.MaximumGroupSize,
                MinimumGroupSize = -effect.Parameters.MinimumGroupSize,
                CurrentCourses = -effect.Parameters.CurrentCourses
            });

            Set(EntityManager.CreateEntity(), new CNewParameterChange
            {
                ID = GDOUtils.GetCustomGameDataObject<VariableParameterUnlockCard>().ID,
                Index = 0
            });
            return false;
        }

        private bool UndoGlobalEffect(GlobalEffect effect)
        {
            for (int i = 0; i < GlobalEffectEntities.Length; i++)
            {
                if (DestroyedGlobalEffects.Contains(i))
                    continue;

                if (Require(GlobalEffectEntities[i], effect.EffectCondition.GetType(), out var conditionComp) && IsEqualCondition(conditionComp, effect.EffectCondition) &&
                    Require(GlobalEffectEntities[i], effect.EffectType.GetType(), out var typeComp) && IsEqualType(typeComp, effect.EffectType))
                {
                    DestroyedGlobalEffects.Add(i);
                    EntityManager.DestroyEntity(GlobalEffectEntities[i]);
                    break;
                }
            }
            return true;
        }

        private bool Require(Entity e, Type type, out object comp)
        {
            comp = null;
            if (!EntityManager.HasComponent(e, type))
            {
                return false;
            }
            MethodInfo getComponentDataGeneric = EntityManagerGetComponentData.MakeGenericMethod(type);
            comp = getComponentDataGeneric.Invoke(EntityManager, new object[] { e });
            return true;
        }

        private bool IsEqualCondition(object obj, object check)
        {
            if (obj.GetType() != check.GetType())
            {
                return false;
            }

            if (obj.GetType() == typeof(CEffectAlways))
                return EffectConditionComparer.CEffectAlwaysComparer.CompareEqual(obj, check);
            if (obj.GetType() == typeof(CEffectWhileBeingUsed))
                return EffectConditionComparer.CEffectWhileBeingUsedComparer.CompareEqual(obj, check);
            if (obj.GetType() == typeof(CEffectAtNight))
                return EffectConditionComparer.CEffectAtNightComparer.CompareEqual(obj, check);

            return false;
        }

        private bool IsEqualType(object obj, object check)
        {
            if (obj.GetType() != check.GetType())
            {
                return false;
            }

            if (obj.GetType() == typeof(CCabinetModifier))
                return EffectTypeComparer.CCabinetModifierComparer.CompareEqual(obj, check);
            if (obj.GetType() == typeof(CApplianceSpeedModifier))
                return EffectTypeComparer.CApplianceSpeedModifierComparer.CompareEqual(obj, check);
            if (obj.GetType() == typeof(CAppliesStatus))
                return EffectTypeComparer.CAppliesStatusComparer.CompareEqual(obj, check);
            if (obj.GetType() == typeof(CTableModifier))
                return EffectTypeComparer.CTableModifierComparer.CompareEqual(obj, check);
            if (obj.GetType() == typeof(CQueueModifier))
                return EffectTypeComparer.CQueueModifierComparer.CompareEqual(obj, check);

            return false;
        }

        private bool RemoveTheme(DecorationType theme)
        {
            SGlobalStatusList orCreate = GetOrCreate<SGlobalStatusList>();
            if (!orCreate.Theme.HasFlagFast(theme))
                return false;
            orCreate.Theme ^= theme;
            SetSingleton(orCreate);
            return true;
        }

        private bool UndoShopEffect(ShopEffect effect)
        {
            if (effect.AddStaple != null)
            {
                for (int i = 0; i < BonusStapleEntities.Length; i++)
                {
                    if (DestroyedBonusStaples.Contains(i))
                        continue;

                    if (Require(BonusStapleEntities[i], out CShopStaple staple) && staple.Appliance == effect.AddStaple.ID)
                    {
                        DestroyedBonusStaples.Add(i);
                        EntityManager.DestroyEntity(BonusStapleEntities[i]);
                        break;
                    }
                }
            }

            if (effect.BlueprintDeskAddition > 0 && DestroyedDuplicatorsCount < DuplicatorEntities.Length)
            {
                for (int i = Mathf.Min(DuplicatorEntities.Length, effect.BlueprintDeskAddition) - DestroyedDuplicatorsCount - 1; i > -1 && DestroyedDuplicatorsCount < DuplicatorEntities.Length; i--)
                {
                    EntityManager.DestroyEntity(DuplicatorEntities[i]);
                    DestroyedDuplicatorsCount++;
                }
            }

            if (effect.ExtraShopBlueprints != 0)
            {
                for (int i = 0; i < RemoveBlueprintEntities.Length; i++)
                {
                    if (DestroyedRemoveBlueprints.Contains(i))
                        continue;

                    if (Require(RemoveBlueprintEntities[i], out CRemovesShopBlueprint removes) && removes.Count == -effect.ExtraShopBlueprints)
                    {
                        DestroyedRemoveBlueprints.Add(i);
                        EntityManager.DestroyEntity(RemoveBlueprintEntities[i]);
                        break;
                    }
                }
            }

            if (effect.ShopCostDecrease != 0f)
            {
                for (int i = 0; i < ShopDiscountEntities.Length; i++)
                {
                    if (DestroyedShopDiscounts.Contains(i))
                        continue;

                    if (Require(ShopDiscountEntities[i], out CGrantsShopDiscount discount) && discount.Amount == effect.ShopCostDecrease)
                    {
                        DestroyedShopDiscounts.Add(i);
                        EntityManager.DestroyEntity(ShopDiscountEntities[i]);
                        break;
                    }
                }
            }

            if (effect.RandomiseShopPrices)
            {
                if (RandomiseShopPricesEntities.Length - DestroyedRandomiseShopPricesCount > 0)
                {
                    EntityManager.DestroyEntity(RandomiseShopPricesEntities[RandomiseShopPricesEntities.Length - DestroyedRandomiseShopPricesCount - 1]);
                    DestroyedRandomiseShopPricesCount++;
                }
            }

            if (effect.ExtraStartingBlueprints > 0)
            {
            }

            if (effect.UpgradedShopChance > 0f)
            {
                for (int i = 0; i < UpgradedShopEntities.Length; i++)
                {
                    if (DestroyedUpgradedShops.Contains(i))
                        continue;

                    if (Require(UpgradedShopEntities[i], out CUpgradedShopChance chance) && chance.Chance == effect.UpgradedShopChance)
                    {
                        DestroyedUpgradedShops.Add(i);
                        EntityManager.DestroyEntity(UpgradedShopEntities[i]);
                        break;
                    }
                }
            }

            if (effect.BlueprintRefreshChance > 0f)
            {
                for (int i = 0; i < RefreshEntities.Length; i++)
                {
                    if (DestroyedRefreshes.Contains(i))
                        continue;

                    if (Require(RefreshEntities[i], out CBlueprintRefreshChance chance) && chance.Chance == effect.BlueprintRefreshChance)
                    {
                        DestroyedRefreshes.Add(i);
                        EntityManager.DestroyEntity(RefreshEntities[i]);
                        break;
                    }
                }
            }

            if (effect.BlueprintRebuyableChance > 0f)
            {
                for (int i = 0; i < RebuyableEntities.Length; i++)
                {
                    if (DestroyedRebuyables.Contains(i))
                        continue;

                    if (Require(RebuyableEntities[i], out CBlueprintRebuyableChance chance) && chance.Chance == effect.BlueprintRebuyableChance)
                    {
                        DestroyedRebuyables.Add(i);
                        EntityManager.DestroyEntity(RebuyableEntities[i]);
                        break;
                    }
                }
            }
            return true;
        }

        private bool UndoStartBonusEffect(StartBonusEffect effect)
        {
            return true;
        }

        private bool UndoEnableGroupEffect(EnableGroupEffect effect)
        {
            for (int i = 0; i < CustomerTypeEntities.Length; i++)
            {
                if (DestroyedCustomerTypes.Contains(i))
                    continue;

                if (Require(CustomerTypeEntities[i], out CCustomerType customerType) && customerType.Type == effect.EnableType.ID &&
                    Require(CustomerTypeEntities[i], out CCustomerSpawnDefinition spawnDefinition) && spawnDefinition.Probability == effect.Probability)
                {
                    DestroyedCustomerTypes.Add(i);
                    EntityManager.DestroyEntity(CustomerTypeEntities[i]);
                    break;
                }
            }
            return true;
        }

        private bool UndoCustomerSpawnEffect(CustomerSpawnEffect effect)
        {
            for (int i = 0; i < SpawnModifierEntities.Length; i++)
            {
                if (DestroyedSpawnModifiers.Contains(i))
                    continue;

                if (Require(SpawnModifierEntities[i], out CCustomerSpawnModifier modifier) && modifier.BaseCustomerMultiplier.Value == effect.Base.Value &&
                    modifier.PerDayCustomerMultiplier.Value == effect.PerDay.Value)
                {
                    DestroyedSpawnModifiers.Add(i);
                    EntityManager.DestroyEntity(SpawnModifierEntities[i]);
                    break;
                }
            }
            return true;
        }

        internal static bool AddProgressionUnlock(int unlockID, out string statusMessage)
        {
            if (CanBeAddedToRun(unlockID, out statusMessage))
            {
                UnlocksToAdd.Enqueue(unlockID);
                return true;
            }
            return false;
        }

        internal static bool RemoveProgressionUnlock(int unlockID, out string statusMessage)
        {
            if (CanBeRemovedFromRun(unlockID, out statusMessage))
            {
                UnlocksToRemove.Enqueue(unlockID);
                return true;
            }
            return true;
        }

        internal static bool CanBeAddedToRun(int unlockID, out string statusMessage)
        {
            if (CurrentScene != SceneType.Kitchen)
            {
                statusMessage = "You must be in a restaurant to add cards!";
                return false;
            }
            Unlock unlock = UnlockHelpers.GetAllUnlocksEnumerable().Where(x => x.ID == unlockID).First();

            if (CurrentUnlockIDs.Contains(unlockID) || UnlocksToAdd.Contains(unlockID))
            {
                statusMessage = $"{unlock.Name} already added!";
                return false;
            }
            if (!unlock.IsUnlockable && !Main.StartingUnlocks.Contains(unlock.ID))
            {
                statusMessage = $"{unlock.Name} is not unlockable!";
                return false;
            }
            if (unlock.CardType == CardType.FranchiseTier)
            {
                if ((unlock is UnlockCard unlockCard) && unlockCard.Effects.Select(x => x.GetType()).Where(type => !SupportedEffects.Contains(type)).Count() > 0)
                {
                    statusMessage = $"You can only get {unlock.Name} by franchising!";
                    return false;
                }
            }
            if (GameInfo.AllCurrentCards.Intersect(unlock.Requires).Count() != unlock.Requires.Count())
            {
                statusMessage = "Requirements not met!\nYou must have:";
                foreach (Unlock require in unlock.Requires)
                {
                    statusMessage += $"\n- {require.Name}";
                }
                return false;
            }
            if (GameInfo.AllCurrentCards.Intersect(unlock.BlockedBy).Count() > 0)
            {
                statusMessage = $"Cannot add {unlock.Name}!\nIt is blocked by:";
                foreach (Unlock blocker in unlock.BlockedBy)
                {
                    statusMessage += $"\n- {blocker.Name}";
                }
                return false;
            }
            statusMessage = $"Successfully added {unlock.Name}";
            return true;
        }

        internal static bool CanBeRemovedFromRun(int unlockID, out string statusMessage)
        {

            if (CurrentScene != SceneType.Kitchen)
            {
                statusMessage = "You must be in a restaurant to add cards!";
                return false;
            }
            Unlock unlock = UnlockHelpers.GetAllUnlocksEnumerable().Where(x => x.ID == unlockID).First();

            if (!CurrentUnlockIDs.Contains(unlockID) || UnlocksToRemove.Contains(unlockID))
            {
                statusMessage = $"{unlock.Name} is not active!";
                return false;
            }
            if (unlock is UnlockCard unlockCard)
            {
                IEnumerable<Type> unsupportedUnlockEffects = unlockCard.Effects.Select(x => x.GetType()).Where(type => !SupportedEffects.Contains(type));
                if (unsupportedUnlockEffects.Count() > 0)
                {
                    statusMessage = "Contains unsupported effects!";
                    foreach (Type type in unsupportedUnlockEffects)
                    {
                        statusMessage += $"\n- {type.Name}";
                    }
                    return false;
                }

                bool hasUnsupportedGlobalEffect = false;
                statusMessage = "Contains unsupported global effects!";
                IEnumerable<GlobalEffect> globalEffects = unlockCard.Effects.Where(x => x.GetType() == typeof(GlobalEffect)).Cast<GlobalEffect>();
                foreach (GlobalEffect globalEffect in globalEffects)
                {
                    if (!SupportedRemoveEffectTypes.Contains(globalEffect.EffectType.GetType()) || !SupportedRemoveEffectConditions.Contains(globalEffect.EffectCondition.GetType()))
                    {
                        statusMessage += $"\n- {globalEffect.EffectType.GetType().Name} ({globalEffect.EffectCondition.GetType().Name})";
                        hasUnsupportedGlobalEffect = true;
                    }
                }

                if (hasUnsupportedGlobalEffect)
                    return false;

            }
            statusMessage = $"Successfully removed {unlock.Name}";
            return true;
        }

        internal static IEnumerable<ICard> GetPresentUnlockBlockers(Unlock unlock)
        {
            return GameInfo.AllCurrentCards.Intersect(unlock.BlockedBy);
        }

        internal static IEnumerable<ICard> GetMissingUnlockRequirements(Unlock unlock)
        {
            return unlock.Requires.Where(x => !GameInfo.AllCurrentCards.Contains(x));
        }
    }
}
