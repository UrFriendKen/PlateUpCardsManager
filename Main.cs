using Kitchen;
using Kitchen.Modules;
using KitchenCardsManager.Customs;
using KitchenCardsManager.Helpers;
using KitchenCardsManager.Patches;
using KitchenData;
using KitchenLib;
using KitchenLib.Event;
using KitchenLib.Preferences;
using KitchenMods;
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
        private const string MOD_VERSION = "1.3.0";
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

        internal static bool BlacklistModeEnabled { get; private set; }
        internal static bool WhitelistModeEnabled { get; private set; }

        private static bool Logged = false;

        internal static PreferenceManager KLPrefManager;

        public Main() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            RegisterPreferences();
            SetupKLPreferencesMenu();

            UpdateMode(KLPrefManager.GetPreference<PreferenceInt>(CARDS_MANAGER_MODE_PREFERENCE_ID).Get());
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
        }

        protected override void OnUpdate()
        {
            if (!Logged)
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
            KLPrefManager = new PreferenceManager(MOD_GUID);

            KLPrefManager.RegisterPreference<PreferenceInt>(new PreferenceInt(CARDS_MANAGER_MODE_PREFERENCE_ID, 1));
            KLPrefManager.RegisterPreference<PreferenceInt>(new PreferenceInt(CARDS_MANAGER_RESET_MODE_PREFERENCE_ID, 0));

            foreach (Unlock unlock in UnlockHelpers.GetAllUnlocksEnumerable())
            {
                KLPrefManager.RegisterPreference<PreferenceBool>(new PreferenceBool(unlock.ID.ToString(), unlock.IsUnlockable || (unlock.CardType == CardType.Setting && !UnlockHelpers.IsModded(unlock))));
            }
            KLPrefManager.Load();
        }

        private static void SetupKLPreferencesMenu()
        {
            Events.PreferenceMenu_PauseMenu_CreateSubmenusEvent += (s, args) => {
                args.Menus.Add(typeof(CardsManagerMenu<PauseMenuAction>), new CardsManagerMenu<PauseMenuAction>(args.Container, args.Module_list));
                args.Menus.Add(typeof(CardsManagerScrollerMenu<PauseMenuAction>), new CardsManagerScrollerMenu<PauseMenuAction>(args.Container, args.Module_list));
            };
            ModsPreferencesMenu<PauseMenuAction>.RegisterMenu(MOD_NAME, typeof(CardsManagerMenu<PauseMenuAction>), typeof(PauseMenuAction));
        }

        internal static void ResetModeToVanilla()
        {
            LogInfo("Resetting mode to Vanilla");
            KLPrefManager.GetPreference<PreferenceInt>(CARDS_MANAGER_MODE_PREFERENCE_ID).Set(0);
            KLPrefManager.Save();
            UpdateMode(KLPrefManager.GetPreference<PreferenceInt>(CARDS_MANAGER_MODE_PREFERENCE_ID).Get());
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

        private class CardsManagerMenu<T> : KLMenu<T>
        {
            private static class PreferencesHelper
            {
                public static void Preference_OnChanged(string preferenceID, int i)
                {
                    Main.KLPrefManager.GetPreference<PreferenceInt>(preferenceID).Set(i);
                    Main.KLPrefManager.Save();
                }
            }

            private Option<int> Mode;
            private Option<int> ResetMode;

            public CardsManagerMenu(Transform container, ModuleList module_list) : base(container, module_list)
            {
            }

            public override void Setup(int player_id)
            {
                AddLabel("Cards Manager");

                AddInfo("Vanilla: Cards settings have no effect.");
                AddInfo("Blacklist: Disabled cards will not appear.");
                AddInfo("Whitelist: All enabled cards have a chance of appearing if prerequisites are met.");

                AddLabel("Mode");
                this.Mode = new Option<int>(
                    new List<int>() { 0, 1, 2 },
                    Main.KLPrefManager.GetPreference<PreferenceInt>(Main.CARDS_MANAGER_MODE_PREFERENCE_ID).Get(),
                    new List<string>() { "Vanilla", "Blacklist", "Whitelist" });
                Add<int>(this.Mode).OnChanged += delegate (object _, int f)
                {
                    PreferencesHelper.Preference_OnChanged(Main.CARDS_MANAGER_MODE_PREFERENCE_ID, f);
                    Main.UpdateMode(f);
                };

                this.ResetMode = new Option<int>(
                    new List<int>() { 0, 1, 2 },
                    Main.KLPrefManager.GetPreference<PreferenceInt>(Main.CARDS_MANAGER_RESET_MODE_PREFERENCE_ID).Get(),
                    new List<string>() { "Never", "When Starting New Run", "When Entering HQ" });
                AddLabel("Automatically Reset Mode To Vanilla");
                Add<int>(this.ResetMode).OnChanged += delegate (object _, int f)
                {
                    PreferencesHelper.Preference_OnChanged(Main.CARDS_MANAGER_RESET_MODE_PREFERENCE_ID, f);
                };

                New<SpacerElement>();

                AddLabel("Edit Cards");
                AddSubmenuButton("Open Cards Menu", typeof(CardsManagerScrollerMenu<T>));

                New<SpacerElement>();
                New<SpacerElement>();

                AddButton(base.Localisation["MENU_BACK_SETTINGS"], delegate
                {
                    RequestPreviousMenu();
                });
            }
        }

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
