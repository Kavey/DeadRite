using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using BattleRight.Core;
using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.Core.GameObjects.Models;
using BattleRight.Core.Models;
using BattleRight.SDK;
using BattleRight.SDK.Enumeration;
using BattleRight.SDK.UI;
using BattleRight.SDK.UI.Models;
using BattleRight.SDK.UI.Values;
using Kavey_Series.Abilities;
using Kavey_Series.AntiGapclose;
using Kavey_Series.Prediction;
using Kavey_Series.Utilities;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Vector2 = BattleRight.Core.Math.Vector2;
using Ability = Kavey_Series.Abilities.Ability;

namespace Kavey_Series.Champions
{
    class Iva : IChampion
    {
        public bool Initialized { get; set; }
        public string ChampionName { get; set; } = "Iva";
        public Ability M1 { get; set; }
        public Ability M2 { get; set; }
        public Ability EX1 { get; set; }
        public Ability EX2 { get; set; }
        public Ability Space { get; set; }
        public Ability Q { get; set; }
        public Ability E { get; set; }
        public Ability R { get; set; }
        public Ability F { get; set; }

        public Menu Menu { get; set; }
        public Menu Keys { get; set; }
        public Menu Combo { get; set; }
        public Menu AntiGapclosing { get; set; }
        public Menu Misc { get; set; }
        public Menu Drawings { get; set; }

        private static Character HeroPlayer => LocalPlayer.Instance;
        private static bool IsAntiGapclosing;

        public Ability CastingAbility { get; set; }

        private static bool Channeling => Utility.Player.IsChanneling || Utility.Player.AbilitySystem.IsCasting;

        public Ability GetAbilityFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return M1;
                case 2:
                    return M2;
                case 3:
                    return Space;
                case 4:
                case 9:
                    return Q;
                case 5:
                    return E;
                case 7:
                    return R;
                case 8:
                    return F;
                case 1:
                    return EX1;
                case 6:
                    return EX2;
                default:
                    return null;
            }
        }


        public void Initialize()
        {
            if (!Initialized)
            {
                InitializeMenu();
                InitializeAbilities();
            }
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnGapcloser;
            Game.OnUpdate += Game_OnUpdate;
            Game.OnDraw += Game_OnDraw;
            MainMenu.GetMenu("kavey_series").Get<Menu>("iva").Hidden = false;
            Utility.Log("[Kavey Series] {0} loaded!", ConsoleColor.White, ChampionName);
            Initialized = true;
        }

        // InGameObject.OnCreate += OnCreate;
        // InGameObject.OnDestroy += OnDestroy;

        public void InitializeMenu()
        {
            var rootMenu = MainMenu.GetMenu("kavey_series");
            Menu = new Menu("iva", "Iva", true);
            //Keys
            {
                Keys = new Menu("iva.keys", "Keys", true);
                Keys.Add(new MenuKeybind("keys.combo", "Combo Key", UnityEngine.KeyCode.Mouse0));
                Keys.Add(new MenuCheckBox("keys.autoCombo", "Auto Combo Mode", true));
                Keys.Add(new MenuKeybind("keys.M1", "Left Mouse keybind to pause Auto Combo", UnityEngine.KeyCode.Mouse2));
                Keys.Add(new MenuKeybind("keys.M2", "Right Mouse keybind to pause Auto Combo", UnityEngine.KeyCode.Mouse1));
                Keys.Add(new MenuKeybind("keys.Space", "Space keybind to pause Auto Combo", UnityEngine.KeyCode.Space));
                Keys.Add(new MenuKeybind("keys.R", "R keybind to pause Auto Combo", UnityEngine.KeyCode.R));
                Keys.Add(new MenuKeybind("keys.EX1", "EX1 keybind to pause Auto Combo", UnityEngine.KeyCode.T));
            }
            //Combo
            {
                Combo = new Menu("iva.combo", "Combo", true);
                // Combo.Add(new MenuSlider("combo.autoSafeRange", "Auto Combo safe range", 2f, 10f, 0f));
                Combo.Add(new MenuCheckBox("combo.invisible", "Attack invisible enemies", true));
                Combo.Add(new MenuCheckBox("combo.useM1", "Use Left Mouse", true));
                Combo.Add(new MenuCheckBox("combo.interruptM1", " ^ Interrupt Left Mouse if needed", true));
                // Combo.Add(new MenuSlider("combo.useM1.safeRange", "    ^ Safe range", 2.5f, 5f, 0f));
                Combo.Add(new MenuCheckBox("combo.useM2", "Use Right Mouse", true));
                Combo.Add(new MenuCheckBox("combo.useQ", "Use Q on allies to dispel", false));
                Combo.Add(new MenuCheckBox("combo.useE", "Use E", true));
                Combo.Add(new MenuCheckBox("combo.interruptE", " ^ Interrupt E if needed", true));
                Combo.Add(new MenuCheckBox("combo.useEX1", "Use EX1", true));
                Combo.Add(new MenuCheckBox("combo.oiledEX1", " ^ Only Oiled enemies", true));
                Combo.Add(new MenuIntSlider("combo.useEX1.minEnergyBars", "    ^ Min energy bars", 2, 4, 2));
                Combo.Add(new MenuCheckBox("combo.useEX2", "Use EX2", true));
                Combo.Add(new MenuIntSlider("combo.useEX2.minEnergyBars", "    ^ Min energy bars", 3, 4, 1));
                Combo.Add(new MenuCheckBox("combo.useF", "Use F", true));
            }
            //Anti-Gapclosing
            {
                AntiGapclosing = new Menu("iva.gapclosing", "Anti-Gapclosing", true);
                AntiGapclosing.Add(new MenuCheckBox("gapclosing.R", "Block with R", true));
                AntiGapclosing.Add(new MenuIntSlider("gapclosing.R.energy", "    ^ Min energy bars", 1, 4, 1));
            }
            //Misc
            {
                Misc = new Menu("iva.misc", "Misc", true);
                // Misc.Add(new MenuCheckBox("misc.targetOrb", "Attack the Orb", true));
                Misc.Add(new MenuCheckBox("misc.useQ.hyperspeed", "Use Q on self with Hyperspeed", true));
                Misc.Add(new MenuCheckBox("misc.useEX1exec", "Use EX1 to execute", true));
                Misc.Add(new MenuCheckBox("misc.useEX2exec", "Use EX2 to execute", true));
            }
            //Drawings
            {
                Drawings = new Menu("iva.drawings", "Drawings", true);
                Drawings.Add(new MenuCheckBox("draw.rangeM1.safeRange", "Draw Left Mouse Range", false));
                Drawings.Add(new MenuCheckBox("draw.Target", "Draw Target", false));
            }
            //Finally
            {
                Menu.Add(Keys);
                Menu.Add(Combo);
                Menu.Add(AntiGapclosing);
                Menu.Add(Misc);
                Menu.Add(Drawings);
                rootMenu.Add(Menu);
            }
        }

        public void InitializeAbilities()
        {
            M1 = new Ability(AbilityKey.M1, 6.2f, 20f, 0.18f);
            M2 = new Ability(AbilityKey.M2, 10.25f, 22f, 0.35f);
            EX1 = new Ability(AbilityKey.EX1, 5f);
            EX2 = new Ability(AbilityKey.EX2, 10f, 32f, 0.35f);
            Space = new Ability(AbilityKey.Space);
            Q = new Ability(AbilityKey.Q, 20f);
            E = new Ability(AbilityKey.E, 9.6f, 30f, 0.3f);
            R = new Ability(AbilityKey.R, 8.5f);
            F = new Ability(AbilityKey.F, 12.6f, 25f, 0.3f);
        }

        private static bool IsInUltimate
        {
            get
            {
                var MachineGun = HeroPlayer.HasBuff("MachineGunChannel");
                if (MachineGun)
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
                var JetPack = HeroPlayer.HasBuff("JetPack");
                if (JetPack)
                {
                    return true;
                }

                return false;
            }
        }

        private static bool IsInBeam
        {
            get
            {
                var JetPack = HeroPlayer.HasBuff("TractorBeam");
                if (JetPack)
                {
                    return true;
                }

                return false;
            }
        }

        private static bool HasHyperspeed
        {
            get
            {
                List<Battlerite> Battlerites = new List<Battlerite>(5);
                if (Battlerites.Any())
                {
                    Battlerites.Clear();
                }

                for (var i = 0; i < 5; i++)
                {
                    var br = HeroPlayer.BattleriteSystem.GetEquippedBattlerite(i);
                    if (br != null)
                    {
                        Battlerites.Add(br);
                    }
                }

                var HyperspeedUpgrade = Battlerites.Any(x => x.Name.Equals("HyperspeedUpgrade"));
                if (HyperspeedUpgrade)
                {
                    return true;
                }

                return false;
            }
        }

        public Iva()
        {
            Initialize();
        }

        // private static void OnCreate(InGameObject inGameObject)
        // {
        //     Console.WriteLine(inGameObject.ObjectName + " of type " + inGameObject.GetType() + " created");
        //
        // }
        //
        // private static void OnDestroy(InGameObject inGameObject)
        // {
        //     Console.WriteLine(inGameObject.ObjectName + " of type " + inGameObject.GetType() + " destroyed");
        // }

        private void AntiGapcloser_OnGapcloser(AntiGapcloser.GapcloseEventArgs args)
        {
            if (args.WillHit || args.EndPosition.Distance(Utility.Player) <= R.Range)
            {
                var energyRequired = AntiGapclosing.GetIntSlider("gapclosing.R.energy") * 25;
                if (R.CanCast && AntiGapclosing.GetBoolean("gapclosing.R") && energyRequired <= HeroPlayer.Energized.Energy)
                {
                    IsAntiGapclosing = true;
                    LocalPlayer.EditAimPosition = true;
                    LocalPlayer.PressAbility(R.Slot, true);
                    LocalPlayer.Aim(args.StartPosition);
                    Utility.DelayAction(() =>
                    {
                        IsAntiGapclosing = false;
                        LocalPlayer.EditAimPosition = false;
                    }, 0.25f);
                }
            }
        }

        private void Game_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame)
            {
                return;
            }

            if (HeroPlayer.Living.IsDead)
            {
                return;
            }

            if (!Game.IsInRoundPhase)
            {
                Drawing.DrawString(new Vector2(1920f / 2f, (1080f / 2f) - 5f).ScreenToWorld(),
                    "don't feed :@", Color.white);
            }

            if (Drawings.GetBoolean("draw.rangeM1.safeRange"))
            {

                Drawing.DrawCircle(Utility.MyPos, M1.Range, UnityEngine.Color.yellow);
            }

            if (Drawings.GetBoolean("draw.Target"))
            {
                Drawing.DrawCircle(LocalPlayer.AimPosition, 1f, Color.red);
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInRoundPhase)
            {
                return;
            }

            if (IsInTheAir || Keys.GetBoolean("keys.autoCombo") && (Keys.GetKeybind("keys.M1") || Keys.GetKeybind("keys.M2") || Keys.GetKeybind("keys.Space") ||
                                                                    Keys.GetKeybind("keys.R") || Keys.GetKeybind("keys.EX1")))
            {
                LocalPlayer.EditAimPosition = false;
                CastingAbility = null;
                return;
            }

            // Orbs object name:
            //RiteOfMap6Object / RiteOfMap7Object / RiteOfBonesObject...
            //
            // var orb = EntitiesManager.CenterOrb;
            // if (orb == null)
            // {
            //     Console.WriteLine("Orb status:" + orb.ObjectName);
            //     var orbHealth = orb.Get<LivingObject>().Health;
            //     var orbPos = orb.Get<MapGameObject>().Position;
            //
            //     Drawing.DrawCircle(orbPos, 1.5f, Color.green);
            //
            //     if (orbHealth <= 0)
            //     {
            //         return;
            //     }
            //
            //     LocalPlayer.EditAimPosition = true;
            //     LocalPlayer.Aim(orbPos);
            //
            //     if (HeroPlayer.Distance(orbPos) <= M2.Range)
            //     {
            //         if (M2.CanCast)
            //         {
            //             LocalPlayer.PressAbility(M2.Slot, true);
            //             CastingAbility = M2;
            //             return;
            //         }
            //     }
            //
            //     if (HeroPlayer.Distance(orbPos) <= M1.Range)
            //     {
            //         if (M1.CanCast)
            //         {
            //             LocalPlayer.PressAbility(M1.Slot, true);
            //             CastingAbility = M1;
            //         }
            //     }
            //     return;
            // }

            if (IsAntiGapclosing)
                return;
            if (Keys.GetKeybind("keys.combo") || Keys.GetBoolean("keys.autoCombo"))
            {
                var EX1Damage = 35;
                var EX2Damage = 10;

                var enemiesToTargetBase = EntitiesManager.EnemyTeam.Where(x => x.IsValid && !x.Living.IsDead && !x.IsTraveling && !x.IsDashing &&
                   !x.HasBuff("OtherSideBuff") && !x.HasBuff("AscensionBuff") && !x.HasBuff("AscensionTravelBuff") && !x.HasBuff("Fleetfoot") &&
                   !x.HasBuff("ValiantLeap") && !x.HasBuff("FrogLeap") && !x.HasBuff("FrogLeapRecast") && !x.HasBuff("ElusiveStrikeCharged") &&
                   !x.HasBuff("ElusiveStrikeWall2") && !x.HasBuff("BurrowAlternate") && !x.HasBuff("JetPack") && !x.HasBuff("ProwlBuff") &&
                   !x.HasBuff("Dive") && !x.HasBuff("InfestingBuff") && !x.HasBuff("PortalBuff") && !x.HasBuff("CrushingBlow") && !x.HasBuff("TempestRushBuff") &&
                   !x.HasBuff("TornadoBuff") && !x.HasBuff("LawBringerInAir") && !x.HasBuff("LawBringerLeap") && !x.HasBuff("JawsSwallowedDebuff"));

                if (!Combo.GetBoolean("combo.invisible"))
                {
                    enemiesToTargetBase = enemiesToTargetBase.Where(x => !x.CharacterModel.IsModelInvisible);
                }

                var alliesToTargetBaseQ = EntitiesManager.LocalTeam.Where(x => !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial && !x.AbilitySystem.IsCasting);

                var enemiesToTargetProjs = enemiesToTargetBase.Where(x =>
                    !x.IsCountering && !x.HasConsumeBuff && !x.HasBuff("ElectricShield") &&
                    !x.HasBuff("BarbedHuskBuff") && !x.HasBuff("BulwarkBuff") && !x.HasBuff("DivineShieldBuff") &&
                    !x.HasBuff("GustBuff") && !x.HasBuff("TimeBenderBuff"));

                var enemiesToTargetE = enemiesToTargetProjs.Where(x => x.AbilitySystem.IsCasting || x.IsChanneling);
                var enemiesToExecuteEX1 = enemiesToTargetBase.Where(x => x.Living.Health <= EX1Damage);
                var enemiesToExcuteEX2 = enemiesToTargetProjs.Where(x => x.Living.Health <= EX2Damage);

                // Dispelling Debuff (allies):
                //
                var alliesToTargetQ = alliesToTargetBaseQ.Where(x => // x.Get<AgeObject>().Age > 0.75f &&
                // x.HasBuff("Weaken") || !x.IsLocalPlayer && x.HasBuff("Stun") || 
                !x.IsLocalPlayer && x.HasBuff("LunarStrikePetrify") || !x.IsLocalPlayer && x.HasBuff("Panic") ||
                !x.IsLocalPlayer && (x.HasBuff("Incapacitate") || x.HasBuff("DeadlyInjectionBuff")) || !x.IsLocalPlayer && x.HasBuff("Fear") || !x.IsLocalPlayer && x.HasBuff("Petrify") ||
                x.HasBuff("PhantomCutBuff") || x.HasBuff("EntanglingRootsBuff") || // x.HasBuff("FrostDebuff") ||
                !x.IsLocalPlayer && x.HasBuff("Frozen") || !x.IsLocalPlayer && x.HasBuff("StormStruckDebuff") || !x.IsLocalPlayer && x.HasBuff("BrainBugDebuff") || x.HasBuff("HandOfJudgementBuff") ||
                // x.HasBuff("HandOfCorruptionBuff) ||
                !x.IsLocalPlayer && x.HasBuff("SheepTrickDebuff") || x.HasBuff("SludgeSpitDebuff") || x.HasBuff("BlindingLightBlind"));
                //

                // var targetSelf = TargetSelector.GetTarget(alliesSelf, targetMode, M1Range);
                var M1Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M1.Range);
                var M2Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M2.Range);
                var QAllies = TargetSelector.GetTarget(alliesToTargetQ, TargetingMode.NearMouse, Q.Range);
                var ETarget = TargetSelector.GetTarget(enemiesToTargetE, TargetingMode.NearMouse, E.Range);
                var RTarget = TargetSelector.GetTarget(enemiesToTargetBase, TargetingMode.NearMouse, R.Range);
                var FTarget = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, F.Range);
                var EX1Target = TargetSelector.GetTarget(enemiesToTargetBase, TargetingMode.NearMouse, EX1.Range);
                var EX2Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, EX2.Range);
                var EX1Execute = TargetSelector.GetTarget(enemiesToExecuteEX1, TargetingMode.NearMouse, EX1.Range);
                var EX2Execute = TargetSelector.GetTarget(enemiesToExcuteEX2, TargetingMode.NearMouse, EX2.Range);

                if (!Channeling)
                {
                    LocalPlayer.EditAimPosition = false;
                    CastingAbility = null;
                    if (IsInUltimate)
                    {
                        return;
                    }

                    if (M1Target != null && Q.CanCast && HasHyperspeed && Misc.GetBoolean("misc.useQ.hyperspeed"))
                    {
                        LocalPlayer.Aim(HeroPlayer.MapObject.Position);
                        LocalPlayer.PressAbility(Q.Slot, true);
                        CastingAbility = Q;
                        return;
                    }

                    if (QAllies != null && Q.CanCast && Combo.GetBoolean("combo.useQ"))
                    {
                        {
                            LocalPlayer.PressAbility(Q.Slot, true);
                            CastingAbility = Q;
                            return;
                        }
                    }

                    if (EX2Execute != null && EX2Execute.Distance(HeroPlayer) > M1.Range && EX2.CanCast && Misc.GetBoolean("misc.useEX2exec"))
                    {
                        LocalPlayer.PressAbility(EX2.Slot, true);
                        CastingAbility = EX2;
                        return;
                    }

                    if (EX1Execute != null && EX1.CanCast && Misc.GetBoolean("misc.useEX1exec"))
                    {
                        LocalPlayer.PressAbility(EX1.Slot, true);
                        CastingAbility = EX1;
                        return;
                    }

                    if (EX1Target != null && EX1.CanCast && Combo.GetBoolean("combo.useEX1"))
                    {
                        if (Combo.GetBoolean("combo.oiledEX1") && EX1Target.HasBuff("OilDebuff"))
                        {
                            var energyRequired = Combo.GetIntSlider("combo.useEX1.minEnergyBars") * 25;
                            if (energyRequired <= HeroPlayer.Energized.Energy)
                            {
                                LocalPlayer.PressAbility(EX1.Slot, true);
                                CastingAbility = EX1;
                                return;
                            }
                        }
                        else if (!Combo.GetBoolean("combo.oiledEX1"))
                        {
                            var energyRequired = Combo.GetIntSlider("combo.useEX1.minEnergyBars") * 25;
                            if (energyRequired <= HeroPlayer.Energized.Energy)
                            {
                                LocalPlayer.PressAbility(EX1.Slot, true);
                                CastingAbility = EX1;
                                return;
                            }
                        }
                    }

                    if (ETarget != null && E.CanCast && Combo.GetBoolean("combo.useE"))
                    {
                        LocalPlayer.PressAbility(E.Slot, true);
                        CastingAbility = E;
                        return;
                    }

                    if (EX2Target != null & EX2.CanCast && !F.CanCast && Combo.GetBoolean("combo.useEX2"))
                    {
                        var energyRequired = Combo.GetIntSlider("combo.useEX2.minEnergyBars") * 25;
                        if (energyRequired <= HeroPlayer.Energized.Energy)
                        {
                            LocalPlayer.PressAbility(EX2.Slot, true);
                            CastingAbility = EX2;
                            return;
                        }
                    }

                    if (FTarget != null && FTarget.Distance(HeroPlayer) > M1.Range && !HeroPlayer.HasBuff("UTurnRecastBuff") && F.CanCast && !IsInUltimate && Combo.GetBoolean("combo.useF"))
                    {
                        LocalPlayer.PressAbility(F.Slot, true);
                        CastingAbility = F;
                        return;
                    }

                    if (M2Target != null && M2Target.HasBuff("OilDebuff") && !HeroPlayer.HasBuff("Zap") && !HeroPlayer.HasBuff("BoomstickRapidFireRecharge") && M2.CanCast && !IsInUltimate && Combo.GetBoolean("combo.useM2"))
                    {
                        LocalPlayer.PressAbility(M2.Slot, true);
                        CastingAbility = M2;
                        return;
                    }

                    if (M2Target != null && M2Target.Distance(HeroPlayer) >= M1.Range && !HeroPlayer.HasBuff("Zap") && !HeroPlayer.HasBuff("BoomstickRapidFireRecharge") && M2.CanCast && !IsInUltimate && Combo.GetBoolean("combo.useM2"))
                    {
                        LocalPlayer.PressAbility(M2.Slot, true);
                        CastingAbility = M2;
                        return;
                    }

                    if (M1Target != null && !M1Target.HasBuff("Incapacitate") && M1.CanCast && Combo.GetBoolean("combo.useM1"))
                    {
                        LocalPlayer.PressAbility(M1.Slot, true);
                        CastingAbility = M1;
                    }
                }
                else
                {
                    if (CastingAbility == null)
                        CastingAbility = GetAbilityFromIndex(Utility.Player.AbilitySystem.CastingAbilityIndex);
                    if (CastingAbility == null)
                        return;

                    if (IsInUltimate)
                    {
                        switch (CastingAbility.Key)
                        {
                            case AbilityKey.F:
                                if (FTarget != null)
                                {
                                    var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, FTarget, F.Range, F.Speed, F.Radius, true);
                                    if (pred.CanHit)
                                        LocalPlayer.Aim(pred.CastPosition);
                                }
                                break;
                        }
                    }

                    if (IsInBeam)
                    {
                        switch (CastingAbility.Key)
                        {
                            case AbilityKey.R:
                                if (RTarget != null)
                                {
                                    LocalPlayer.Aim(RTarget.MapObject.Position);
                                }
                                break;
                        }
                    }

                    switch (CastingAbility.Key)
                    {
                        case AbilityKey.EX2:
                            if (EX2Execute != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, EX2Execute, EX2.Range, EX2.Speed, EX2.Radius, true);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            else if (EX2Target != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, EX2Target, EX2.Range, EX2.Speed, EX2.Radius, true);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            else
                            {
                                LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            break;

                        case AbilityKey.EX1:
                            if (EX1Execute != null)
                            {
                                LocalPlayer.Aim(EX1Execute.MapObject.Position);
                            }
                            else if (EX1Target != null)
                            {
                                LocalPlayer.Aim(EX1Target.MapObject.Position);
                            }
                            break;

                        case AbilityKey.E:
                            if (ETarget != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, ETarget, E.Range, E.Speed, E.Radius, true);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else if (Combo.GetBoolean("combo.interruptE"))
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            else if (Combo.GetBoolean("combo.interruptE"))
                            {
                                LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            break;

                        case AbilityKey.Q:
                            if (M1Target != null)
                            {
                                LocalPlayer.Aim(HeroPlayer.MapObject.Position);
                            }
                            else if (QAllies != null)
                            {
                                LocalPlayer.Aim(QAllies.MapObject.Position);
                            }
                            else
                            {
                                LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            break;

                        case AbilityKey.M2:
                            if (M2Target != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, M2Target, M2.Range, M2.Speed, M2.Radius, true);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            else
                            {
                                LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            break;

                        case AbilityKey.M1:
                            if (M1Target != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, M1Target, M1.Range, M1.Speed, M1.Radius, true);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else if (Combo.GetBoolean("combo.interruptM1"))
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            else if (Combo.GetBoolean("combo.interruptM1"))
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
                CastingAbility = null;
            }
        }

        public void Destroy()
        {
            MainMenu.GetMenu("kavey_series").Get<Menu>("iva").Hidden = true;
            Game.OnUpdate -= Game_OnUpdate;
            Game.OnDraw -= Game_OnDraw;
            AntiGapcloser.OnGapcloser -= AntiGapcloser_OnGapcloser;
        }

        // InGameObject.OnCreate -= OnCreate;
        // InGameObject.OnDestroy -= OnDestroy;
    }
}
