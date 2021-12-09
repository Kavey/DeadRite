using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleRight.Core;
using BattleRight.Core.Models;
using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.Core.Math;

using BattleRight.SDK;
using BattleRight.SDK.Enumeration;
using BattleRight.SDK.Events;
using BattleRight.SDK.UI;
using BattleRight.SDK.UI.Models;
using BattleRight.SDK.UI.Values;

using BattleRight.Sandbox;

using PipLibrary.Extensions;
using PipLibrary.Utils;

using TestPrediction2NS;

namespace KVCroak
{
    public class KVCroak : IAddon
    {
        private static Menu HeroMenu;
        private static Menu KeysMenu, ComboMenu, HealMenu, DrawingsMenu, MiscMenu;

        private static Character HeroPlayer => LocalPlayer.Instance;
        private static readonly string HeroName = "Croak";
        private static Vector2 MyPos => HeroPlayer.MapObject.Position;

        private static AbilitySlot? LastAbilityFired = null;
        
        //Range
        private const float M1Range = 2.5f;
        // private const float M2Range = 0.0f;
        // private const float SpaceRange = 0.0f;
        // private const float QRange = 0.0f;
        // private const float ERange = 0.0f;
        // private const float RRange = 0.0f;
        // private const float FRange = 0.0f;
        // private const float EX1Range = 0.0f;
        // private const float EX2Range = 0.0f;

        //Radius
        // private const float M1Radius = 0.6f;
        // private const float M2Radius = 0.0f;
        // private const float SpaceRadius = 0.0f;
        // private const float QRadius = 0.0f;
        // private const float ERadius = 0.0f;
        // private const float RRadius = 0.0f;
        // private const float FRadius = 0.0f;
        // private const float EX1Radius = 0.0f;
        // private const float EX2Radius = 0.0f;
        //

        //Speed
        // private const float M1Speed = 9f;
        // private const float M2Speed = 0.0f;
        // private const float SpaceSpeed = 0.0f;
        // private const float QSpeed = 0.0f;
        // private const float ESpeed = 0.0f;
        // private const float RSpeed = 0.0f;
        // private const float FSpeed = 0.0f;
        // private const float EX1Speed = 0.0f;
        // private const float EX2Speed = 0.0f;

        //AirTime
        // private const float M1AirTime = 0.0f;
        // private const float M2AirTime = 0.0f;
        // private const float SpaceAirTime = 0.0f;
        // private const float QAirTime = 0.0f;
        // private const float EAirTime = 0.0f;
        // private const float RAirTime = 0.0f;
        // private const float FAirTime = 0.0f;
        // private const float EX1AirTime = 0.0f;
        // private const float EX2AirTime = 0.0f;

        private static bool DidMatchInit = false;

        public void OnInit()
        {
            InitMenu();

            Game.OnMatchStart += OnMatchStart;
            Game.OnMatchEnd += OnMatchEnd;
        }

        private void OnMatchStart(EventArgs args)
        {
            if (HeroPlayer == null || !HeroPlayer.CharName.Equals(HeroName))
            {
                return;
            }

            Game.OnUpdate += OnUpdate;
            Game.OnDraw += OnDraw;

            DidMatchInit = true;
        }

        private void OnMatchEnd(EventArgs args)
        {
            if (DidMatchInit)
            {
                Game.OnUpdate -= OnUpdate;
                Game.OnDraw -= OnDraw;

                DidMatchInit = false;
            }
        }

        // KeysMenu.Add(new MenuKeybind("keys.combo", "Combo Key", UnityEngine.KeyCode.Mouse0));
        // KeysMenu.Add(new MenuCheckBox("keys.autoCombo", "Auto Combo Mode", false));
        // ComboMenu.Add(new MenuIntSlider("combo.useEX1.minEnergyBars", "    ^ Min energy bars", 2, 4, 1));
        // HealMenu.Add(new MenuSlider("heal.allySafeRange", "Target ally safe range", 10f, 10f, 0f));
        // HealMenu.AddLabel("Hold Healing key to use");


        private static void InitMenu()
        {
            HeroMenu = new Menu("templatemenu", "Kavey's Croak!");

            KeysMenu = new Menu("keysmenu", "Keys", true);
            KeysMenu.Add(new MenuKeybind("keys.combo", "Combo Key", UnityEngine.KeyCode.Mouse0));
            KeysMenu.Add(new MenuKeybind("keys.changeTargeting", "Change targeting mode", UnityEngine.KeyCode.Y, false, true));
            KeysMenu.Add(new MenuCheckBox("keys.autoCombo", "Auto Combo Mode", false));
            KeysMenu.Add(new MenuKeybind("keys.Space", "Space keybind to pause Auto Combo", UnityEngine.KeyCode.Space));
            HeroMenu.Add(KeysMenu);

            ComboMenu = new Menu("combomenu", "Combo", true);
            ComboMenu.Add(new MenuCheckBox("combo.invisible", "Attack invisible enemies", true));
            ComboMenu.Add(new MenuCheckBox("combo.useM1", "Use Left Mouse", false));
            ComboMenu.Add(new MenuCheckBox("combo.useM2", "Use Right Mouse", false));
            ComboMenu.Add(new MenuCheckBox("combo.useQ", "Use Q", false));
            ComboMenu.Add(new MenuCheckBox("combo.useE", "Use E", false));
            ComboMenu.Add(new MenuCheckBox("combo.useR", "Use R", false));
            ComboMenu.Add(new MenuCheckBox("combo.useEX1", "Use EX1", false));
            ComboMenu.Add(new MenuIntSlider("combo.useEX1.minEnergyBars", "    ^ Min energy bars", 2, 4, 1));
            ComboMenu.Add(new MenuCheckBox("combo.useEX2", "Use EX2", false));
            ComboMenu.Add(new MenuIntSlider("combo.useEX2.minEnergyBars", "    ^ Min energy bars", 2, 4, 1));
            ComboMenu.Add(new MenuCheckBox("combo.useF", "Use F", false));
            HeroMenu.Add(ComboMenu);

            HealMenu = new Menu("healmenu", "Healing", false);
            HeroMenu.Add(HealMenu);

            MiscMenu = new Menu("miscmenu", "Misc", false);
            HeroMenu.Add(MiscMenu);

            DrawingsMenu = new Menu("drawmenu", "Drawings", true);
            DrawingsMenu.Add(new MenuCheckBox("draw.rangeM1.safeRange", "Draw Left Mouse Range", true));
            HeroMenu.Add(DrawingsMenu);

            MainMenu.AddMenu(HeroMenu);
        }

        private static void OnUpdate(EventArgs args)
        {
            if (HeroPlayer.Living.IsDead)
            {
                return;
            }

            else if (KeysMenu.GetKeybind("keys.Space"))
            {
                LocalPlayer.EditAimPosition = false;
                LastAbilityFired = null;
            }

            else if (KeysMenu.GetBoolean("keys.autoCombo"))
            {
                ComboMode();
            }
            else if (KeysMenu.GetKeybind("keys.combo"))
            {
                ComboMode();
            }
            else
            {
                LocalPlayer.EditAimPosition = false;
                LastAbilityFired = null;
            }
        }

        private static void ComboMode()
        {
            //Valid Target
            var targetModeKey = KeysMenu.GetKeybind("keys.changeTargeting");
            var targetMode = targetModeKey ? TargetingMode.LowestHealth : TargetingMode.NearMouse;

            var enemiesToTargetBase = EntitiesManager.EnemyTeam.Where(x => x.IsValid && !x.Living.IsDead
            && !x.HasBuff("OtherSideBuff") && !x.HasBuff("AscensionBuff") && !x.HasBuff("AscensionTravelBuff") &&
            !x.HasBuff("Fleetfoot") && !x.HasBuff("TempestRushBuff") && !x.HasBuff("ValiantLeap") && !x.HasBuff("FrogLeap") &&
            !x.HasBuff("FrogLeapRecast") && !x.HasBuff("ElusiveStrikeCharged") && !x.HasBuff("ElusiveStrikeWall2") && 
            !x.HasBuff("BurrowAlternate") && !x.HasBuff("JetPack") && !x.HasBuff("ProwlBuff") && !x.HasBuff("Dive") &&
            !x.HasBuff("InfestingBuff") && !x.HasBuff("PortalBuff") && !x.HasBuff("CrushingBlow") && !x.HasBuff("TornadoBuff"));

            if (!ComboMenu.GetBoolean("combo.invisible"))
            {
                enemiesToTargetBase = enemiesToTargetBase.Where(x => !x.CharacterModel.IsModelInvisible);
            }

            var enemiesToTargetProjs = enemiesToTargetBase.Where(x =>
            !x.IsCountering && !x.HasShield() && !x.HasConsumeBuff && !x.HasParry() && !x.HasBuff("ElectricShield") && !x.HasBuff("BarbedHuskBuff"));

            var alliesToTargetBase = EntitiesManager.LocalTeam.Where(x => !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial);

            var targetM1 = TargetSelector.GetTarget(enemiesToTargetProjs, targetMode, M1Range);
            // var targetM2 = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, M2Range);
            // var targetSpace = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, SpaceRange);
            // var targetQ = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, QRange);
            // var targetE = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, ERange);
            // var targetR = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, RRange);
            // var targetEX1 = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, EX1Range);
            // var targetEX2 = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, EX2Range);
            // var targetF = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, FRange);
            //

            //GetNormalLinePrediction
            // var pred = TestPrediction.GetNormalLinePrediction(MyPos, target, Range, Speed, Radius, true);        true/false = colision
            // if (pred.CanHit)
            // {
            //     LocalPlayer.PressAbility(AbilitySlot.AbilityX, true);
            //     LocalPlayer.EditAimPosition = true;
            //     LocalPlayer.Aim(pred.CastPosition);
            //     return;
            // }
            //

            //GetPrediction
            // var pred = TestPrediction.GetPrediction(MyPos, target, Range, 0f, Radius, AirTime);
            // if (pred.CanHit)
            // {
            //     LocalPlayer.PressAbility(AbilitySlot.AbilityX, true);
            //     LocalPlayer.EditAimPosition = true;
            //     LocalPlayer.Aim(pred.CastPosition);
            //     return;
            // }
            //

            //Aim
            // if (LastAbilityFired == null && target != null)
            //     {
            //         LocalPlayer.PressAbility(AbilitySlot.AbilityX, true);
            //         LocalPlayer.EditAimPosition = true;
            //         LocalPlayer.Aim(X.MapObject.Position);
            //         return;
            //     }
            //

            //Saferange
            // var safeRange = ComboMenu.GetSlider("combo.useX.safeRange");
            // if (HeroPlayer.EnemiesAroundAlive(safeRange) > 0)
            //
            
            var isCastingOrChanneling = HeroPlayer.AbilitySystem.IsCasting || HeroPlayer.IsChanneling;

            if (isCastingOrChanneling && LastAbilityFired == null)
            {
                LastAbilityFired = CastingIndexToSlot(HeroPlayer.AbilitySystem.CastingAbilityIndex);
            }

            if (isCastingOrChanneling)
            {
                LocalPlayer.EditAimPosition = true;

                switch (LastAbilityFired)
                {
                    // case AbilitySlot.Ability7:
                    //     break;

                    // case AbilitySlot.EXAbility2:
                    //     break;
                    
                    // case AbilitySlot.EXAbility1:
                    //     break;

                    // case AbilitySlot.Ability6:
                    //     break;

                    // case AbilitySlot.Ability5:
                    //     break;

                    // case AbilitySlot.Ability4:
                    //     break;

                    // case AbilitySlot.Ability3:
                    //     break;

                    // case AbilitySlot.Ability2:
                    //     break;

                    case AbilitySlot.Ability1:
                        if (LastAbilityFired == null && targetM1 != null)
                        {
                            LocalPlayer.Aim(targetM1.MapObject.Position);
                        }
                        else
                        {
                            LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                        }
                        break;
                }
            }
            else
            {
                LocalPlayer.EditAimPosition = false;
                LastAbilityFired = null;
            }

            // if (ComboMenu.GetBoolean("combo.useF") && MiscUtils.CanCast(AbilitySlot.Ability7))
            // {
            //     if (LastAbilityFired == null && targetF != null)
            //     {
            //         var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetF, FRange, FSpeed, FRadius, true);
            //         if (pred.CanHit)
            //         {
            //             LocalPlayer.PressAbility(AbilitySlot.Ability7, true);
            //             LocalPlayer.EditAimPosition = true;
            //             LocalPlayer.Aim(pred.CastPosition);
            //             return;
            //         }
            //     }
            // }

            // if (ComboMenu.GetBoolean("combo.useEX2") && MiscUtils.CanCast(AbilitySlot.EXAbility2))
            // {
            //     var energyRequired = ComboMenu.GetIntSlider("combo.useEX2.minEnergyBars") * 25;
            //     if (energyRequired <= HeroPlayer.Energized.Energy)
            //     {
            //         if (LastAbilityFired == null && targetEX2 != null)
            //         {
            //             var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetEX2, EX2Range, EX2Speed, EX2Radius, true);
            //             if (pred.CanHit)
            //             {
            //                 LocalPlayer.PressAbility(AbilitySlot.EXAbility2, true);
            //                 LocalPlayer.EditAimPosition = true;
            //                 LocalPlayer.Aim(pred.CastPosition);
            //                 return;
            //             }
            //         }
            //     }
            // }

            // if (ComboMenu.GetBoolean("combo.useEX1") && MiscUtils.CanCast(AbilitySlot.EXAbility1))
            // {
            //     var energyRequired = ComboMenu.GetIntSlider("combo.useEX1.minEnergyBars") * 25;
            //     if (energyRequired <= HeroPlayer.Energized.Energy)
            //     {
            //         if (LastAbilityFired == null && targetEX1 != null)
            //         {
            //             var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetEX1, EX1Range, EX1Speed, EX1Radius, true);
            //             if (pred.CanHit)
            //             {
            //                 LocalPlayer.PressAbility(AbilitySlot.EXAbility1, true);
            //                 LocalPlayer.EditAimPosition = true;
            //                 LocalPlayer.Aim(pred.CastPosition);
            //                 return;
            //             }
            //         }
            //     }
            // }

            // if (ComboMenu.GetBoolean("combo.useR") && MiscUtils.CanCast(AbilitySlot.Ability6))
            // {
            //     if (LastAbilityFired == null && targetR != null)
            //     {
            //         var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetR, RRange, RSpeed, RRadius, true);
            //         if (pred.CanHit)
            //         {
            //             LocalPlayer.PressAbility(AbilitySlot.Ability6, true);
            //             LocalPlayer.EditAimPosition = true;
            //             LocalPlayer.Aim(pred.CastPosition);
            //             return;
            //         }
            //     }
            // }

            // if (ComboMenu.GetBoolean("combo.useE") && MiscUtils.CanCast(AbilitySlot.Ability5))
            // {
            //     if (LastAbilityFired == null && targetE != null)
            //     {
            //         var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetE, ERange, ESpeed, ERadius, true);
            //         if (pred.CanHit)
            //         {
            //             LocalPlayer.PressAbility(AbilitySlot.Ability5, true);
            //             LocalPlayer.EditAimPosition = true;
            //             LocalPlayer.Aim(pred.CastPosition);
            //             return;
            //         }
            //     }
            // }

            // if (ComboMenu.GetBoolean("combo.useQ") && MiscUtils.CanCast(AbilitySlot.Ability4))
            // {
            //     if (LastAbilityFired == null && targetQ != null)
            //     {
            //         var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetQ, QRange, QSpeed, QRadius, true);
            //         if (pred.CanHit)
            //         {
            //             LocalPlayer.PressAbility(AbilitySlot.Ability4, true);
            //             LocalPlayer.EditAimPosition = true;
            //             LocalPlayer.Aim(pred.CastPosition);
            //             return;
            //         }
            //     }
            // }

            // if (ComboMenu.GetBoolean("combo.useSpace") && MiscUtils.CanCast(AbilitySlot.Ability3))
            // {
            //     if (LastAbilityFired == null && targetSpace != null)
            //     {
            //         var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetSpace, SpaceRange, SpaceSpeed, SpaceRadius, true);
            //         if (pred.CanHit)
            //         {
            //             LocalPlayer.PressAbility(AbilitySlot.Ability3, true);
            //             LocalPlayer.EditAimPosition = true;
            //             LocalPlayer.Aim(pred.CastPosition);
            //             return;
            //         }
            //     }
            // }

            // if (ComboMenu.GetBoolean("combo.useM2") && MiscUtils.CanCast(AbilitySlot.Ability2))
            // {
            //     if (LastAbilityFired == null && targetM2 != null)
            //     {
            //         var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetM2, M2Range, M2Speed, M2Radius, true);
            //         if (pred.CanHit)
            //         {
            //             LocalPlayer.PressAbility(AbilitySlot.Ability2, true);
            //             LocalPlayer.EditAimPosition = true;
            //             LocalPlayer.Aim(pred.CastPosition);
            //             return;
            //         }
            //     }
            // }

            if (ComboMenu.GetBoolean("combo.useM1") && MiscUtils.CanCast(AbilitySlot.Ability1))
            {
                if (LastAbilityFired == null && targetM1 != null)
                {
                    LocalPlayer.PressAbility(AbilitySlot.Ability1, true);
                }
            }
        }

        private static AbilitySlot? CastingIndexToSlot(int index)
        {
            switch (index)
            {
                case 0:
                    return AbilitySlot.Ability1;
                case 2:
                    return AbilitySlot.Ability2;
                case 3:
                    return AbilitySlot.Ability3;
                case 4:
                case 9:
                    return AbilitySlot.Ability4;
                case 5:
                    return AbilitySlot.Ability5;
                case 7:
                    return AbilitySlot.Ability6;
                case 8:
                    return AbilitySlot.Ability7;
                case 1:
                    return AbilitySlot.EXAbility1;
                case 6:
                    return AbilitySlot.EXAbility2;
                case 10:
                    return AbilitySlot.Mount;
            }

            return null;
        }

        private static void OnDraw(EventArgs args)
        {
            if (!Game.IsInGame)
            {
                return;
            }

            if (HeroPlayer.Living.IsDead)
            {
                return;
            }

            Drawing.DrawString(new Vector2(1920f / 2f, 1080f / 2f - 5f).ScreenToWorld(),
                "Target: " + (KeysMenu.GetKeybind("keys.changeTargeting") ? "LowestHealth" : "NearMouse"), UnityEngine.Color.white);

            if (DrawingsMenu.GetBoolean("draw.rangeM1.safeRange"))
            {

                Drawing.DrawCircle(MyPos, M1Range, UnityEngine.Color.yellow);
            }
        }

        public void OnUnload()
        {

        }
    }
}
