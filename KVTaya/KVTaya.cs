using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleRight.Core;
using BattleRight.Core.Models;
using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.Core.GameObjects.Models;
using BattleRight.SDK;
using BattleRight.SDK.Enumeration;
using BattleRight.SDK.Events;
using BattleRight.SDK.UI;
using BattleRight.SDK.UI.Models;
using BattleRight.SDK.UI.Values;

using BattleRight.Helper;

using BattleRight.Sandbox;

using PipLibrary.Extensions;
using PipLibrary.Utils;

using TestPrediction2NS;
using UnityEngine;
using Vector2 = BattleRight.Core.Math.Vector2;

namespace KVTaya
{
    public class KVTaya : IAddon
    {
        private static Menu HeroMenu;
        private static Menu KeysMenu, ComboMenu, DrawingsMenu, MiscMenu;

        private static Character HeroPlayer => LocalPlayer.Instance;
        private static readonly string HeroName = "Taya";
        private static Vector2 MyPos => HeroPlayer.MapObject.Position;

        private static AbilitySlot? LastAbilityFired = null;

        //Range
        private const float M1Range = 7.9f;
        private const float M2MinRange = 6.75f;
        private const float M2MaxRange = 8.5f;
        // private const float SpaceRange = 0.0f;
        private const float QRange = 2.5f;
        private const float ERange = 5.5f;
        // private const float RRange = 0.0f;
        // private const float FRange = 0.0f;
        private const float EX1Range = 11.5f;
        private const float EX2Range = 4f;
        

        //Radius
        private const float M1Radius = 0.4f;
        private const float M2Radius = 0.5f;
        // private const float SpaceRadius = 0.0f;
        // private const float QRadius = 0.0f;
        // private const float ERadius = 0.0f;
        // private const float RRadius = 0.0f;
        // private const float FRadius = 0.0f;
        private const float EX1Radius = 0.25f;
        // private const float EX2Radius = 0.0f;
        //

        //Speed
        private const float M1Speed = 18f;
        private const float M2Speed = 18f;
        // private const float SpaceSpeed = 0.0f;
        // private const float QSpeed = 0.0f;
        // private const float ESpeed = 0.0f;
        // private const float RSpeed = 0.0f;
        // private const float FSpeed = 0.0f;
        private const float EX1Speed = 25f;
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

        //Misc
        // private const float M1Damage = 14f;
        private const float EX1Damage = 16f;
        private const float EX2Damage = 14f;

        private static bool IsInUltimate
        {
            get
            {
                var CompanionCall =  HeroPlayer.HasBuff("CompanionCallMountBuff");
                if (CompanionCall)
                {
                    return true;
                }

                return false;
            }
        }

        private static bool IsInTheAir
        {
            get
            {
                var TornadoBuff =  HeroPlayer.HasBuff("TornadoBuff");
                if (TornadoBuff)
                {
                    return true;
                }

                return false;
            }
        }

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

        private static void InitMenu()
        {
            HeroMenu = new Menu("templatemenu", "Kavey's Taya");

            KeysMenu = new Menu("keysmenu", "Keys", true);
            KeysMenu.Add(new MenuKeybind("keys.combo", "Combo Key", UnityEngine.KeyCode.Mouse0));
            KeysMenu.Add(new MenuKeybind("keys.changeTargeting", "Change targeting mode", UnityEngine.KeyCode.Y, false, true));
            KeysMenu.Add(new MenuCheckBox("keys.autoCombo", "Auto Combo Mode", false));
            KeysMenu.Add(new MenuKeybind("keys.M1", "Left Mouse keybind to pause Auto Combo", UnityEngine.KeyCode.Mouse2));
            KeysMenu.Add(new MenuKeybind("keys.M2", "Right Mouse keybind to pause Auto Combo", UnityEngine.KeyCode.Mouse1));
            KeysMenu.Add(new MenuKeybind("keys.R", "R keybind to pause Auto Combo", UnityEngine.KeyCode.R));
            KeysMenu.Add(new MenuKeybind("keys.EX1", "EX1 keybind to pause Auto Combo", UnityEngine.KeyCode.T));
            KeysMenu.Add(new MenuKeybind("keys.EX2", "EX2 keybind to pause Auto Combo", UnityEngine.KeyCode.G));
            HeroMenu.Add(KeysMenu);

            ComboMenu = new Menu("combomenu", "Combo", true);
            // ComboMenu.Add(new MenuSlider("combo.autoSafeRange", "Auto Combo safe range", 2f, 10f, 0f));
            ComboMenu.Add(new MenuCheckBox("combo.invisible", "Attack invisible enemies", true));
            ComboMenu.Add(new MenuCheckBox("combo.useM1", "Use Left Mouse", true));
            // ComboMenu.Add(new MenuSlider("combo.useM1.safeRange", "    ^ Safe range", 2.5f, 5f, 0f));
            ComboMenu.Add(new MenuCheckBox("combo.useM2", "Use Right Mouse", true));
            ComboMenu.Add(new MenuCheckBox("combo.useSpace", "Use Space", true));
            ComboMenu.Add(new MenuCheckBox("combo.useQ", "Use Q", true));
            ComboMenu.Add(new MenuCheckBox("combo.hasteQ", "^ Only with Haste", true));
            ComboMenu.Add(new MenuCheckBox("combo.useE", "Use E", true));
            ComboMenu.Add(new MenuCheckBox("combo.useEX1", "Use EX1", true));
            ComboMenu.Add(new MenuIntSlider("combo.useEX1.minEnergyBars", "    ^ Min energy bars", 2, 4, 1));
            ComboMenu.Add(new MenuCheckBox("combo.useEX2haste", "Use EX2 with Haste", true));
            ComboMenu.Add(new MenuIntSlider("combo.useEX2haste.minEnergyBars", "    ^ Min energy bars", 2, 4, 1));
            ComboMenu.Add(new MenuCheckBox("combo.useF", "Use F", true));
            HeroMenu.Add(ComboMenu);

            MiscMenu = new Menu("miscmenu", "Misc", true);
            // MiscMenu.Add(new MenuCheckBox("misc.targetOrb", "Attack the Orb", true));
            MiscMenu.Add(new MenuCheckBox("misc.useEself", "Use E self", true));
            MiscMenu.Add(new MenuCheckBox("misc.useEX1exec", "Use EX1 to execute", true));
            MiscMenu.Add(new MenuCheckBox("misc.useEX2exec", "Use EX2 to execute", true));
            HeroMenu.Add(MiscMenu);

            DrawingsMenu = new Menu("drawmenu", "Drawings", true);
            DrawingsMenu.Add(new MenuCheckBox("draw.rangeM1.safeRange", "Draw Left Mouse Range", true));
            DrawingsMenu.Add(new MenuCheckBox("draw.Target", "Draw Target", true));
            HeroMenu.Add(DrawingsMenu);

            MainMenu.AddMenu(HeroMenu);
        }

        private static void OnUpdate(EventArgs args)
        {
            if (HeroPlayer.Living.IsDead)
            {
                return;
            }

            if (KeysMenu.GetBoolean("keys.autoCombo") && ((KeysMenu.GetKeybind("keys.M1")) || (KeysMenu.GetKeybind("keys.M2")) || (KeysMenu.GetKeybind("keys.R")) ||
            (KeysMenu.GetKeybind("keys.EX1")) || (KeysMenu.GetKeybind("keys.EX2"))))
            {
                LocalPlayer.EditAimPosition = false;
                LastAbilityFired = null;
                return;
            }
        
            if (KeysMenu.GetBoolean("keys.autoCombo"))
            {
                if (!IsInTheAir)
                {
                    // if (MiscMenu.GetBoolean("misc.targetOrb"))
                    // {
                    //      OrbMode();
                    // }
                    ComboMode();
                }
                else
                {
                    LocalPlayer.EditAimPosition = false;
                    LastAbilityFired = null;
                }
            }
            else if (KeysMenu.GetKeybind("keys.combo") && !IsInTheAir)
            {
                // if (MiscMenu.GetBoolean("misc.targetOrb"))
                // {
                //     OrbMode();
                // }
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

            var enemiesToTargetXStrike = enemiesToTargetProjs.Where(x => (x.Distance(HeroPlayer) >= M2MinRange) && (x.Distance(HeroPlayer) <= M2MaxRange));
            var enemiesToTargetQ = enemiesToTargetProjs.Where(x => x.Distance(HeroPlayer) <= QRange);
            var enemiesToTargetEX2 = enemiesToTargetProjs.Where(x => (x.Distance(HeroPlayer) >= QRange) && (x.Distance(HeroPlayer) <= EX2Range));
            var enemiesToExecuteEX1 = enemiesToTargetProjs.Where(x => x.Living.Health <= EX1Damage);
            var enemiesToExcuteEX2 = enemiesToTargetEX2.Where(x => x.Living.Health <= EX2Damage);

            // var alliesToTargetBase = EntitiesManager.LocalTeam.Where(x => !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial);
            // var alliesSelf = EntitiesManager.LocalTeam.Where(x => x.IsLocalPlayer && !x.HasBuff("HasteBuff"));

            // var targetSelf = TargetSelector.GetTarget(alliesSelf, targetMode, M1Range);
            var targetM1 = TargetSelector.GetTarget(enemiesToTargetProjs, targetMode, M1Range);
            var targetM2 = TargetSelector.GetTarget(enemiesToTargetXStrike, targetMode, M2MaxRange);
            var targetQ = TargetSelector.GetTarget(enemiesToTargetQ, targetMode, QRange);
            var targetE = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, ERange);
            var targetEX1 = TargetSelector.GetTarget(enemiesToTargetProjs, targetMode, EX1Range);
            var targetEX2 = TargetSelector.GetTarget(enemiesToTargetEX2, targetMode, EX2Range);
            var executeEX1 = TargetSelector.GetTarget(enemiesToExecuteEX1, targetMode, EX1Range);
            var executeEX2 = TargetSelector.GetTarget(enemiesToExcuteEX2, targetMode, (EX2Range /2));
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

                if (!IsInUltimate)
                {
                    switch (LastAbilityFired)
                    {

                    // case AbilitySlot.Ability7:
                    //     break;

                    case AbilitySlot.EXAbility2:
                        if (executeEX2 != null)
                        {
                            LocalPlayer.Aim(executeEX2.MapObject.Position);
                            return;
                        }
                        else if (targetEX2 != null)
                        {
                            LocalPlayer.Aim(targetEX2.MapObject.Position);
                            return;
                        }
                        break;
                    
                    case AbilitySlot.EXAbility1:
                        if (executeEX1 != null)
                        {
                            var pred = TestPrediction.GetNormalLinePrediction(MyPos, executeEX1, EX1Range, EX1Speed, EX1Radius);
                            if (pred.CanHit)
                            {
                                LocalPlayer.Aim(pred.CastPosition);
                            }
                        }
                        else if (targetEX1 != null)
                        {
                            var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetEX1, EX1Range, EX1Speed, EX1Radius);
                            if (pred.CanHit)
                            {
                                LocalPlayer.Aim(pred.CastPosition);
                            }
                        }
                        break;

                    // case AbilitySlot.Ability6:
                    //     break;

                    case AbilitySlot.Ability5:
                        if (targetE != null)
                        {
                            LocalPlayer.Aim(targetE.MapObject.Position);
                            return;
                        }
                        else if (targetM1 != null)
                        {
                            LocalPlayer.Aim(HeroPlayer.MapObject.Position);
                        }
                        break;

                        // case AbilitySlot.Ability4:
                        //     break;

                        // case AbilitySlot.Ability3:
                        //     break;

                        case AbilitySlot.Ability2:
                        if (targetM2 != null)
                        {
                            var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetM2, M2MaxRange, M2Speed, M2Radius, true);
                            if (pred.CanHit)
                            {
                                LocalPlayer.Aim(pred.CastPosition);
                            }
                        }
                        break;

                    case AbilitySlot.Ability1:
                        if (targetM1 != null)
                        {
                            var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetM1, M1Range, M1Speed, M1Radius, true);
                            if (pred.CanHit)
                            {
                                LocalPlayer.Aim(pred.CastPosition);
                            }
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
                    switch (LastAbilityFired)
                    {
                    //RazorBoomerangHasteUltimateAbility
                    case AbilitySlot.Ability1:
                        if (targetM1 != null)
                        {
                            var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetM1, M1Range, M1Speed, M1Radius, true);
                            if (pred.CanHit)
                            {
                                LocalPlayer.Aim(pred.CastPosition);
                            }
                        }
                        else
                        {
                            LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                        }
                        break;
                    }
                }
            }
            else
            {
                LocalPlayer.EditAimPosition = false;
                LastAbilityFired = null;
            }

            if (!IsInUltimate)
            {
                if (ComboMenu.GetBoolean("combo.useF") && MiscUtils.CanCast(AbilitySlot.Ability7) &&
                !MiscUtils.CanCast(AbilitySlot.Ability3) && !MiscUtils.CanCast(AbilitySlot.Ability5) &&
                !HeroPlayer.HasBuff("Haste"))
                {
                    if (LastAbilityFired == null && targetM1 != null)
                    {
                        LocalPlayer.PressAbility(AbilitySlot.Ability7, true);
                        LocalPlayer.EditAimPosition = true;
                        LocalPlayer.Aim(targetM1.MapObject.Position);
                        return;
                    }
                }

                if (MiscMenu.GetBoolean("misc.useEX2exec") && MiscUtils.CanCast(AbilitySlot.EXAbility2))
                {
                    var energyRequired = 25;
                    if (energyRequired <= HeroPlayer.Energized.Energy)
                    {
                        if  (LastAbilityFired == null && executeEX2 != null)
                        {
                            LocalPlayer.PressAbility(AbilitySlot.EXAbility2, true);
                            LocalPlayer.EditAimPosition = true;
                            LocalPlayer.Aim(executeEX2.MapObject.Position);
                            return;
                        }
                    }
                }

                if (ComboMenu.GetBoolean("combo.useEX2haste") && MiscUtils.CanCast(AbilitySlot.EXAbility2))
                {
                    var energyRequired = ComboMenu.GetIntSlider("combo.useEX2haste.minEnergyBars") * 25;
                    if (energyRequired <= HeroPlayer.Energized.Energy)
                    {
                        if (LastAbilityFired == null && targetEX2 != null && HeroPlayer.HasBuff("HasteBuff"))
                        {
                            LocalPlayer.PressAbility(AbilitySlot.EXAbility2, true);
                            LocalPlayer.EditAimPosition = true;
                            LocalPlayer.Aim(targetEX2.MapObject.Position);
                            return;
                        }
                    }
                }

                if (MiscMenu.GetBoolean("misc.useEX1exec") && MiscUtils.CanCast(AbilitySlot.EXAbility1))
                {
                    if (LastAbilityFired == null && executeEX1 != null)
                    {
                        var pred = TestPrediction.GetNormalLinePrediction(MyPos, executeEX1, EX1Range, EX1Speed, EX1Radius, true);
                        if (pred.CanHit)
                        {
                            LocalPlayer.PressAbility(AbilitySlot.EXAbility1, true);
                            LocalPlayer.EditAimPosition = true;
                            LocalPlayer.Aim(pred.CastPosition);
                            return;
                        }
                    }
                }

                if (ComboMenu.GetBoolean("combo.useEX1") && MiscUtils.CanCast(AbilitySlot.EXAbility1))
                {
                    if (LastAbilityFired == null && targetEX1 != null)
                    {
                        var energyRequired = ComboMenu.GetIntSlider("combo.useEX1.minEnergyBars") * 25;
                        if (energyRequired <= HeroPlayer.Energized.Energy)
                        {
                            var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetEX1, EX1Range, EX1Speed, EX1Radius, true);
                            if (pred.CanHit)
                            {
                                LocalPlayer.PressAbility(AbilitySlot.EXAbility1, true);
                                LocalPlayer.EditAimPosition = true;
                                LocalPlayer.Aim(pred.CastPosition);
                                return;
                            }
                        }
                    }
                }

                if (ComboMenu.GetBoolean("combo.useE") && MiscUtils.CanCast(AbilitySlot.Ability5) &&
                !MiscUtils.CanCast(AbilitySlot.Ability3))
                {
                    if (LastAbilityFired == null && targetE != null && !HeroPlayer.HasBuff("HasteBuff"))
                    {
                        LocalPlayer.PressAbility(AbilitySlot.Ability5, true);
                        LocalPlayer.EditAimPosition = true;
                        LocalPlayer.Aim(targetE.MapObject.Position);
                        return;
                    }
                }

                if (MiscMenu.GetBoolean("misc.useEself") && MiscUtils.CanCast(AbilitySlot.Ability5) &&
                !MiscUtils.CanCast(AbilitySlot.Ability3))
                {
                    if (LastAbilityFired == null && targetM1 != null && !HeroPlayer.HasBuff("HasteBuff"))
                    {
                        LocalPlayer.PressAbility(AbilitySlot.Ability5, true);
                        LocalPlayer.EditAimPosition = true;
                        LocalPlayer.Aim(HeroPlayer.MapObject.Position);
                        return;
                    }
                }

                if (ComboMenu.GetBoolean("combo.useSpace") && MiscUtils.CanCast(AbilitySlot.Ability3))
                {
                    if (HeroPlayer.EnemiesAroundAlive(M1Range) > 0)
                    {
                        if (LastAbilityFired == null && !HeroPlayer.HasBuff("HasteBuff"))
                        {
                            LocalPlayer.PressAbility(AbilitySlot.Ability3, true);
                            return;
                        }
                    }
                }

                if (ComboMenu.GetBoolean("combo.hasteQ") && MiscUtils.CanCast(AbilitySlot.Ability4))
                {
                    if (LastAbilityFired == null && targetQ != null && HeroPlayer.HasBuff("HasteBuff"))
                    {
                        LocalPlayer.PressAbility(AbilitySlot.Ability4, true);
                        return;
                    }
                }

                if (ComboMenu.GetBoolean("combo.useQ") && MiscUtils.CanCast(AbilitySlot.Ability4))
                {
                    if (ComboMenu.GetBoolean("combo.hasteQ") && MiscUtils.CanCast(AbilitySlot.Ability4))
                    {
                        if (LastAbilityFired == null && targetQ != null && HeroPlayer.HasBuff("HasteBuff"))
                        {
                            LocalPlayer.PressAbility(AbilitySlot.Ability4, true);
                            return;
                        }
                    }
                    else if (!ComboMenu.GetBoolean("combo.hasteQ") && MiscUtils.CanCast(AbilitySlot.Ability4))
                    {
                        if (LastAbilityFired == null && targetQ != null)
                        {
                            LocalPlayer.PressAbility(AbilitySlot.Ability4, true);
                            return;
                        }
                    }
                }

                if (ComboMenu.GetBoolean("combo.useM2") && MiscUtils.CanCast(AbilitySlot.Ability2))
                {
                    if (LastAbilityFired == null && targetM2 != null && HeroPlayer.HasBuff("HasteBuff") && !IsInUltimate)
                    {
                        var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetM2, M2MaxRange, M2Speed, M2Radius, true);
                        if (pred.CanHit)
                        {
                            LocalPlayer.PressAbility(AbilitySlot.Ability2, true);
                            LocalPlayer.EditAimPosition = true;
                            LocalPlayer.Aim(pred.CastPosition);
                        }
                    }
                }

                if (ComboMenu.GetBoolean("combo.useM1") && MiscUtils.CanCast(AbilitySlot.Ability1))
                {
                    if (LastAbilityFired == null && targetM1 != null)
                    {
                        var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetM1, M1Range, M1Speed, M1Radius, true);
                        if (pred.CanHit)
                        {
                            LocalPlayer.PressAbility(AbilitySlot.Ability1, true);
                            LocalPlayer.EditAimPosition = true;
                            LocalPlayer.Aim(pred.CastPosition);
                        }
                    }
                }
            }
            else
            {
                //RazorBoomerangHasteUltimateAbility / CompanionCallMountBuff
                if (ComboMenu.GetBoolean("combo.useM1") && MiscUtils.CanCast(AbilitySlot.Ability1))
                {
                    if (LastAbilityFired == null && targetM1 != null)
                    {
                        var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetM1, M1Range, M1Speed, M1Radius);
                        if (pred.CanHit)
                        {
                            LocalPlayer.PressAbility(AbilitySlot.Ability1, true);
                            LocalPlayer.EditAimPosition = true;
                            LocalPlayer.Aim(pred.CastPosition);
                        }
                    }
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

        // private static void OrbMode()
        // {
        //     var orb = EntitiesManager.CenterOrb;
        //     if (orb == null || !orb.IsValid || !orb.IsActiveObject)
        //     {
        //         return;
        //     }
        //
        //     Console.WriteLine("Orb SPAWN???");
        //
        //     var getOrb = orb.Get<LivingObject>();
        //     if (getOrb.IsDead) return;
        //
        //     var orbMapObj = orb.Get<MapGameObject>();
        //     var orbPos = orbMapObj.Position;
        //
        //     LocalPlayer.EditAimPosition = true;
        //     LocalPlayer.Aim(orbPos);
        //
        //     if (HeroPlayer.Distance(orbPos) <= EX2Range)
        //     {
        //         if (MiscUtils.CanCast(AbilitySlot.EXAbility2) && getOrb.Health <= (EX2Damage))
        //         {
        //             LocalPlayer.PressAbility(AbilitySlot.EXAbility2, true);
        //             return;
        //         }
        //     }
        //
        //     if (HeroPlayer.Distance(orbPos) <= EX1Range)
        //     {
        //         if (MiscUtils.CanCast(AbilitySlot.EXAbility1))
        //         {
        //             LocalPlayer.PressAbility(AbilitySlot.EXAbility1, true);
        //             return;
        //         }
        //     }
        //
        //     if (HeroPlayer.Distance(orbPos) <= M1Range)
        //     {
        //         if (MiscUtils.CanCast(AbilitySlot.Ability1))
        //         {
        //             LocalPlayer.PressAbility(AbilitySlot.Ability1, true);
        //         }
        //     }
        // }

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

            if (DrawingsMenu.GetBoolean("draw.Target"))
            {
                Drawing.DrawCircle(LocalPlayer.AimPosition, 1f, Color.red);
            }
        }

        public void OnUnload()
        {

        }
    }
}
