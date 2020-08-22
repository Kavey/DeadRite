using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using BattleRight.Core;
using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.Core.GameObjects.Models;
using BattleRight.SDK;
using BattleRight.SDK.Enumeration;
using BattleRight.SDK.UI;
using BattleRight.SDK.UI.Models;
using BattleRight.SDK.UI.Values;
using Kavey_Series.Abilities;
using Kavey_Series.Prediction;
using Kavey_Series.Utilities;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Vector2 = BattleRight.Core.Math.Vector2;

namespace Kavey_Series.Champions
{
    class Shen : IChampion
    {
        public bool Initialized { get; set; }
        public string ChampionName { get; set; } = "Shen";
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
        public Menu Misc { get; set; }
        public Menu AntiGapclosing { get; set; }
        public Menu Drawings { get; set; }

        private static Character HeroPlayer => LocalPlayer.Instance;

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
            Game.OnUpdate += Game_OnUpdate;
            Game.OnDraw += Game_OnDraw;
            MainMenu.GetMenu("kavey_series").Get<Menu>("shen").Hidden = false;
            Utility.Log("[Kavey Series] {0} loaded!", ConsoleColor.DarkCyan, ChampionName);
            Initialized = true;
        }

        public void InitializeMenu()
        {
            var rootMenu = MainMenu.GetMenu("kavey_series");
            Menu = new Menu("shen", "Shen Rao", true);
            //Keys
            {
                Keys = new Menu("shen.keys", "Keys", true);
                Keys.Add(new MenuKeybind("keys.combo", "Combo Key", UnityEngine.KeyCode.Mouse0));
                Keys.Add(new MenuCheckBox("keys.autoCombo", "Auto Combo Mode", true));
                Keys.Add(new MenuKeybind("keys.M1", "Left Mouse keybind to pause Auto Combo", UnityEngine.KeyCode.Mouse2));
                Keys.Add(new MenuKeybind("keys.M2", "Right Mouse keybind to pause Auto Combo", UnityEngine.KeyCode.Mouse1));
                Keys.Add(new MenuKeybind("keys.E", "E keybind to pause Auto Combo", UnityEngine.KeyCode.Alpha2));
                Keys.Add(new MenuKeybind("keys.EX1", "EX1 keybind to pause Auto Combo", UnityEngine.KeyCode.T));
                Keys.Add(new MenuKeybind("keys.EX2", "EX2 keybind to pause Auto Combo", UnityEngine.KeyCode.G));
            }
            //Combo
            {
                Combo = new Menu("shen.combo", "Combo", true);
                // Combo.Add(new MenuSlider("combo.autoSafeRange", "Auto Combo safe range", 2f, 10f, 0f));
                Combo.Add(new MenuCheckBox("combo.invisible", "Attack invisible enemies", true));
                Combo.Add(new MenuCheckBox("combo.useM1", "Use Left Mouse", true));
                Combo.Add(new MenuCheckBox("combo.interruptM1", " ^ Interrupt M1 if needed", false));
                // Combo.Add(new MenuSlider("combo.useM1.safeRange", "    ^ Safe range", 2.5f, 5f, 0f));
                Combo.Add(new MenuCheckBox("combo.useM2", "Use Right Mouse", true));
                Combo.Add(new MenuCheckBox("combo.useSpace", "Use Space", true));
                Combo.Add(new MenuCheckBox("combo.useQ", "Use Q", true));
                Combo.Add(new MenuCheckBox("combo.useE", "Use E", true));
                Combo.Add(new MenuCheckBox("combo.useR", "Use R", true));
                Combo.Add(new MenuCheckBox("combo.useEX2", "Use EX2", true));
                Combo.Add(new MenuIntSlider("combo.useEX2.minEnergyBars", "    ^ Min energy bars", 1, 4, 1));
                Combo.Add(new MenuCheckBox("combo.useF", "Use F", false));
            }
            //Misc
            {
                Misc = new Menu("shen.misc", "Misc", true);
                // Misc.Add(new MenuCheckBox("misc.targetOrb", "Attack the Orb", true));
                Misc.Add(new MenuCheckBox("misc.useM2M1exec", "Use M2 into Left Mouse to execute", true));
                Misc.Add(new MenuCheckBox("misc.useEX2exec", "Use EX2 to execute", true));
            }
            //Drawings
            {
                Drawings = new Menu("shen.drawings", "Drawings", true);
                Drawings.Add(new MenuCheckBox("draw.rangeM1.safeRange", "Draw Left Mouse Range", false));
                Drawings.Add(new MenuCheckBox("draw.Target", "Draw Target", false));
            }
            //Finally
            {
                Menu.Add(Keys);
                Menu.Add(Combo);
                Menu.Add(Misc);
                Menu.Add(Drawings);
                rootMenu.Add(Menu);
            }
        }

        public void InitializeAbilities()
        {
            M1 = new Ability(AbilityKey.M1, 8f, 16f, 0.25f);
            M2 = new Ability(AbilityKey.M2, 7.8f, 23.5f, 0.35f);
            EX1 = new Ability(AbilityKey.EX1, 10f);
            EX2 = new Ability(AbilityKey.EX2, 8.5f);
            Space = new Ability(AbilityKey.Space);
            Q = new Ability(AbilityKey.Q, 2.5f);
            E = new Ability(AbilityKey.E, 7f, 10f, 2f);
            R = new Ability(AbilityKey.R, 3.5f);
            F = new Ability(AbilityKey.F, 13f, 30f, 0.45f);
        }

        //private static bool IsInUltimate
        //{
        //    get
        //    {
        //        var DragonStorm = HeroPlayer.HasBuff("DragonStormBuff");
        //        if (DragonStorm)
        //        {
        //            return true;
        //        }

        //        return false;
        //    }
        //}

        private static bool IsInTheAir
        {
            get
            {
                var Ascension = HeroPlayer.HasBuff("AscensionBuff");
                if (Ascension)
                {
                    return true;
                }

                return false;
            }
        }

        public Shen()
        {
            Initialize();
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

            if (Drawings.GetBoolean("draw.rangeM1.safeRange"))
            {
                Drawing.DrawCircle(Utility.MyPos, M1.Range, Color.yellow);
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

            if (Keys.GetBoolean("keys.autoCombo") && (Keys.GetKeybind("keys.M1") || Keys.GetKeybind("keys.M2") || Keys.GetKeybind("keys.E") ||
                                                                    Keys.GetKeybind("keys.EX1") || Keys.GetKeybind("keys.EX2")))
            {
                LocalPlayer.EditAimPosition = false;
                CastingAbility = null;
                return;
            }

            if (Keys.GetKeybind("keys.combo") || Keys.GetBoolean("keys.autoCombo"))
            {
                var M2M1Damage = 26;
                var EX2Damage = 15;

                var SpaceHeal = 14;
                var RHeal = 24;

                var alliesToTargetBase =
               EntitiesManager.LocalTeam.Where(x => !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial);

                var alliesToTargetR =
                alliesToTargetBase.Where(x => x.Living.Health <= x.Living.MaxRecoveryHealth - RHeal);

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

                var enemiesToTargetProjs = enemiesToTargetBase.Where(x =>
                    !x.IsCountering && !x.HasConsumeBuff && !x.HasBuff("ElectricShield") &&
                    !x.HasBuff("BarbedHuskBuff") && !x.HasBuff("BulwarkBuff") && !x.HasBuff("DivineShieldBuff") &&
                    !x.HasBuff("GustBuff") && !x.HasBuff("TimeBenderBuff"));

                var enemiesToTargetQ = enemiesToTargetProjs.Where(x => x.Distance(HeroPlayer) <= Q.Range);
                var enemiesToTargetM1Space = enemiesToTargetProjs.Where(x => (x.Distance(HeroPlayer) >= 3.5f));
                var ennemiesToTargetEX2 = enemiesToTargetBase.Where(x => x.HasBuff("StormStruckDebuff"));
                var enemiesToExecuteM2M1 = enemiesToTargetProjs.Where(x => x.Living.Health <= M2M1Damage);
                var enemiesToExcuteEX2 = enemiesToTargetBase.Where(x => x.Living.Health <= EX2Damage);

                var M1Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M1.Range);
                var M1SpaceTarget = TargetSelector.GetTarget(enemiesToTargetM1Space, TargetingMode.NearMouse, M1.Range + 1.5f);
                var M2Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M2.Range);
                var QTarget = TargetSelector.GetTarget(enemiesToTargetQ, TargetingMode.NearMouse, Q.Range);
                var ETarget = TargetSelector.GetTarget(enemiesToTargetBase, TargetingMode.NearMouse, E.Range);
                var FTarget = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, F.Range);
                var EX2Target = TargetSelector.GetTarget(ennemiesToTargetEX2, TargetingMode.NearMouse, EX2.Range);
                var M2M1Execute = TargetSelector.GetTarget(enemiesToExecuteM2M1, TargetingMode.LowestHealth, E.Range);
                var EX2Execute = TargetSelector.GetTarget(enemiesToExcuteEX2, TargetingMode.LowestHealth, EX2.Range);
                var RAllies = TargetSelector.GetTarget(alliesToTargetR, TargetingMode.Closest, R.Range);

                if (!Channeling)
                {
                    LocalPlayer.EditAimPosition = false;
                    CastingAbility = null;
                    if (IsInTheAir && M1SpaceTarget != null && M1.CanCast)
                    {
                        LocalPlayer.PressAbility(M1.Slot, true);
                        CastingAbility = M1;
                        return;
                    }

                    if (FTarget != null && F.CanCast && Combo.GetBoolean("combo.useF"))
                    {
                        LocalPlayer.PressAbility(F.Slot, true);
                        CastingAbility = F;
                        return;
                    }

                    if (M2M1Execute != null && M2.CanCast && Misc.GetBoolean("misc.useM2M1exec"))
                    {
                        LocalPlayer.PressAbility(M2.Slot, true);
                        CastingAbility = M2;
                        return;
                    }

                    if (EX2Execute != null && EX2.CanCast && Misc.GetBoolean("misc.useEX2exec"))
                    {
                        LocalPlayer.PressAbility(EX2.Slot, true);
                        CastingAbility = EX2;
                        return;
                    }

                    if (EX2Target != null && EX2.CanCast && Combo.GetBoolean("combo.useEX2"))
                    {
                        var energyRequired = Combo.GetIntSlider("combo.useEX2.minEnergyBars") * 25;
                        if (energyRequired <= HeroPlayer.Energized.Energy)
                        {
                            LocalPlayer.PressAbility(EX2.Slot, true);
                            CastingAbility = EX2;
                            return;
                        }
                    }

                    if (HeroPlayer.Living.Health <= HeroPlayer.Living.MaxRecoveryHealth - SpaceHeal && Space.CanCast && !R.CanCast && !HeroPlayer.HasBuff("DominionFortify"))
                    {
                        LocalPlayer.PressAbility(Space.Slot, true);
                        CastingAbility = Space;
                        return;
                    }

                    if (QTarget != null && Q.CanCast && Combo.GetBoolean("combo.useQ"))
                    {
                        LocalPlayer.PressAbility(Q.Slot, true);
                        CastingAbility = Q;
                        return;
                    }

                    if (RAllies != null & R.CanCast && Combo.GetBoolean("combo.useR"))
                    {
                        LocalPlayer.PressAbility(R.Slot, true);
                        CastingAbility = R;
                        return;
                    }

                    if (ETarget != null && !M2.CanCastAbility(6) && E.CanCast && Combo.GetBoolean("combo.useE"))
                    {
                        LocalPlayer.PressAbility(E.Slot, true);
                        CastingAbility = E;
                        return;
                    }

                    if (M2Target != null && M2.CanCast && Combo.GetBoolean("combo.useM2"))
                    {
                        LocalPlayer.PressAbility(M2.Slot, true);
                        CastingAbility = M2;
                        return;
                    }

                    if (M1Target != null && M1.CanCast && Combo.GetBoolean("combo.useM1"))
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

                    if (IsInTheAir)
                    {
                        switch (CastingAbility.Key)
                        {
                            case AbilityKey.M1:
                                if (M1SpaceTarget != null)
                                {
                                    var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, M1SpaceTarget, M1.Range + 1.5f, M1.Speed, M1.Radius + 0.85f);
                                    if (pred.CanHit)
                                        LocalPlayer.Aim(pred.CastPosition);
                                }
                                break;
                        }
                    }

                    switch (CastingAbility.Key)
                    {
                        case AbilityKey.F:
                            if (FTarget != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, FTarget, F.Range, F.Speed, F.Radius);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            break;

                        case AbilityKey.EX2:
                            if (EX2Execute != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, EX2Execute, EX2.Range, EX2.Speed, EX2.Radius);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            else if (EX2Target != null)
                            {
                                LocalPlayer.Aim(EX2Target.MapObject.Position);
                            }
                            break;

                        case AbilityKey.EX1:
                            break;

                        case AbilityKey.E:
                            if (ETarget != null)
                            {
                                var pred = TestPrediction.GetPrediction(Utility.MyPos, ETarget, E.Range, E.Speed, E.Radius, 0.6f);
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

                        case AbilityKey.Q:
                            if (QTarget != null)
                            {
                                LocalPlayer.Aim(QTarget.MapObject.Position);
                            }
                            break;

                        case AbilityKey.M2:
                            if (M2M1Execute != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, M2M1Execute, M2.Range, M2.Speed, M2.Radius);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            else if (M2Target != null)
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
            MainMenu.GetMenu("kavey_series").Get<Menu>("shen").Hidden = true;
            Game.OnUpdate -= Game_OnUpdate;
            Game.OnDraw -= Game_OnDraw;
        }
    }
}
