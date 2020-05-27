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

namespace KVRook
{
    public class KVRook : IAddon
    {
        private static Menu HeroMenu;
        private static Menu KeysMenu, ComboMenu, HealMenu, DrawingsMenu, MiscMenu;

        private static Character HeroPlayer => LocalPlayer.Instance;
        private static readonly string HeroName = "Rook";
        private static Vector2 MyPos => HeroPlayer.MapObject.Position;

        private static AbilitySlot? LastAbilityFired = null;

        //SkillType.Circle

        //Range
        private const float M1Range = 2.5f;
        private const float M2Range = 3f;
        private const float SpaceRange = 10f;
        // private const float QRange = 0.0f;
        private const float ERange = 10.0f;
        private const float RRange = 4.0f;
        private const float FRange = 10f;
        // private const float EX1Range = 0.0f;
        private const float EX2Range = 10f;

        //Speed
        // private const float M1Speed = 0.0;
        // private const float M2Speed = 0.0f;
        private const float SpaceSpeed = 25f;
        // private const float QSpeed = 0.0f;
        private const float ESpeed = 20f;
        private const float RSpeed = 25;
        private const float FSpeed = 25;
        // private const float EX1Speed = 0.0f;
        private const float EX2Speed = 25f;

        //Radius
        // private const float M1Radius = 0.0f;
        private const float M2Radius = 1.9f;
        private const float SpaceRadius = 0.5f;
        // private const float QRadius = 0.0f;
        private const float ERadius = 2.35f;
        private const float RRadius = 3f;
        private const float FRadius = 1f;
        // private const float EX1Radius = 0.0f;
        private const float EX2Radius = 0.5f;
        //

        //AirTime
        // private const float M1AirTime = 0.0f;
        private const float M2AirTime = 0.5f;
        // private const float SpaceAirTime = 0.0f;
        // private const float QAirTime = 0.0f;
        private const float EAirTime = 0.8f;
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
            HeroMenu = new Menu("templatemenu", "Kavey's Rook!");

            KeysMenu = new Menu("keysmenu", "Keys", true);
            KeysMenu.Add(new MenuKeybind("keys.combo", "Combo Key", UnityEngine.KeyCode.Mouse0));
            KeysMenu.Add(new MenuKeybind("keys.changeTargeting", "Change targeting mode", UnityEngine.KeyCode.Y, false, true));
            KeysMenu.Add(new MenuCheckBox("keys.autoCombo", "Auto Combo Mode", false));
            KeysMenu.Add(new MenuKeybind("keys.Space", "Space keybind to Auto Aim", UnityEngine.KeyCode.Space));
            KeysMenu.Add(new MenuKeybind("keys.F", "F keybind to Auto Aim", UnityEngine.KeyCode.F));
            KeysMenu.Add(new MenuKeybind("keys.M1", "Left Mouse keybind to pause Auto Combo", UnityEngine.KeyCode.Mouse2));
            KeysMenu.Add(new MenuKeybind("keys.M2", "Right Mouse keybind to pause Auto Combo", UnityEngine.KeyCode.Mouse1));
            KeysMenu.Add(new MenuKeybind("keys.E", "E keybind to pause Auto Combo", UnityEngine.KeyCode.E));
            KeysMenu.Add(new MenuKeybind("keys.R", "R keybind to pause Auto Combo", UnityEngine.KeyCode.R));
            HeroMenu.Add(KeysMenu);

            ComboMenu = new Menu("combomenu", "Combo", true);
            ComboMenu.Add(new MenuCheckBox("combo.invisible", "Attack invisible enemies", true));
            ComboMenu.Add(new MenuCheckBox("combo.useM1", "Use Left Mouse", false));
            ComboMenu.Add(new MenuCheckBox("combo.useM2", "Use Right Mouse", false));
            ComboMenu.Add(new MenuCheckBox("combo.useE", "Use E", false));
            ComboMenu.Add(new MenuSlider("combo.useEminRange", "Use E min range", 4f, 6f, 2.5f));
            ComboMenu.Add(new MenuCheckBox("combo.useR", "Use R", false));
            ComboMenu.Add(new MenuIntSlider("combo.useR.minEnergyBars", "    ^ Min energy bars", 2, 4, 1)); 
            ComboMenu.Add(new MenuCheckBox("combo.useEX2", "Use EX2", false));
            ComboMenu.Add(new MenuIntSlider("combo.useEX2.minEnergyBars", "    ^ Min energy bars", 2, 4, 1));
            HeroMenu.Add(ComboMenu);

            HealMenu = new Menu("healmenu", "Healing", true);
            HealMenu.Add(new MenuCheckBox("heal.useEX1", "Use EX1", false));
            HealMenu.Add(new MenuIntSlider("heal.useEX1.minEnergyBars", "    ^ Min energy bars", 1, 4, 1));
            HeroMenu.Add(HealMenu);

            MiscMenu = new Menu("miscmenu", "Misc", true);
            HeroMenu.Add(MiscMenu);

            DrawingsMenu = new Menu("drawmenu", "Drawings", true);
            DrawingsMenu.Add(new MenuCheckBox("draw.rangeM1.safeRange", "Draw Left Mouse Range", false));
            DrawingsMenu.Add(new MenuCheckBox("draw.Target", "Draw Target", false));
            HeroMenu.Add(DrawingsMenu);

            MainMenu.AddMenu(HeroMenu);
        }

        private static void OnUpdate(EventArgs args)
        {
            if (HeroPlayer.Living.IsDead)
            {
                return;
            }

            else if (KeysMenu.GetKeybind("keys.M1") || KeysMenu.GetKeybind("keys.M2") || 
            KeysMenu.GetKeybind("keys.E") || KeysMenu.GetKeybind("keys.R") || KeysMenu.GetKeybind("keys.F"))
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

            var enemiesToTargetBase = EntitiesManager.EnemyTeam.Where(x => x.IsValid && !x.Living.IsDead && !x.HasBuff("OtherSideBuff") &&
                !x.HasBuff("AscensionBuff") && !x.HasBuff("AscensionTravelBuff") && !x.HasBuff("Fleetfoot") && !x.HasBuff("TempestRushBuff") &&
                !x.HasBuff("ValiantLeap") && !x.HasBuff("FrogLeap") && !x.HasBuff("FrogLeapRecast") && !x.HasBuff("ElusiveStrikeCharged") &&
                !x.HasBuff("ElusiveStrikeWall2") && !x.HasBuff("BurrowAlternate") && !x.HasBuff("JetPack") && !x.HasBuff("ProwlBuff") &&
                !x.HasBuff("Dive") && !x.HasBuff("InfestingBuff") && !x.HasBuff("PortalBuff") && !x.HasBuff("CrushingBlow") && !x.HasBuff("TornadoBuff"));

            if (!ComboMenu.GetBoolean("combo.invisible"))
            {
                enemiesToTargetBase = enemiesToTargetBase.Where(x => !x.CharacterModel.IsModelInvisible);
            }

            var enemiesToTargetProjs = enemiesToTargetBase.Where(x =>
                !x.IsCountering && !x.HasShield() && !x.HasConsumeBuff && !x.HasParry() &&
                !x.HasBuff("ElectricShield") && !x.HasBuff("BarbedHuskBuff"));

            var targetM1 = TargetSelector.GetTarget(enemiesToTargetProjs, targetMode, M1Range);
            var targetM2 = TargetSelector.GetTarget(enemiesToTargetProjs, targetMode, M2Range);
            var targetSpace = TargetSelector.GetTarget(enemiesToTargetProjs, targetMode, SpaceRange);
            // var targetQ = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, QRange);
            var targetE = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, ERange);
            var targetR = TargetSelector.GetTarget(enemiesToTargetProjs, targetMode, RRange);
            // var targetEX1 = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, EX1Range);
            var targetEX2 = TargetSelector.GetTarget(enemiesToTargetProjs, targetMode, EX2Range);
            var targetF = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, FRange);
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

            var IsBerserk = HeroPlayer.HasBuff("BerserkBuff");
            var IsCountering = HeroPlayer.HasBuff("BerskTrance");
            var TremorChannel = HeroPlayer.HasBuff("TremorChannel");
            var isCastingOrChanneling = HeroPlayer.AbilitySystem.IsCasting || HeroPlayer.IsChanneling || IsCountering;

            if (isCastingOrChanneling && LastAbilityFired == null)
            {
                LastAbilityFired = CastingIndexToSlot(HeroPlayer.AbilitySystem.CastingAbilityIndex);
            }

            if (KeysMenu.GetKeybind("keys.Space"))
            {
                if (targetSpace != null)
                {
                    LocalPlayer.Aim(targetSpace.MapObject.Position);
                }
                else
                {
                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                }
            }

            if (KeysMenu.GetKeybind("keys.F"))
            {
                if (targetF != null)
                {
                    LocalPlayer.Aim(targetF.MapObject.Position);
                }
            }

            if (isCastingOrChanneling)
            {
                LocalPlayer.EditAimPosition = true;

                switch (LastAbilityFired)
                {
                    case AbilitySlot.Ability1:
                        if (targetM1 != null)
                        {
                            LocalPlayer.Aim(targetM1.MapObject.Position);
                        }
                        else
                        {
                            LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                        }
                        break;

                    case AbilitySlot.Ability2:
                        if (targetM2 != null)
                        {
                            var pred = TestPrediction.GetPrediction(MyPos, targetM2, M2Range, 0f, M2Radius, M2AirTime);
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

                    case AbilitySlot.EXAbility2:
                        if (targetEX2 != null)
                        {
                            var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetEX2, EX2Range, EX2Speed, EX2Radius, true);
                            if (pred.CanHit)
                            {
                                LocalPlayer.Aim(pred.CastPosition);
                            }
                            else
                            {
                                LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                        }
                        break;

                    case AbilitySlot.Ability5:
                        if (targetE != null)
                        {
                            var pred = TestPrediction.GetPrediction(MyPos, targetE, ERange, 0f, ERadius, EAirTime);
                            if (pred.CanHit)
                            {
                                LocalPlayer.Aim(pred.CastPosition);
                            }
                            else
                            {
                                LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                        }
                        break;

                    case AbilitySlot.Ability6:
                        if (targetR != null)
                        {
                            var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetR, RRange, RSpeed, RRadius);
                            if (pred.CanHit)
                            {
                                LocalPlayer.Aim(pred.CastPosition);
                            }
                            else
                            {
                                LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                        }
                        break;
                }
            }
            else
            {
                LocalPlayer.EditAimPosition = false;
                LastAbilityFired = null;
            }

            if (!isCastingOrChanneling)
            {
                if (HealMenu.GetBoolean("heal.useEX1") && MiscUtils.CanCast(AbilitySlot.EXAbility1) && !IsBerserk)
                {
                    var energyRequired = HealMenu.GetIntSlider("heal.useEX1.minEnergyBars") * 25;
                    if (energyRequired <= HeroPlayer.Energized.Energy)
                    {
                        var IsEating = HeroPlayer.HasBuff("EatBuff");
                        var Fish = 30;
                        var EatFish = !IsEating && (HeroPlayer.Living.Health <= (HeroPlayer.Living.MaxRecoveryHealth - Fish));
                        if (LastAbilityFired == null && EatFish)
                        {
                            LocalPlayer.PressAbility(AbilitySlot.EXAbility1, true);
                            return;
                        }
                    }
                }

                if (ComboMenu.GetBoolean("combo.useE") && MiscUtils.CanCast(AbilitySlot.Ability5) && !IsBerserk)
                {
                    var pred = TestPrediction.GetPrediction(MyPos, targetE, ERange, ESpeed, ERadius, EAirTime);
                    if (pred.CanHit)
                    {
                        if (LastAbilityFired == null && targetE != null && !targetE.HasBuff("AmorBreak"))
                        {
                            if (targetE.Distance(HeroPlayer) >= ComboMenu.GetSlider("combo.useEminRange"))
                            {
                                LocalPlayer.PressAbility(AbilitySlot.Ability5, true);
                                LocalPlayer.EditAimPosition = true;
                                LocalPlayer.Aim(pred.CastPosition);
                                return;
                            }
                        }
                    }
                }

                if (ComboMenu.GetBoolean("combo.useEX2") && MiscUtils.CanCast(AbilitySlot.EXAbility2) && !IsBerserk)
                {
                    var energyRequired = ComboMenu.GetIntSlider("combo.useEX2.minEnergyBars") * 25;
                    if (energyRequired <= HeroPlayer.Energized.Energy)
                    {
                        if (LastAbilityFired == null && targetEX2 != null && targetEX2.Distance(HeroPlayer) >= 5f)
                        {
                            LocalPlayer.PressAbility(AbilitySlot.EXAbility2, true);
                            return;
                        }
                    }
                }

                if (ComboMenu.GetBoolean("combo.useM2") && MiscUtils.CanCast(AbilitySlot.Ability2))
                {
                    var pred = TestPrediction.GetPrediction(MyPos, targetM2, M2Range, 0f, M2Radius, M2AirTime);
                    if (pred.CanHit)
                    {
                        if (LastAbilityFired == null && targetM2 != null && !targetM2.HasBuff("AmorBreak") && (targetM2.AbilitySystem.IsCasting || targetM2.IsChanneling))
                        {
                            LocalPlayer.PressAbility(AbilitySlot.Ability2, true);
                            LocalPlayer.EditAimPosition = true;
                            LocalPlayer.Aim(pred.CastPosition);
                            return;
                        }
                    }
                }

                if (ComboMenu.GetBoolean("combo.useR") && MiscUtils.CanCast(AbilitySlot.Ability6))
                {
                    var energyRequired = ComboMenu.GetIntSlider("combo.useR.minEnergyBars") * 25;
                    if (energyRequired <= HeroPlayer.Energized.Energy)
                    {
                        if (LastAbilityFired == null && targetR != null)
                        {
                            LocalPlayer.PressAbility(AbilitySlot.Ability6, true);
                            //var direction = (Utility.Player.MapObject.Position - RTarget.MapObject.Position).Normalized;
                            //var endPos = Utility.Player.MapObject.Position + (direction * 4f);
                            //Drawing.DrawLine(RTarget.MapObject.ScreenPosition, endPos.WorldToScreen(), Color.red);
                            return;
                        }
                    }
                }

                if (ComboMenu.GetBoolean("combo.useM1") && MiscUtils.CanCast(AbilitySlot.Ability1))
                {
                    if (LastAbilityFired == null && targetM1 != null)
                    {
                        LocalPlayer.PressAbility(AbilitySlot.Ability1, true);
                        return;
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
                Drawing.DrawCircle(LocalPlayer.AimPosition, 0.5f, UnityEngine.Color.red);
            }
        }

        public void OnUnload()
        {

        }
    }
}
