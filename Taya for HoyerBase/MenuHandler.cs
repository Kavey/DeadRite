using System;
using System.Collections.Generic;
using BattleRight.Core;
using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.SDK.UI;
using BattleRight.SDK.UI.Models;
using BattleRight.SDK.UI.Values;
using UnityEngine;

namespace Hoyer.Champions.Taya
{
    public static class MenuHandler
    {
        public static Menu HoyerMainMenu;
        public static Menu TayaMenu;
        public static Menu SkillMenu;

        public static bool AvoidStealthed;
        public static bool UseCursor;
        public static bool AimUserInput;
        public static bool DrawDebugText;
        public static bool InterruptSpells;

        private static MenuCheckBox _avoidStealthed;
        private static MenuCheckBox _useCursor;
        private static MenuCheckBox _aimUserInput;
        private static MenuCheckBox _interruptSpells;
        private static MenuCheckBox _enabledBox;
        private static MenuCheckBox _comboToggle;
        private static MenuKeybind _comboKey;

        private static readonly Dictionary<string, bool> SkillCheckBoxes = new Dictionary<string, bool>();

        public static void Init()
        {
            HoyerMainMenu = MainMenu.GetMenu("Hoyer.MainMenu");
            TayaMenu = HoyerMainMenu.Add(new Menu("HoyerTaya", "Taya", true));

            TayaMenu.Add(new MenuLabel("Taya"));
            _enabledBox = new MenuCheckBox("taya_enabled", "Enabled");
            _enabledBox.OnValueChange += delegate(ChangedValueArgs<bool> args) { Main.Enabled = args.NewValue; };
            TayaMenu.Add(_enabledBox);

            TayaMenu.AddSeparator();

            _comboKey = new MenuKeybind("taya_combokey", "Combo key", KeyCode.V);
            _comboKey.OnValueChange += delegate(ChangedValueArgs<bool> args) { Main.SetMode(args.NewValue); };
            TayaMenu.Add(_comboKey);

            _comboToggle = new MenuCheckBox("taya_combotoggle", "Should Combo key be a toggle", false);
            _comboToggle.OnValueChange += delegate(ChangedValueArgs<bool> args) { _comboKey.IsToggle = args.NewValue; };
            TayaMenu.Add(_comboToggle);

            _aimUserInput = new MenuCheckBox("taya_aimuserinput", "Apply aim logic when combo isn't active");
            _aimUserInput.OnValueChange += delegate(ChangedValueArgs<bool> args) { AimUserInput = args.NewValue; };
            TayaMenu.Add(_aimUserInput);

            _useCursor = new MenuCheckBox("taya_usecursor", "Use cursor pos for target selection");
            _useCursor.OnValueChange += delegate(ChangedValueArgs<bool> args) { UseCursor = args.NewValue; };
            TayaMenu.Add(_useCursor);

            _avoidStealthed = new MenuCheckBox("taya_ignorestealthed", "Ignore stealthed enemies", false);
            _avoidStealthed.OnValueChange += delegate(ChangedValueArgs<bool> args) { AvoidStealthed = args.NewValue; };
            TayaMenu.Add(_avoidStealthed);

            _interruptSpells = new MenuCheckBox("Varesh_interruptspells", "Interrupt spellcasts if aim logic is active and no valid targets");
            _interruptSpells.OnValueChange += delegate (ChangedValueArgs<bool> args)
            {
                InterruptSpells = args.NewValue;
            };
            TayaMenu.Add(_interruptSpells);

            InitSkillMenu();

            FirstRun();
            Base.Main.DelayAction(delegate
            {
                var drawText = HoyerMainMenu.Get<Menu>("Hoyer.Debug").Add(new MenuCheckBox("Taya_drawdebug", "Draw Taya debug text"));
                drawText.OnValueChange += delegate(ChangedValueArgs<bool> args) { DrawDebugText = args.NewValue; };
                DrawDebugText = drawText.CurrentValue;
            }, 0.8f);
        }

        public static void Unload()
        {
            SkillCheckBoxes.Clear();
        }

        private static void InitSkillMenu()
        {
            SkillMenu = TayaMenu.Add(new Menu("HoyerTaya.Skills", "Skills", true));
            AddSkillCheckbox("combo_a1", "Use M1 in combo");
            AddSkillCheckbox("combo_a2", "Use M2 in combo");
            AddSkillCheckbox("close_a3", "Use Space to avoid melees");
            AddSkillCheckbox("combo_a4", "Use Q in combo");
            AddSkillCheckbox("combo_a5", "Use E in combo");
            AddSkillCheckbox("save_a6", "Save energy for R in combo");
            AddSkillCheckbox("combo_ex1", "Use EX1 in combo");
            AddSkillCheckbox("combo_ex2", "Use EX2 in combo");
        }

        public static bool SkillBool(string name)
        {
            return SkillCheckBoxes[name];
        }

        public static bool UseSkill(AbilitySlot slot)
        {
            switch (slot)
            {
                case AbilitySlot.Ability1:
                    return SkillCheckBoxes["combo_a1"];
                case AbilitySlot.Ability2:
                    return SkillCheckBoxes["combo_a2"];
                case AbilitySlot.Ability4:
                    return SkillCheckBoxes["combo_a4"];
                case AbilitySlot.Ability5:
                    return SkillCheckBoxes["combo_a5"];
                case AbilitySlot.EXAbility1:
                    return SkillCheckBoxes["combo_ex1"];
                case AbilitySlot.EXAbility2:
                    return SkillCheckBoxes["combo_ex2"];
            }

            return false;
        }

        private static void AddSkillCheckbox(string name, string displayname, bool defaultVal = true)
        {
            var skill = SkillMenu.Add(new MenuCheckBox(name, displayname, defaultVal));
            SkillCheckBoxes.Add(name, skill.CurrentValue);
            skill.OnValueChange += delegate(ChangedValueArgs<bool> args) { SkillCheckBoxes[skill.Name] = args.NewValue; };
        }

        private static void FirstRun()
        {
            Main.Enabled = _enabledBox.CurrentValue;
            Main.SetMode(false);
            _comboKey.IsToggle = _comboToggle.CurrentValue;
            UseCursor = _useCursor;
            AimUserInput = _aimUserInput;
            InterruptSpells = _interruptSpells;
        }

        public static void Update()
        {
            if (TayaMenu == null)
            {
                Console.WriteLine("[HoyerTaya/MenuHandler] Can't find menu, if this message is getting spammed, try F5 and please report this to Hoyer :(");
                return;
            }

            if (!Game.IsInGame)
            {
                if (TayaMenu.Hidden) TayaMenu.Hidden = false;
                return;
            }

            if (LocalPlayer.Instance.ChampionEnum != Champion.Taya && !TayaMenu.Hidden)
            {
                TayaMenu.Hidden = true;
                return;
            }

            if (LocalPlayer.Instance.ChampionEnum == Champion.Taya && TayaMenu.Hidden) TayaMenu.Hidden = false;
        }
    }
}