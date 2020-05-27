﻿using System;
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

namespace PipLucie
{
    public class PipLucie : IAddon
    {
        private static Menu LucieMenu;
        private static Menu KeysMenu, ComboMenu, HealMenu, DrawingsMenu, MiscMenu;

        private static Character LucieHero => LocalPlayer.Instance;
        private static readonly string HeroName = "Lucie";
        private static Vector2 MyPos => LucieHero.MapObject.Position;

        private static AbilitySlot? LastAbilityFired = null;

        private const float M1Range = 7.4f;
        private const float M2Range = 10f;
        private const float QRange = 8f;
        private const float ERange = 6.8f;
        private const float RRange = 5f;
        private const float FRange = 7.5f;
        private const float EX1Range = 8.8f;
        private const float EX2Range = 9.8f;

        private const float M1Speed = 16.8f;
        private const float M2Speed = 28f;
        private const float QAirTime = 0.55f;
        private const float ESpeed = 22.5f;
        private const float EX1Speed = 23.9f;
        private const float EX2Speed = 22.5f;
        private const float FAirTime = 0.75f;

        private const float M1Radius = 0.25f;
        private const float M2Radius = 1.2f;
        private const float QRadius = 2f;
        private const float ERadius = 0.35f;
        private const float FRadius = 3.5f;
        private const float EX1Radius = 0.35f;
        private const float EX2Radius = 0.35f;

        private const float M1Damage = 12;
        private const float M2Revitalize = 6;

        private static bool DidMatchInit = false;

        public void OnInit()
        {
            InitMenu();

            Game.OnMatchStart += OnMatchStart;
            Game.OnMatchEnd += OnMatchEnd;
        }

        private void OnMatchStart(EventArgs args)
        {
            if (LucieHero == null || !LucieHero.CharName.Equals(HeroName))
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
            LucieMenu = new Menu("pipluciemenu", "DaPip's Lucie Rework");

            KeysMenu = new Menu("keysmenu", "Keys", true);
            KeysMenu.Add(new MenuKeybind("keys.combo", "Combo Key", UnityEngine.KeyCode.Mouse0));
            KeysMenu.Add(new MenuKeybind("keys.healSelf", "Heal self", UnityEngine.KeyCode.LeftControl));
            KeysMenu.Add(new MenuKeybind("keys.changeTargeting", "Change targeting mode", UnityEngine.KeyCode.Y, false, true));
            KeysMenu.Add(new MenuCheckBox("keys.autoCombo", "Auto Combo Mode", false));
            KeysMenu.Add(new MenuKeybind("keys.M1", "Left Mouse keybind to pause Auto Combo", UnityEngine.KeyCode.Mouse2));
            KeysMenu.Add(new MenuKeybind("keys.Q", "Q keybind to pause Auto Combo", UnityEngine.KeyCode.Alpha1));
            KeysMenu.Add(new MenuKeybind("keys.Space", "Space keybind to pause Auto Combo", UnityEngine.KeyCode.Space));
            KeysMenu.Add(new MenuKeybind("keys.R", "R keybind to pause Auto Combo", UnityEngine.KeyCode.R));
            LucieMenu.Add(KeysMenu);

            ComboMenu = new Menu("combomenu", "Combo", true);
            ComboMenu.Add(new MenuCheckBox("combo.invisible", "Attack invisible enemies", true));
            ComboMenu.Add(new MenuCheckBox("combo.useM1", "Use Left Mouse (Toxic Bolt)", true));
            ComboMenu.Add(new MenuSlider("combo.M1safeRangeAutoCombo", "Left Mouse Auto Combo safe range", 3.5f, 10f, 0f));
            ComboMenu.Add(new MenuCheckBox("combo.useQ", "Use Q (Clarity Potion) to remove buffs from enemies", true));
            ComboMenu.Add(new MenuCheckBox("combo.useQA", "Use Q (Clarity Potion) to cleanse allies", true));
            ComboMenu.Add(new MenuCheckBox("combo.useE", "Use E (Panic Flask)", true));
            ComboMenu.Add(new MenuCheckBox("combo.useEX1", "Use EX1 (Deadly Injection)", true));
            ComboMenu.Add(new MenuIntSlider("combo.useEX1.minEnergyBars", "    ^ Min energy bars", 2, 4, 1));
            ComboMenu.Add(new MenuCheckBox("combo.useEX2", "Use EX2 (Petrify Bolt)", true));
            ComboMenu.Add(new MenuIntSlider("combo.useEX2.minEnergyBars", "    ^ Min energy bars", 3, 4, 1));
            ComboMenu.Add(new MenuCheckBox("combo.useR", "Use R (Roll) when enemies are too close", false));
            ComboMenu.Add(new MenuComboBox("combo.useR.direction", "    ^ Direction", 0, new[] { "Safest teammate", "Mouse Position" }));
            ComboMenu.Add(new MenuIntSlider("combo.useR.minEnergyBars", "    ^ Min energy bars", 1, 4, 1));
            ComboMenu.Add(new MenuSlider("combo.useR.safeRange", "    ^ Safe range", 1.7f, 2.5f, 0f));
            ComboMenu.Add(new MenuCheckBox("combo.useF", "Use F (Crippling Goo)", false));
            LucieMenu.Add(ComboMenu);

            HealMenu = new Menu("healmenu", "Healing", true);
            HealMenu.AddLabel("Hold Healing key to use");
            HealMenu.Add(new MenuSlider("heal.allySafeRange", "Target ally safe range", 10f, 10f, 0f));
            HealMenu.Add(new MenuCheckBox("heal.useM2", "Heal with M2 (Healing Potion)", true));
            HealMenu.Add(new MenuSlider("heal.useM2.safeRange", "    ^ Safe range", 4f, 10f, 0f));
            HealMenu.Add(new MenuCheckBox("heal.useM2.fullHP", "    ^ Use even if target ally has full health", false));
            HealMenu.Add(new MenuCheckBox("heal.useSpace", "Use Space (Barrier)", false));
            HealMenu.Add(new MenuComboBox("heal.useSpace.mode", "    ^ Priority Mode", 0, new[] { "Surrounded by most enemies", "Closest to mouse" }));
            LucieMenu.Add(HealMenu);

            MiscMenu = new Menu("miscmenu", "Misc", true);
            MiscMenu.Add(new MenuCheckBox("misc.antiRook", "Anti Rooks M2 (Crushing Blow)", true));
            LucieMenu.Add(MiscMenu);

            DrawingsMenu = new Menu("drawmenu", "Drawings", true);
            DrawingsMenu.Add(new MenuCheckBox("draw.healSafeRange", "Draw Healing safe Range", false));
            DrawingsMenu.Add(new MenuCheckBox("draw.rangeM1.safeRange", "Draw Toxic Bolt Range", true));
            DrawingsMenu.Add(new MenuCheckBox("draw.Target", "Draw Target", true));
            LucieMenu.Add(DrawingsMenu);

            MainMenu.AddMenu(LucieMenu);
        }

        private static void OnUpdate(EventArgs args)
        {
            if (LucieHero.Living.IsDead)
            {
                return;
            }

            else if ((KeysMenu.GetKeybind("keys.M1")) || (KeysMenu.GetKeybind("keys.Q")) || (KeysMenu.GetKeybind("keys.Space")) || (KeysMenu.GetKeybind("keys.R")))
            {
                LocalPlayer.EditAimPosition = false;
                LastAbilityFired = null;
            }

            else if (KeysMenu.GetKeybind("keys.healSelf"))
            {
                HealSelf();
            }
            else if (KeysMenu.GetBoolean("keys.autoCombo"))
            {
                HealOthers();
                ComboMode();
            }
            else if (KeysMenu.GetKeybind("keys.combo"))
            {
                HealOthers();
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
            var targetModeKey = KeysMenu.GetKeybind("keys.changeTargeting");
            var targetMode = targetModeKey ? TargetingMode.LowestHealth : TargetingMode.NearMouse;

            var enemiesToTargetBase = EntitiesManager.EnemyTeam.Where(x => x.IsValid && !x.Living.IsDead
            && !x.HasBuff("OtherSideBuff") && !x.HasBuff("AscensionBuff") && !x.HasBuff("AscensionTravelBuff") &&
            !x.HasBuff("Fleetfoot") && !x.HasBuff("TempestRushBuff") && !x.HasBuff("ValiantLeap") && !x.HasBuff("FrogLeap") &&
            !x.HasBuff("FrogLeapRecast") && !x.HasBuff("ElusiveStrikeCharged") && !x.HasBuff("ElusiveStrikeWall2") && 
            !x.HasBuff("BurrowAlternate") && !x.HasBuff("JetPack") && !x.HasBuff("ProwlBuff") && !x.HasBuff("Dive") &&
            !x.HasBuff("InfestingBuff") && !x.HasBuff("PortalBuff") && !x.HasBuff("CrushingBlow") && !x.HasBuff("TornadoBuff"));

            var alliesToTargetBaseQ = EntitiesManager.LocalTeam.Where(x => !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial);
            var enemiesToTargetBaseQ = EntitiesManager.EnemyTeam.Where(x => x.IsValid && !x.Living.IsDead);

            var CrushingBlowRange = 3.75f;
            var antiRook = EntitiesManager.EnemyTeam.Where(x => x.HasBuff("CrushingBlow") && (x.Distance(LucieHero) <= CrushingBlowRange));

            if (!ComboMenu.GetBoolean("combo.invisible"))
            {
                enemiesToTargetBase = enemiesToTargetBase.Where(x => !x.CharacterModel.IsModelInvisible);
            }

            var enemiesToTargetProjs = enemiesToTargetBase.Where(x =>
            !x.IsCountering && !x.HasShield() && !x.HasConsumeBuff && !x.HasParry() && !x.HasBuff("ElectricShield") && !x.HasBuff("BarbedHuskBuff"));

            // Dispelling Debuff (allies):
            //
            var alliesToTargetQ = alliesToTargetBaseQ.Where(x =>
            x.HasBuff("Weaken") || x.HasBuff("LunarStrikePetrify") || x.HasBuff("Incapacitate") ||
            (!x.IsLocalPlayer && (x.HasBuff("Panic") || x.HasBuff("DeadlyInjectionBuff"))) || 
            x.HasBuff("PhantomCutBuff") || x.HasBuff("EntanglingRootsBuff") || x.HasBuff("FrostDebuff") ||
            x.HasBuff("Frozen") || x.HasBuff("StormStruckDebuff") || x.HasBuff("BrainBugDebuff") || x.HasBuff("HandOfJudgementBuff") ||
            // x.HasBuff("HandOfCorruptionBuff) ||
            x.HasBuff("SheepTrickDebuff") || x.HasBuff("SludgeSpitDebuff") || x.HasBuff("BlindingLightBlind"));
            //

            // Dispelling Buff (ennemies):
            //
            var enemiesToTargetQ = enemiesToTargetBaseQ.Where(x =>
            // Bakko:
            x.HasBuff("BulwarkBuff") || x.HasBuff("WarShoutShield") ||
            // Croak:
            x.HasBuff("NewCamouflage") || x.HasBuff("Deceit") ||
            // Freya:
            // x.HasBuff("ElectricShield") || x.HasBuff("ElectricShieldSecondary") ||
            // Jamila:
            x.HasBuff("ShadowStalkStealth") ||
            // Raigon:
            // x.HasBuff("Parry") ||
            // Ruh Kaan:
            // x.HasBuff("ConsumeBuff") ||
            // Shifu:
            x.HasBuff("Fleetfoot") || x.HasBuff("TempestRushBuff") ||
            // Thorns:
            x.HasBuff("BarbedHuskBuff") ||
            // Alysia:
            x.HasBuff("GlacialPrism") ||
            // Ezmo:
            x.HasBuff("ArcaneWard") ||
            // Iva:
            // x.HasBuff("Zap") ||
            // Jade:
            x.HasBuff("Stealth") ||
            // Taya:
            // x.HasBuff("HasteBuff") ||
            // Varesh:
            // x.HasBuff("InhibitorsGuard") ||
            // Blossom:
            // x.HasBuff("InstinctStealth") ||
            // Lucie:
            x.HasBuff("Barrier") ||
            // Oldur:
            x.HasBuff("TimeBenderBuff") ||
            // Pearl:
            x.HasBuff("BubbleShield") ||
            // Pestilus:
            x.HasBuff("Swarm") ||
            // Poloma:
            x.HasBuff("OtherSideBuff") ||
            // Ulric:
            x.HasBuff("DivineShieldBuff") ||
            // Zander:
            x.HasBuff("RabbitFormBuff"));
            //

            var targetM1 = TargetSelector.GetTarget(enemiesToTargetProjs, targetMode, M1Range);
            var targetE = TargetSelector.GetTarget(enemiesToTargetProjs, targetMode, ERange);
            var alliesQ = TargetSelector.GetTarget(alliesToTargetQ, targetMode, QRange);
            var tantiRook = TargetSelector.GetTarget(antiRook, targetMode, QRange);
            var targetQ = TargetSelector.GetTarget(enemiesToTargetQ, targetMode, QRange);
            var targetEX1 = TargetSelector.GetTarget(enemiesToTargetProjs, targetMode, EX1Range);
            var targetEX2 = TargetSelector.GetTarget(enemiesToTargetProjs, targetMode, EX2Range);
            var targetF = TargetSelector.GetTarget(enemiesToTargetBase, targetMode, FRange);

            var isCastingOrChanneling = LucieHero.AbilitySystem.IsCasting || LucieHero.IsChanneling;

            if (isCastingOrChanneling && LastAbilityFired == null)
            {
                LastAbilityFired = CastingIndexToSlot(LucieHero.AbilitySystem.CastingAbilityIndex);
            }

            if (isCastingOrChanneling)
            {
                LocalPlayer.EditAimPosition = true;

                switch (LastAbilityFired)
                {
                    case AbilitySlot.Ability6:
                        var priorityMode = ComboMenu.GetComboBox("combo.useR.direction");
                        var bestPosition = MathUtils.GetBestJumpPosition(priorityMode, 46, RRange);
                        if (tantiRook != null)
                        {
                            LocalPlayer.Aim(bestPosition);
                        }

                        LocalPlayer.Aim(bestPosition);
                        break;

                    case AbilitySlot.Ability4:
                        if (tantiRook != null)
                        {
                            LocalPlayer.Aim(LucieHero.MapObject.Position);
                        }
                        else if (alliesQ != null)
                        {
                            var pred = TestPrediction.GetPrediction(MyPos, alliesQ, QRange, 0f, QRadius, QAirTime);
                            if (pred.CanHit)
                            {
                                LocalPlayer.Aim(pred.CastPosition);
                            }
                        }
                        else if (targetQ != null)
                        {
                            var pred = TestPrediction.GetPrediction(MyPos, targetQ, QRange, 0f, QRadius, QAirTime);
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

                    case AbilitySlot.Ability7:
                        if (targetF != null)
                        {
                            LocalPlayer.Aim(targetF.MapObject.Position);
                        }
                        else
                        {
                            LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                        }
                        break;

                    case AbilitySlot.Ability5:
                        if (targetE != null)
                        {
                            var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetE, ERange, ESpeed, ERadius, true);
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
                        }
                        else
                        {
                            LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                        }
                        break;

                    case AbilitySlot.EXAbility1:
                        if (targetEX1 != null)
                        {
                            var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetEX1, EX1Range, EX1Speed, EX1Radius, true);
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
                LocalPlayer.EditAimPosition = false;
                LastAbilityFired = null;
            }

            if (!isCastingOrChanneling && ComboMenu.GetBoolean("combo.useR") && MiscUtils.CanCast(AbilitySlot.Ability6))
            {
                var energyRequired = ComboMenu.GetIntSlider("combo.useR.minEnergyBars") * 25;
                if (energyRequired <= LucieHero.Energized.Energy)
                {
                    var safeRange = ComboMenu.GetSlider("combo.useR.safeRange");

                    if (LucieHero.EnemiesAroundAlive(safeRange) > 0)
                    {
                        var priorityMode = ComboMenu.GetComboBox("combo.useR.direction");
                        var bestPosition = MathUtils.GetBestJumpPosition(priorityMode, 46, RRange);

                        LocalPlayer.PressAbility(AbilitySlot.Ability6, true);
                        LocalPlayer.EditAimPosition = true;
                        LocalPlayer.Aim(bestPosition);
                        return;
                    }
                }
            }

            if (ComboMenu.GetBoolean("combo.useF") && MiscUtils.CanCast(AbilitySlot.Ability7))
            {
                if (LastAbilityFired == null && targetF != null && !targetF.HasBuff("Panic"))
                {
                    LocalPlayer.PressAbility(AbilitySlot.Ability7, true);
                    return;
                }
            }

            if (MiscMenu.GetBoolean("misc.antiRook") && MiscUtils.CanCast(AbilitySlot.Ability6))
            {
                var priorityMode = ComboMenu.GetComboBox("combo.useR.direction");
                var bestPosition = MathUtils.GetBestJumpPosition(priorityMode, 46, RRange);
                if (LastAbilityFired == null && tantiRook != null)
                {
                        LocalPlayer.PressAbility(AbilitySlot.Ability6, true);
                        LocalPlayer.EditAimPosition = true;
                        LocalPlayer.Aim(bestPosition);
                }
            }

            if (MiscMenu.GetBoolean("misc.antiRook") && MiscUtils.CanCast(AbilitySlot.Ability4))
            {
                if (LastAbilityFired == null && tantiRook != null)
                {
                    LocalPlayer.PressAbility(AbilitySlot.Ability4, true);
                    LocalPlayer.EditAimPosition = true;
                    LocalPlayer.Aim(LucieHero.MapObject.Position);
                    return;
                }
            }

            if (ComboMenu.GetBoolean("combo.useQA") && MiscUtils.CanCast(AbilitySlot.Ability4))
            {
                if (LastAbilityFired == null && alliesQ != null)
                {
                    var pred = TestPrediction.GetPrediction(MyPos, alliesQ, QRange, 0f, QRadius, QAirTime);
                    if (pred.CanHit)
                    {
                        LocalPlayer.PressAbility(AbilitySlot.Ability4, true);
                        LocalPlayer.EditAimPosition = true;
                        LocalPlayer.Aim(pred.CastPosition);
                        return;
                    }
                }
            }

            if (ComboMenu.GetBoolean("combo.useQ") && MiscUtils.CanCast(AbilitySlot.Ability4))
            {
                if (LastAbilityFired == null && targetQ != null)
                {
                    var pred = TestPrediction.GetPrediction(MyPos, targetQ, QRange, 0f, QRadius, QAirTime);
                    if (pred.CanHit)
                    {
                        LocalPlayer.PressAbility(AbilitySlot.Ability4, true);
                        LocalPlayer.EditAimPosition = true;
                        LocalPlayer.Aim(pred.CastPosition);
                        return;
                    }
                }
            }

            if (ComboMenu.GetBoolean("combo.useEX2") && MiscUtils.CanCast(AbilitySlot.EXAbility2))
            {
                var energyRequired = ComboMenu.GetIntSlider("combo.useEX2.minEnergyBars") * 25;
                if (energyRequired <= LucieHero.Energized.Energy)
                {
                    if (LastAbilityFired == null && targetEX2 != null)
                    {
                        var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetEX2, EX2Range, EX2Speed, EX2Radius, true);
                        if (pred.CanHit)
                        {
                            LocalPlayer.PressAbility(AbilitySlot.EXAbility2, true);
                            return;
                        }
                    }
                }
            }

            if (ComboMenu.GetBoolean("combo.useEX1") && MiscUtils.CanCast(AbilitySlot.EXAbility1))
            {
                var energyRequired = ComboMenu.GetIntSlider("combo.useEX1.minEnergyBars") * 25;
                if (energyRequired <= LucieHero.Energized.Energy)
                {
                    if (LastAbilityFired == null && targetEX1 != null)
                    {
                        var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetEX1, EX1Range, EX1Speed, EX1Radius, true);
                        if (pred.CanHit)
                        {
                            LocalPlayer.PressAbility(AbilitySlot.EXAbility1, true);
                            return;
                        }
                    }
                }
            }

            if (ComboMenu.GetBoolean("combo.useE") && MiscUtils.CanCast(AbilitySlot.Ability5))
            {
                if (LastAbilityFired == null && targetE != null && !targetE.HasBuff("CripplingGooSlow"))
                {
                    var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetE, ERange, ESpeed, ERadius, true);
                    if (pred.CanHit)
                    {
                        LocalPlayer.PressAbility(AbilitySlot.Ability5, true);
                        return;
                    }
                }
            }

            if (ComboMenu.GetBoolean("combo.useM1") && MiscUtils.CanCast(AbilitySlot.Ability1))
            {
                if (KeysMenu.GetBoolean("keys.autoCombo"))
                {
                    var safeRange = ComboMenu.GetSlider("combo.M1safeRangeAutoCombo");
                    if (LucieHero.EnemiesAroundAlive(safeRange) < 1)
                    {
                        if (LastAbilityFired == null && targetM1 != null)
                        {
                            var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetM1, M1Range, M1Speed,
                                M1Radius, true);
                            if (pred.CanHit)
                            {
                                LocalPlayer.PressAbility(AbilitySlot.Ability1, true);
                            }
                        }
                    }
                }
                else if (LastAbilityFired == null && targetM1 != null)
                {
                    var pred = TestPrediction.GetNormalLinePrediction(MyPos, targetM1, M1Range, M1Speed, M1Radius, true);
                    if (pred.CanHit)
                    {
                        LocalPlayer.PressAbility(AbilitySlot.Ability1, true);
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

        private static void HealOthers()
        {
            var useM2 = HealMenu.GetBoolean("heal.useM2");
            var useM2FullHP = HealMenu.GetBoolean("heal.useM2.fullHP");
            var useSpace = HealMenu.GetBoolean("heal.useSpace");
            var useSpaceMode = HealMenu.GetComboBox("heal.useSpace.mode");
            var safeRange = HealMenu.GetSlider("heal.allySafeRange");
            var M2safeRange = HealMenu.GetSlider("heal.useM2.safeRange");

            var possibleAllies = EntitiesManager.LocalTeam.Where(x => !x.IsLocalPlayer && 
            !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial);

            var allySpace = useSpaceMode == 0
                ? possibleAllies.Where(x => x.EnemiesAroundAlive(safeRange) > 0)
                .OrderByDescending(x => x.EnemiesAroundAlive(safeRange))
                .ThenBy(x => x.MapObject.ScreenPosition.Distance(InputManager.MousePosition)).FirstOrDefault()

                : possibleAllies.Where(x => x.EnemiesAroundAlive(safeRange) > 0)
                .OrderBy(x => x.MapObject.ScreenPosition.Distance(InputManager.MousePosition))
                .ThenBy(x => x.EnemiesAroundAlive(safeRange)).FirstOrDefault();

            if (!useM2FullHP)
            {
                possibleAllies = possibleAllies.Where(x => x.Living.Health < (x.Living.MaxRecoveryHealth - (M2Revitalize *2)));
            }

            var allyM2 = TargetSelector.GetTarget(possibleAllies, TargetingMode.NearMouse, M2Range);

            var isCastingOrChanneling = LucieHero.AbilitySystem.IsCasting || LucieHero.IsChanneling;

            if (isCastingOrChanneling && LastAbilityFired == null)
            {
                LastAbilityFired = CastingIndexToSlot(LucieHero.AbilitySystem.CastingAbilityIndex);
            }

            if (isCastingOrChanneling)
            {
                LocalPlayer.EditAimPosition = true;

                switch (LastAbilityFired)
                {
                    case AbilitySlot.Ability3:
                        if (allySpace != null)
                        {
                            LocalPlayer.Aim(allySpace.MapObject.Position);
                        }
                        break;

                    case AbilitySlot.Ability2:
                        if (allyM2 != null)
                        {
                            var pred = TestPrediction.GetNormalLinePrediction(MyPos, allyM2, M2Range, M2Speed, M2Radius, true);
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
                LocalPlayer.EditAimPosition = false;
                LastAbilityFired = null;
            }

            if (useSpace && MiscUtils.CanCast(AbilitySlot.Ability3))
            {
                if (LastAbilityFired == null && allySpace != null)
                {
                    LocalPlayer.PressAbility(AbilitySlot.Ability3, true);
                    LocalPlayer.EditAimPosition = true;
                    LocalPlayer.Aim(allySpace.MapObject.Position);
                    return;
                }
            }

            if (useM2 && MiscUtils.CanCast(AbilitySlot.Ability2))
            {
                if (LastAbilityFired == null && allyM2 != null && (LucieHero.EnemiesAroundAlive(M2safeRange) < 1))
                {
                    var pred = TestPrediction.GetNormalLinePrediction(MyPos, allyM2, M2Range, M2Speed, M2Radius, true);
                    if (pred.CanHit)
                    {
                        LocalPlayer.PressAbility(AbilitySlot.Ability2, true);
                        LocalPlayer.EditAimPosition = true;
                        LocalPlayer.Aim(pred.CastPosition);
                    }
                    return;
                }
            }
        }

        private static void HealSelf()
        {
            var useM2 = HealMenu.GetBoolean("heal.useM2");
            var useM2FullHP = HealMenu.GetBoolean("heal.useM2.fullHP");
            var useSpace = HealMenu.GetBoolean("heal.useSpace");
            var safeRange = HealMenu.GetSlider("heal.allySafeRange");
            var M2safeRange = HealMenu.GetSlider("heal.useM2.safeRange");

            var shouldSpace = useSpace && LucieHero.EnemiesAroundAlive(safeRange) > 0;

            var isCastingOrChanneling = LucieHero.AbilitySystem.IsCasting || LucieHero.IsChanneling;

            if (isCastingOrChanneling && LastAbilityFired == null)
            {
                LastAbilityFired = CastingIndexToSlot(LucieHero.AbilitySystem.CastingAbilityIndex);
            }

            if (isCastingOrChanneling)
            {
                LocalPlayer.EditAimPosition = true;

                switch (LastAbilityFired)
                {
                    case AbilitySlot.Ability3:
                    case AbilitySlot.Ability2:
                        LocalPlayer.Aim(LucieHero.MapObject.Position);
                        break;
                }
            }
            else
            {
                LocalPlayer.EditAimPosition = false;
                LastAbilityFired = null;
            }

            if (shouldSpace && MiscUtils.CanCast(AbilitySlot.Ability3) && LastAbilityFired == null)
            {
                LocalPlayer.PressAbility(AbilitySlot.Ability3, true);
            }

            if (MiscUtils.CanCast(AbilitySlot.Ability2) && LastAbilityFired == null)
            {
                LocalPlayer.PressAbility(AbilitySlot.Ability2, true);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (!Game.IsInGame)
            {
                return;
            }

            if (LucieHero.Living.IsDead)
            {
                return;
            }

            Drawing.DrawString(new Vector2(1920f / 2f, 1080f / 2f - 5f).ScreenToWorld(),
                "Target: " + (KeysMenu.GetKeybind("keys.changeTargeting") ? "LowestHealth" : "NearMouse"), UnityEngine.Color.white);

            if (DrawingsMenu.GetBoolean("draw.healSafeRange"))
            {
                var allyTargets = EntitiesManager.LocalTeam.Where(x => !x.Living.IsDead);

                foreach (var ally in allyTargets)
                {
                    Drawing.DrawCircle(ally.MapObject.Position, HealMenu.GetSlider("heal.allySafeRange"), UnityEngine.Color.green);
                }
            }

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
