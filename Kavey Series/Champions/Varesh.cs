using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
// using System.Text;
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
    class Varesh : IChampion
    {
        public bool Initialized { get; set; }
        public string ChampionName { get; set; } = "Varesh";
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
            MainMenu.GetMenu("kavey_series").Get<Menu>("varesh").Hidden = false;
            Utility.Log("[Kavey Series] {0} loaded!", ConsoleColor.DarkBlue, ChampionName);
            Initialized = true;
        }

        public void InitializeMenu()
        {
            var rootMenu = MainMenu.GetMenu("kavey_series");
            Menu = new Menu("varesh", "Varesh", true);
            //Keys
            {
                Keys = new Menu("varesh.keys", "Keys", true);
                Keys.Add(new MenuKeybind("keys.combo", "Combo Key", UnityEngine.KeyCode.Mouse0));
                Keys.Add(new MenuCheckBox("keys.autoCombo", "Auto Combo Mode", true));
                Keys.Add(new MenuKeybind("keys.M1", "Left Mouse keybind to pause Auto Combo", UnityEngine.KeyCode.Mouse2));
                Keys.Add(new MenuKeybind("keys.M2", "Right Mouse keybind to pause Auto Combo", UnityEngine.KeyCode.Alpha1));
                Keys.Add(new MenuKeybind("keys.R", "R keybind to pause Auto Combo", UnityEngine.KeyCode.R));
                Keys.Add(new MenuKeybind("keys.EX1", "EX1 keybind to pause Auto Combo", UnityEngine.KeyCode.T));
                Keys.Add(new MenuKeybind("keys.EX2", "EX2 keybind to pause Auto Combo", UnityEngine.KeyCode.G));
            }
            //Combo
            {
                Combo = new Menu("varesh.combo", "Combo", true);
                // Combo.Add(new MenuSlider("combo.autoSafeRange", "Auto Combo safe range", 2f, 10f, 0f));
                Combo.Add(new MenuCheckBox("combo.invisible", "Attack invisible enemies", true));
                Combo.Add(new MenuCheckBox("combo.useM1", "Use Left Mouse", true));
                Combo.Add(new MenuCheckBox("combo.interruptM1", " ^ Interrupt M1 if needed", false));
                // Combo.Add(new MenuSlider("combo.useM1.safeRange", "    ^ Safe range", 2.5f, 5f, 0f));
                Combo.Add(new MenuCheckBox("combo.useM2", "Use Right Mouse", true));
                Combo.Add(new MenuCheckBox("combo.useSpace", "Use Space", true));
                Combo.Add(new MenuCheckBox("combo.useE", "Use E", true));
                Combo.Add(new MenuCheckBox("combo.useEX1", "Use EX1", true));
                Combo.Add(new MenuIntSlider("combo.useEX1.minEnergyBars", "    ^ Min energy bars", 2, 4, 1));
                Combo.Add(new MenuCheckBox("combo.useEX2", "Use EX2", false));
                Combo.Add(new MenuIntSlider("combo.useEX2.minEnergyBars", "    ^ Min energy bars", 3, 4, 1));
                Combo.Add(new MenuCheckBox("combo.useF", "Use F", true));
            }
            //Misc
            {
                Misc = new Menu("varesh.misc", "Misc", true);
                // Misc.Add(new MenuCheckBox("misc.targetOrb", "Attack the Orb", true));
                Misc.Add(new MenuCheckBox("misc.useQ", "Try to counter melees with Q", true));
                Misc.Add(new MenuCheckBox("misc.useEX1exec", "Use EX1 to execute", true));
                Misc.Add(new MenuCheckBox("misc.useEX2exec", "Use EX2 to execute", true));

            }
            //Drawings
            {
                Drawings = new Menu("varesh.drawings", "Drawings", true);
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
            M1 = new Ability(AbilityKey.M1, 7f, 20f, 0.25f);
            M2 = new Ability(AbilityKey.M2, 9.5f, 23f, 0.3f);
            EX1 = new Ability(AbilityKey.EX1, 8.3f, 23f, 0.35f);
            EX2 = new Ability(AbilityKey.EX2, 8f, 10f, 1.5f, SkillType.Circle);
            Space = new Ability(AbilityKey.Space, 10f);
            Q = new Ability(AbilityKey.Q, 10f);
            E = new Ability(AbilityKey.E, 8f, 10f, 1.5f, SkillType.Circle);
            R = new Ability(AbilityKey.R, 7.5f, 16f, 0.6f);
            F = new Ability(AbilityKey.F, 6.5f, 17f, 0.6f);
        }

        private static bool IsInUltimate
        {
            get
            {
                var PowersCombined = HeroPlayer.HasBuff("PowersCombinedFly1") || HeroPlayer.HasBuff("PowersCombinedFly2");
                if (PowersCombined)
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
                var KineticEnergyBuff = HeroPlayer.HasBuff("KineticEnergyTravel");
                if (KineticEnergyBuff)
                {
                    return true;
                }

                return false;
            }
        }

        private static bool IsInWuju
        {
            get
            {
                var WujuBuff = HeroPlayer.HasBuff("WujuReformFly");
                if (WujuBuff)
                {
                    return true;
                }

                return false;
            }
        }

        public Varesh()
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

                Drawing.DrawCircle(Utility.MyPos, M1.Range, UnityEngine.Color.yellow);
            }

            if (Drawings.GetBoolean("draw.Target"))
            {
                Drawing.DrawCircle(LocalPlayer.AimPosition, 1f, Color.red);
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInRoundPhase || HeroPlayer.HasBuff("WujuReformTrance"))
            {
                return;
            }

            if (Keys.GetBoolean("keys.autoCombo") && (Keys.GetKeybind("keys.M1") || Keys.GetKeybind("keys.M2") ||
                                        Keys.GetKeybind("keys.R") || Keys.GetKeybind("keys.EX1") || Keys.GetKeybind("keys.EX2")))
            {
                LocalPlayer.EditAimPosition = false;
                CastingAbility = null;
                return;
            }

            if (Keys.GetKeybind("keys.combo") || Keys.GetBoolean("keys.autoCombo"))
            {
                var EX1Damage = 16;
                var EX2Damage = 12;

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

                var MeleeRange = 2.5f;

                var enemiesToTargetSpace = enemiesToTargetBase.Where(x => x.HasBuff("HandOfCorruptionBuff"));
                var alliesToTarget = EntitiesManager.LocalTeam.Where(x => !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial);
                var alliesSpaceTarget = TargetSelector.GetTarget(alliesToTarget, TargetingMode.NearMouse, Space.Range);

                var enemiesToTargetSpaceA = enemiesToTargetSpace.Where(x => x.Distance(alliesSpaceTarget) <= MeleeRange + 0.5f);

                var enemiesToExecuteEX1 = enemiesToTargetProjs.Where(x => x.Living.Health <= EX1Damage);
                var enemiesToExcuteEX2 = enemiesToTargetBase.Where(x => x.Living.Health <= EX2Damage);

                var ennemiesToTargetE = enemiesToTargetBase.Where(x => x.HasBuff("HandOfJudgementBuff") || x.HasBuff("HandOfCorruptionBuff"));
                var ennemiesToTargetEX1 = enemiesToTargetProjs.Where(x => !x.HasBuff("HandOfJudgementBuff") && !x.HasBuff("HandOfCorruptionBuff"));

                var selfToTargetQ = enemiesToTargetBase.Where(x => (x.ChampionEnum == Champion.Bakko || x.ChampionEnum == Champion.Croak ||
                                                                            x.ChampionEnum == Champion.Freya || x.ChampionEnum == Champion.Jamila ||
                                                                            x.ChampionEnum == Champion.Raigon || x.ChampionEnum == Champion.Rook ||
                                                                            x.ChampionEnum == Champion.RuhKaan || x.ChampionEnum == Champion.Shifu ||
                                                                            x.ChampionEnum == Champion.Thorn)
                                                   && x.Distance(HeroPlayer) <= MeleeRange && x.AbilitySystem.IsCasting);

                var M1Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M1.Range);
                var M2Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M2.Range);
                var SpaceTarget = TargetSelector.GetTarget(enemiesToTargetSpaceA, TargetingMode.NearMouse, Space.Range);
                var QSelf = TargetSelector.GetTarget(selfToTargetQ, TargetingMode.NearMouse, Q.Range);
                var QTarget = TargetSelector.GetTarget(ennemiesToTargetE, TargetingMode.NearMouse, Q.Range);
                var ETarget = TargetSelector.GetTarget(ennemiesToTargetE, TargetingMode.NearMouse, E.Range);
                var RTarget = TargetSelector.GetTarget(ennemiesToTargetE, TargetingMode.NearMouse, R.Range);
                var FTarget = TargetSelector.GetTarget(enemiesToTargetBase, TargetingMode.NearMouse, F.Range);
                var EX1Target = TargetSelector.GetTarget(ennemiesToTargetEX1, TargetingMode.NearMouse, EX1.Range);
                var EX2Target = TargetSelector.GetTarget(ennemiesToTargetE, TargetingMode.NearMouse, EX2.Range);
                var EX1Execute = TargetSelector.GetTarget(enemiesToExecuteEX1, TargetingMode.NearMouse, EX1.Range);
                var EX2Execute = TargetSelector.GetTarget(enemiesToExcuteEX2, TargetingMode.NearMouse, (EX2.Range));

                if (!Channeling)
                {
                    LocalPlayer.EditAimPosition = false;
                    CastingAbility = null;

                    if (QTarget != null && IsInWuju)
                    {
                       LocalPlayer.Aim(QTarget.MapObject.Position);
                       return;
                    }

                    if (RTarget != null && IsInTheAir)
                    {
                        var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, RTarget, R.Range, R.Speed,
                            R.Radius);
                        if (pred.CanHit)
                            LocalPlayer.Aim(pred.CastPosition);
                        return;
                    }

                    if (FTarget != null && IsInUltimate)
                    {
                        LocalPlayer.Aim(FTarget.MapObject.Position);
                        return;
                    }

                    if (QSelf != null && Q.CanCast && Misc.GetBoolean("misc.useQ"))
                    {
                        LocalPlayer.PressAbility(Q.Slot, true);
                        CastingAbility = Q;
                        return;
                    }

                    if (FTarget != null && F.CanCast && !Space.CanCast && !Q.CanCast && !IsInUltimate && Combo.GetBoolean("combo.useF"))
                    {
                        LocalPlayer.PressAbility(F.Slot, true);
                        CastingAbility = F;
                        return;
                    }

                    if (SpaceTarget != null && Space.CanCast)
                    {
                        LocalPlayer.PressAbility(Space.Slot, true);
                        CastingAbility = Space;
                        return;
                    }

                    if (EX1Execute != null && EX1.CanCast && Misc.GetBoolean("misc.useEX1exec"))
                    {
                        LocalPlayer.PressAbility(EX1.Slot, true);
                        CastingAbility = EX1;
                        return;
                    }

                    if (EX2Execute != null & EX2.CanCastCharges && Misc.GetBoolean("misc.useEX2exec"))
                    {
                        LocalPlayer.PressAbility(EX2.Slot, true);
                        CastingAbility = EX2;
                        return;
                    }

                    if (EX1Target != null & EX1.CanCast && Combo.GetBoolean("combo.useEX1"))
                    {
                        var energyRequired = Combo.GetIntSlider("combo.useEX1.minEnergyBars") * 25;
                        if (energyRequired <= HeroPlayer.Energized.Energy)
                        {
                            LocalPlayer.PressAbility(EX1.Slot, true);
                            CastingAbility = EX1;
                            return;
                        }
                    }

                    if (EX2Target != null & EX2.CanCastCharges && !Space.CanCast && Combo.GetBoolean("combo.useEX2"))
                    {
                        var energyRequired = Combo.GetIntSlider("combo.useEX2haste.minEnergyBars") * 25;
                        if (energyRequired <= HeroPlayer.Energized.Energy)
                        {
                            LocalPlayer.PressAbility(EX2.Slot, true);
                            CastingAbility = EX2;
                            return;
                        }
                    }

                    if (ETarget != null && E.CanCastCharges && !Space.CanCast && Combo.GetBoolean("combo.useE"))
                    {
                        LocalPlayer.PressAbility(E.Slot, true);
                        CastingAbility = E;
                        return;
                    }

                    if (M2Target != null && !M2Target.HasBuff("HandOfJudgementBuff") && M2.CanCast && Combo.GetBoolean("combo.useM2"))
                    {
                        var safeRange = M1.Range + 1f;
                        if (HeroPlayer.EnemiesAroundAlive(safeRange) < 1)
                        {
                            LocalPlayer.PressAbility(M2.Slot, true);
                            CastingAbility = M2;
                            return;

                        }
                    }

                    if (M1Target != null && M1.CanCast && Combo.GetBoolean("combo.useM1"))
                    {
                        LocalPlayer.PressAbility(M1.Slot, true);
                        CastingAbility = M1;
                        return;
                    }
                }
                else
                {
                    if (CastingAbility == null)
                        CastingAbility = GetAbilityFromIndex(Utility.Player.AbilitySystem.CastingAbilityIndex);
                    if (CastingAbility == null)
                        return;

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
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, EX1Execute, EX1.Range, EX1.Speed, EX1.Radius, true);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            else if (EX1Target != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, EX1Target, EX1.Range, EX1.Speed, EX1.Radius, true);
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

                        case AbilityKey.E:
                            if (ETarget != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, ETarget, E.Range, E.Speed, E.Radius, true);
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

                        case AbilityKey.Space:
                            if (SpaceTarget != null)
                            {
                                LocalPlayer.Aim(SpaceTarget.MapObject.Position);
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
            MainMenu.GetMenu("kavey_series").Get<Menu>("varesh").Hidden = true;
            Game.OnUpdate -= Game_OnUpdate;
            Game.OnDraw -= Game_OnDraw;
        }
    }
}
