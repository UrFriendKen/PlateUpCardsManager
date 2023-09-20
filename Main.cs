using Kitchen;
using Kitchen.Modules;
using KitchenCardsManager.Customs;
using KitchenCardsManager.Helpers;
using KitchenCardsManager.Patches;
using KitchenData;
using KitchenLib;
using KitchenLib.Event;
using KitchenMods;
using PreferenceSystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenCardsManager
{
    internal class Main : BaseMod
    {
        // guid must be unique and is recommended to be in reverse domain name notation
        // mod name that is displayed to the player and listed in the mods menu
        // mod version must follow semver e.g. "1.2.3"
        internal const string MOD_GUID = "IcedMilo.PlateUp.CardsManager";
        private const string MOD_NAME = "Cards Manager";
        private const string MOD_VERSION = "1.4.5";
        private const string MOD_AUTHOR = "IcedMilo";
        private const string MOD_GAMEVERSION = ">=1.1.1";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.1" current and all future
        // e.g. ">=1.1.1 <=1.2.3" for all from/until

        internal const string CARDS_MANAGER_VARIABLE_PARAMETER_UNLOCK_CARD_UNIQUENAMEID = "IcedMilo.PlateUp.CardsManager:VariableParameterUnlockCard";
        internal const string CARDS_MANAGER_MODULAR_UNLOCK_PACK_UNIQUENAMEID = "IcedMilo.PlateUp.CardsManager:CardsManagerModularUnlockPack";
        internal const string CARDS_MANAGER_COMPOSITE_UNLOCK_PACK_UNIQUENAMEID = "IcedMilo.PlateUp.CardsManager:CardsManagerCompositeUnlockPack";
        internal const string CARDS_MANAGER_TURBO_MODULAR_UNLOCK_PACK_UNIQUENAMEID = "IcedMilo.PlateUp.CardsManager:CardsManagerTurboModularUnlockPack";
        internal const string CARDS_MANAGER_TURBO_COMPOSITE_UNLOCK_PACK_UNIQUENAMEID = "IcedMilo.PlateUp.CardsManager:CardsManagerTurboCompositeUnlockPack";

        internal const string CARDS_MANAGER_MODE_PREFERENCE_ID = "Mode";
        internal const string CARDS_MANAGER_RESET_MODE_PREFERENCE_ID = "ResetMode";
        internal const string CARDS_MANAGER_ADD_REMOVE_VALIDITY_CHECKING = "AddRemoveValidityCheck";
        internal const string CARDS_MANAGER_CARD_GROUPS_ENABLED = "CardGroupsEnabled";

        internal const string CARDS_MANAGER_CARD_DAY_INTERVAL = "CardDayInterval";

        internal const string CARDS_MANAGER_ADD_REMOVE_HOLD_DURATION = "AddRemoveHoldDuration";
        internal const string CARDS_MANAGER_ENABLE_DISABLE_HOLD_DURATION = "EnableDisableHoldDuration";

        internal const string DISH_CARDS_GROUPING_ID = "dishCardsGrouping";

        internal static bool BlacklistModeEnabled { get; private set; }
        internal static bool WhitelistModeEnabled { get; private set; }

        private const bool SHOULD_LOG = false;
        private static bool Logged = false;

        internal static PreferenceSystemManager PrefManager;

        internal static HashSet<int> StartingUnlocks { get; private set; } = new HashSet<int>();

        public Main() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            foreach (RestaurantSetting setting in Data.Get<RestaurantSetting>())
            {
                if (setting.StartingUnlock != null)
                {
                    StartingUnlocks.Add(setting.StartingUnlock.ID);
                }
            }
            RegisterPreferences();
            UpdateMode(PrefManager.Get<int>(CARDS_MANAGER_MODE_PREFERENCE_ID));
        }

        protected override void OnPostActivate(Mod mod)
        {
            // For log file output so the official plateup support staff can identify if/which a mod is being used
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
            AddGameDataObject<VariableParameterUnlockCard>();
            AddGameDataObject<CardsManagerModularUnlockPack>();
            AddGameDataObject<CardsManagerCompositeUnlockPack>();
            AddGameDataObject<CardsManagerTurboModularUnlockPack>();
            AddGameDataObject<CardsManagerTurboCompositeUnlockPack>();

            Events.BuildGameDataEvent += delegate (object _, BuildGameDataEventArgs args)
            {
                ModularUnlockPack themesModularUnlockPack = null;
                foreach (CompositeUnlockPack compositeUnlockPack in args.gamedata.Get<CompositeUnlockPack>())
                {
                    int franchisePackIndex = -1;
                    int themesPackIndex = -1;
                    for (int i = 0; i < compositeUnlockPack.Packs.Count; i++)
                    {
                        switch (compositeUnlockPack.Packs[i].ID)
                        {
                            case 1355831133: // Franchise Card Pack
                                franchisePackIndex = i;
                                break;
                            case 786043106: // Themes Card Pack
                                if (themesModularUnlockPack == null && (compositeUnlockPack.Packs[i] is ModularUnlockPack modularUnlockPack))
                                    themesModularUnlockPack = modularUnlockPack;
                                themesPackIndex = i;
                                break;
                            default:
                                break;
                        }

                        if (franchisePackIndex != -1 && themesPackIndex != -1)
                            break;
                    }

                    if (franchisePackIndex > -1 && themesPackIndex - franchisePackIndex > 1 && themesModularUnlockPack != null)
                    {
                        compositeUnlockPack.Packs.RemoveAt(themesPackIndex);
                        compositeUnlockPack.Packs.Insert(franchisePackIndex + 1, themesModularUnlockPack);
                    }
                }
            };
        }

        protected override void OnUpdate()
        {
            if (SHOULD_LOG && !Logged)
            {
                foreach (CompositeUnlockPack compositepack in GameData.Main.Get<CompositeUnlockPack>())
                {
                    LogInfo($"CompositeUnlockPack - {compositepack.name} ({compositepack.ID})");
                    try
                    {
                        foreach (UnlockPack unlockpack in compositepack.Packs)
                        {
                            LogInfo($"-- UnlockPack - {unlockpack.name} ({unlockpack.ID})");
                        }
                    }
                    catch (NullReferenceException)
                    {
                        LogInfo($"-- Does not have Packs field. Is {compositepack.name} ({compositepack.ID}) not a CompositeUnlockPack?");
                    }
                }
                foreach (ModularUnlockPack modularpack in GameData.Main.Get<ModularUnlockPack>())
                {
                    LogInfo($"ModularUnlockPack - {modularpack.name} ({modularpack.ID})");
                }

                Logged = true;
            }
        }

        private static void RegisterPreferences()
        {
            PrefManager = new PreferenceSystemManager(MOD_GUID, MOD_NAME);

            PrefManager
            .AddLabel("Cards Manager")
            .AddInfo("See Cards Manager Steam workshop page for details on mode behavior.")
            .AddLabel("Mode")
            .AddOption<int>(
                CARDS_MANAGER_MODE_PREFERENCE_ID,
                1,
                new int[] { 0, 1, 2 },
                new string[] { "Vanilla", "Blacklist", "Whitelist" },
                UpdateMode)
            .AddLabel("Automatically Reset Mode To Vanilla")
            .AddOption<int>(
                CARDS_MANAGER_RESET_MODE_PREFERENCE_ID,
                0,
                new int[] { 0, 1, 2 },
                new string[] { "Never", "When Starting New Run", "When Entering HQ" })
            .AddLabel("Enabled Card Groups")
            .AddOption<string>(
                CARDS_MANAGER_CARD_GROUPS_ENABLED,
                "ALL",
                new string[] { "ALL", "VANILLA", "MODDED" },
                new string[] { "All Cards", "Vanilla Cards Only", "Modded Cards Only" })
            .AddLabel("Card Day Interval")
            .AddOption<int>(
                CARDS_MANAGER_CARD_DAY_INTERVAL,
                -1,
                new int[] { -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                new string[] { "Default", "No Card Day", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" })
            .AddLabel("Impossible Card Combinations (Use at your own risk!)")
            .AddOption<bool>(
                CARDS_MANAGER_ADD_REMOVE_VALIDITY_CHECKING,
                true,
                new bool[] { true, false },
                new string[] { "Prevent", "Allow" })
            .AddSpacer()
            .AddLabel("Edit Cards")
            .AddSelfRegisteredSubmenu<CardsManagerScrollerMenu<MainMenuAction>, CardsManagerScrollerMenu<PauseMenuAction>>("Open Cards Menu")
            .AddButtonWithConfirm("Reset Card Settings", "Confirm reset all card settings to default?", delegate (GenericChoiceDecision decision)
            {
                foreach (Unlock unlock in UnlockHelpers.GetAllUnlocksEnumerable())
                {
                    PrefManager.Set<bool>(unlock.ID.ToString(), UnlockHelpers.GetDefaultEnabledState(unlock));
                }
                PrefManager.Set<string>(CARDS_MANAGER_CARD_GROUPS_ENABLED, "ALL");
                PrefManager.Set<int>(CARDS_MANAGER_CARD_DAY_INTERVAL, -1);
                PrefManager.Set<bool>(CARDS_MANAGER_ADD_REMOVE_VALIDITY_CHECKING, true);

            })
            .AddSubmenu("Controls Behavior", "controlsBehavior")
                .AddLabel("Enable/Disable Hold Time")
                .AddOption<float>(
                    CARDS_MANAGER_ENABLE_DISABLE_HOLD_DURATION,
                    1.5f,
                    new float[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f, 4.5f, 5f },
                    new string[] { "0.5 seconds", "1 second", "1.5 seconds", "2 seconds", "2.5 seconds", "3 seconds", "3.5 seconds", "4 seconds", "4.5 seconds", "5 seconds" })
                .AddLabel("Add/Remove Hold Time")
                .AddOption<float>(
                    CARDS_MANAGER_ADD_REMOVE_HOLD_DURATION,
                    3f,
                    new float[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f, 4.5f, 5f },
                    new string[] { "0.5 seconds", "1 second", "1.5 seconds", "2 seconds", "2.5 seconds", "3 seconds", "3.5 seconds", "4 seconds", "4.5 seconds", "5 seconds" })
                .AddSpacer()
                .AddSpacer()
            .SubmenuDone()
            .AddSubmenu("Display", "display")
                .AddLabel("Dish Cards Grouping")
                .AddOption<int>(
                    DISH_CARDS_GROUPING_ID,
                    0,
                    new int[] { 0, 1 },
                    new string[] { "Default", "By Type" })
                .AddSpacer()
                .AddSpacer()
            .SubmenuDone()
            .AddSpacer()
            .AddSpacer();

            AddProperties();

            Events.PreferenceMenu_MainMenu_CreateSubmenusEvent += (s, args) =>
            {
                CardsManagerScrollerMenu<MainMenuAction> cardsMenu = new CardsManagerScrollerMenu<MainMenuAction>(args.Container, args.Module_list);
                args.Menus.Add(typeof(CardsManagerScrollerMenu<MainMenuAction>), cardsMenu);
            };

            Events.PreferenceMenu_PauseMenu_CreateSubmenusEvent += (s, args) =>
            {
                CardsManagerScrollerMenu<PauseMenuAction> cardsMenu = new CardsManagerScrollerMenu<PauseMenuAction>(args.Container, args.Module_list);
                args.Menus.Add(typeof(CardsManagerScrollerMenu<PauseMenuAction>), cardsMenu);
            };

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
        }

        private static void AddProperties()
        {
            foreach (Unlock unlock in UnlockHelpers.GetAllUnlocksEnumerable())
            {
                PrefManager.AddProperty<bool>(unlock.ID.ToString(), unlock.IsUnlockable || StartingUnlocks.Contains(unlock.ID));
            }
        }

        internal static void ResetModeToVanilla()
        {
            LogInfo("Resetting mode to Vanilla");
            PrefManager.Set<int>(CARDS_MANAGER_MODE_PREFERENCE_ID, 0);
            UpdateMode(PrefManager.Get<int>(CARDS_MANAGER_MODE_PREFERENCE_ID));
        }

        private static void UpdateMode(int modeValue)
        {
            switch (modeValue)
            {
                case 0:
                    BlacklistModeEnabled = false;
                    WhitelistModeEnabled = false;
                    break;
                case 1:
                    BlacklistModeEnabled = true;
                    WhitelistModeEnabled = false;
                    break;
                case 2:
                    BlacklistModeEnabled = true;
                    WhitelistModeEnabled = true;
                    break;
                default:
                    BlacklistModeEnabled = false;
                    WhitelistModeEnabled = false;
                    break;
            }
            string mode = WhitelistModeEnabled ? "Whitelist" : (BlacklistModeEnabled ? "Blacklist" : "Vanilla");
            LogInfo($"Mode Updated: {mode}");
        }

        #region Logging
        // You can remove this, I just prefer a more standardized logging
        internal static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        internal static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        internal static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        internal static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        internal static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        internal static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion

        private class CardsManagerScrollerMenu<T> : KLMenu<T>
        {
            public override bool RequiresBackingPanel { get; protected set; } = false;

            public CardsManagerScrollerMenu(Transform container, ModuleList module_list) : base(container, module_list)
            {
            }

            public override void Setup(int player_id)
            {
                CardScrollerElement_Patch.MenuOpenedFromModPreferences = true;
                CardScrollerElement cardScrollerElement = ModuleDirectory.Add<CardScrollerElement>(Container, new Vector2(0f, 0f));
                cardScrollerElement.SetCardList(GameInfo.AllCurrentCards);
                ModuleList.AddModule(cardScrollerElement, cardScrollerElement.transform.localPosition.ToFlat());
            }
        }
    }
}