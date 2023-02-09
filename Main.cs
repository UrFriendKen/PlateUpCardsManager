using Kitchen;
using Kitchen.Modules;
using KitchenCardsManager.Helpers;
using KitchenData;
using KitchenLib;
using KitchenLib.Event;
using KitchenLib.Utils;
using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

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
        private const string MOD_VERSION = "1.0.2";
        private const string MOD_AUTHOR = "IcedMilo";
        private const string MOD_GAMEVERSION = ">=1.1.1";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.1" current and all future
        // e.g. ">=1.1.1 <=1.2.3" for all from/until

        internal const string CARDS_MANAGER_MODULAR_UNLOCK_PACK_UNIQUENAMEID = "IcedMilo.PlateUp.CardsManager:CardsManagerModularUnlockPack";
        internal const string CARDS_MANAGER_COMPOSITE_UNLOCK_PACK_UNIQUENAMEID = "IcedMilo.PlateUp.CardsManager:CardsManagerCompositeUnlockPack";

        internal const string CARDS_MANAGER_MODE_PREFERENCE_ID = "Mode";
        internal const string CARDS_MANAGER_RESET_MODE_PREFERENCE_ID = "ResetMode";

        internal static bool BlacklistModeEnabled { get; private set; }
        internal static bool WhitelistModeEnabled { get; private set; }

        private static bool Logged = false;

        public Main() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void Initialise()
        {
            base.Initialise();
            // For log file output so the official plateup support staff can identify if/which a mod is being used
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
            AddGameDataObject<CardsManagerModularUnlockPack>();
            AddGameDataObject<CardsManagerCompositeUnlockPack>();
            RegisterPreferences();
            SetupKLPreferencesMenu();
            UpdateMode(PreferenceUtils.Get<KitchenLib.IntPreference>(MOD_GUID, CARDS_MANAGER_MODE_PREFERENCE_ID).Value);
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
            PreferenceUtils.Register<KitchenLib.IntPreference>(MOD_GUID, CARDS_MANAGER_MODE_PREFERENCE_ID, "Mode");
            PreferenceUtils.Get<KitchenLib.IntPreference>(MOD_GUID, CARDS_MANAGER_MODE_PREFERENCE_ID).Value = 0;
            PreferenceUtils.Register<KitchenLib.IntPreference>(MOD_GUID, CARDS_MANAGER_RESET_MODE_PREFERENCE_ID, "ResetMode");
            PreferenceUtils.Get<KitchenLib.IntPreference>(MOD_GUID, CARDS_MANAGER_RESET_MODE_PREFERENCE_ID).Value = 0;

            foreach (Unlock unlock in UnlockHelpers.GetAllUnlocksEnumerable())
            {
                PreferenceUtils.Register<KitchenLib.BoolPreference>(MOD_GUID, unlock.ID.ToString(), unlock.Name);
                PreferenceUtils.Get<KitchenLib.BoolPreference>(MOD_GUID, unlock.ID.ToString()).Value = unlock.IsUnlockable;
            }
            PreferenceUtils.Load();
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
            PreferenceUtils.Get<KitchenLib.IntPreference>(MOD_GUID, CARDS_MANAGER_MODE_PREFERENCE_ID).Value = 0;
            PreferenceUtils.Save();
            UpdateMode(PreferenceUtils.Get<KitchenLib.IntPreference>(MOD_GUID, CARDS_MANAGER_MODE_PREFERENCE_ID).Value);
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
                public static void Preference_OnChanged(string preferenceID, int f)
                {
                    PreferenceUtils.Get<KitchenLib.IntPreference>(Main.MOD_GUID, preferenceID).Value = f;
                    PreferenceUtils.Save();
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
                    PreferenceUtils.Get<KitchenLib.IntPreference>(Main.MOD_GUID, Main.CARDS_MANAGER_MODE_PREFERENCE_ID).Value,
                    new List<string>() { "Vanilla", "Blacklist", "Whitelist" });
                Add<int>(this.Mode).OnChanged += delegate (object _, int f)
                {
                    PreferencesHelper.Preference_OnChanged(Main.CARDS_MANAGER_MODE_PREFERENCE_ID, f);
                    Main.UpdateMode(f);
                };

                this.ResetMode = new Option<int>(
                    new List<int>() { 0, 1, 2 },
                    PreferenceUtils.Get<KitchenLib.IntPreference>(Main.MOD_GUID, Main.CARDS_MANAGER_RESET_MODE_PREFERENCE_ID).Value,
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
                CardScrollerElement cardScrollerElement = ModuleDirectory.Add<CardScrollerElement>(Container, new Vector2(0f, 0f));
                cardScrollerElement.SetCardList(GameInfo.AllCurrentCards);
                ModuleList.AddModule(cardScrollerElement, cardScrollerElement.transform.localPosition.ToFlat());
            }
        }
    }

    
}
