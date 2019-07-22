using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleRight.SDK;
using BattleRight.SDK.Enumeration;
using BattleRight.SDK.Events;
using BattleRight.SDK.UI;
using BattleRight.SDK.UI.Models;
using BattleRight.SDK.UI.Values;

using PipAshka.Modes;

using AH = PipAshka.AbilityHandler;

namespace PipAshka.Utils
{
    internal static class AshkaMenu
    {
        internal static Menu RootMenu;
        internal static Menu KeysMenu, ComboMenu, KSMenu, DrawingsMenu;

        internal static void LoadMenu()
        {
            RootMenu = new Menu("pipvareshmenu", "Kavey's Varesh");

            KeysMenu = new Menu("keysmenu", "Keys", true);
            KeysMenu.Add(new MenuKeybind("combo", "Combo", UnityEngine.KeyCode.LeftControl, false, false));
            RootMenu.Add(KeysMenu);

            ComboMenu = new Menu("combomenu", "Combo", true);
            ComboMenu.Add(new MenuCheckBox("invisible", "Attack invisible targets", true));
            ComboMenu.Add(new MenuCheckBox("interrupt", "Interrupt M1 or M2 when target gets out of range", false));
            ComboMenu.Add(new MenuCheckBox("useM1", "Use M1 (Hand of Corruption)", true));
            ComboMenu.Add(new MenuCheckBox("useM1.priority", "- - - - - - - - - - - -", false));
            ComboMenu.Add(new MenuCheckBox("useM2", "Use M2 (Hand of Jugement)", false));
            ComboMenu.Add(new MenuSlider("useM2.safeRange", "    ^ Safe Range", 6.5f, AH.M2.Range - 1f, 0f));
            ComboMenu.Add(new MenuCheckBox("useSpace", "Use Space (Inhibitor's Guard)", true));
            ComboMenu.Add(new MenuIntSlider("useSpace.enemies.count", "Use Space (Inhibitor's Guard) when X ennemy is in range", 1, 3, 1));
            ComboMenu.Add(new MenuCheckBox("useQ", "- - - - - - - - - - - -", false));
            ComboMenu.Add(new MenuCheckBox("useQ.chainCC", "- - - - - - - - - - - -", false));
            ComboMenu.Add(new MenuCheckBox("useE.near", "Use E (Shatter)", false));
            ComboMenu.Add(new MenuSlider("useE.near.safeRange", "    ^ Safe Range", 0f, AH.E.Range - 1f, 0f));
            //ComboMenu.Add(new MenuCheckBox("useR.harass", "Use R (Firewall) to upgrade M1 projectiles (if conflagration is enabled)", false));
            //ComboMenu.Add(new MenuIntSlider("useR.harass.energy", "    ^ Min energy bars", 3, 4, 1));
            ComboMenu.Add(new MenuCheckBox("useEX1", "Use EX1 (Hand of Punishement)", true));
            ComboMenu.Add(new MenuIntSlider("useEX1.energy", "    ^ Min energy bars", 2, 4, 1));
            ComboMenu.Add(new MenuSlider("useEX1.minHP", "- - - - - - - - - - - -", 0f, 100f, 0f));
            ComboMenu.Add(new MenuCheckBox("useEX2.enemies", "- - - - - - - - - - - -", false));
            ComboMenu.Add(new MenuIntSlider("useEX2.enemies.count", "- - - - - - - - - - - -", 1, 3, 1));
            ComboMenu.Add(new MenuCheckBox("useEX2.panic", "- - - - - - - - - - - -", false));
            ComboMenu.Add(new MenuSlider("useEX2.panic.minHP", "- - - - - - - - - - - -", 0f, 100f, 0f));
            ComboMenu.Add(new MenuIntSlider("useEX2.energy", "- - - - - - - - - - - -", 1, 4, 1));
            ComboMenu.Add(new MenuCheckBox("useF", "Use F (Powers Combined)", false));
            RootMenu.Add(ComboMenu);

            KSMenu = new Menu("ksmenu", "Killsteal", true);
            //KSMenu.Add(new MenuCheckBox("useF", "Use F (Infernal Scorch) to killsteal", true));
            RootMenu.Add(KSMenu);

            DrawingsMenu = new Menu("drawingsmenu", "Drawings", true);
            DrawingsMenu.Add(new MenuCheckBox("disableAll", "Disable all drawings", false));
            DrawingsMenu.AddSeparator(10f);
            DrawingsMenu.Add(new MenuCheckBox("rangeM1", "Draw M1 (Hand of Corruption) Range", true));
            DrawingsMenu.Add(new MenuCheckBox("rangeM2", "Draw M2 (Hand of Jugement) Range", false));
            DrawingsMenu.Add(new MenuCheckBox("rangeM2.safeRange", "Draw M2 (Hand of Jugement) Safe Range", false));
            DrawingsMenu.Add(new MenuCheckBox("rangeF", "Draw F (Powers Combined) Range", false));
            RootMenu.Add(DrawingsMenu);

            MainMenu.AddMenu(RootMenu);
        }
    }
}
