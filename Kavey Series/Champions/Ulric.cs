using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.NetworkInformation;
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
    internal class Ulric : IChampion
    {
        public bool Initialized { get; set; }
        public string ChampionName { get; set; } = "Ulric";
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
        public Menu Healing { get; set; }
        public Menu AntiGapclosing { get; set; }
        public Menu Misc { get; set; }
        public Menu Drawings { get; set; }

        private static Character HeroPlayer => LocalPlayer.Instance;
        // private static bool IsAntiGapclosing;

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

            // AntiGapcloser.OnGapcloser += AntiGapcloser_OnGapcloser;
            Game.OnUpdate += Game_OnUpdate;
            Game.OnDraw += Game_OnDraw;
            MainMenu.GetMenu("kavey_series").Get<Menu>("ulric").Hidden = false;
            Utility.Log("[Kavey Series] {0} loaded!", ConsoleColor.Green, ChampionName);
            Initialized = true;
        }

        public void InitializeMenu()
        {
            var rootMenu = MainMenu.GetMenu("kavey_series");
            Menu = new Menu("ulric", "Ulric", true);
            //Keys
            {
                Keys = new Menu("ulric.keys", "Keys", true);
                Keys.Add(new MenuKeybind("keys.combo", "Combo Key", KeyCode.Y));
                Keys.Add(new MenuKeybind("keys.healSelf", "Heal self", KeyCode.LeftControl));
                Keys.Add(new MenuCheckBox("keys.autoCombo", "Auto Combo Mode", true));
                Keys.Add(new MenuKeybind("keys.M1", "Left Mouse keybind to pause Auto Combo", KeyCode.Mouse2));
                Keys.Add(new MenuKeybind("keys.M2", "Right Mouse keybind to pause Auto Combo", KeyCode.Mouse1));
                Keys.Add(new MenuKeybind("keys.Space", "Space keybind to pause Auto Combo", KeyCode.Space));
                Keys.Add(new MenuKeybind("keys.EX1", "EX1 keybind to pause Auto Combo", KeyCode.T));
            }
            //Combo
            {
                Combo = new Menu("ulric.combo", "Combo", true);
                // Combo.Add(new MenuSlider("combo.autoSafeRange", "Auto Combo safe range", 2f, 10f, 0f));
                Combo.Add(new MenuCheckBox("combo.invisible", "Attack invisible enemies", true));
                Combo.Add(new MenuCheckBox("combo.useM1", "Use Left Mouse", true));
                Combo.Add(new MenuCheckBox("combo.interruptM1", " ^ Interrupt Left Mouse if needed", true));
                // Combo.Add(new MenuSlider("combo.useM1.safeRange", "    ^ Safe range", 2.5f, 5f, 0f));
                Combo.Add(new MenuCheckBox("combo.useE", "Use E", true));
                Combo.Add(new MenuCheckBox("combo.cancelE", " ^ Cancel E if no target", true));
                Combo.Add(new MenuCheckBox("combo.useR", "Use R", false));
                Combo.Add(new MenuCheckBox("combo.useRS", " ^ Only to reset Space CD regardless min energy", false));
                Combo.Add(new MenuIntSlider("combo.useR.minEnergyBars", "    ^ Min energy bars", 3, 4, 1));
                Combo.Add(new MenuCheckBox("combo.useF", "Use F", true));
            }
            //Healing
            {
                Healing = new Menu("ulric.healing", "Healing", true);
                Healing.Add(new MenuCheckBox("healing.autohealSelf", "Auto Self Healing", true));
                Healing.Add(new MenuCheckBox("healing.useM2", "Heal M2", true));
                Healing.Add(new MenuSlider("healing.useM2.safeRange", "    ^ Safe range", 3.25f, 5f, 2.5f));
                Healing.Add(new MenuCheckBox("healing.useSpace", "Heal Space", true));
                Healing.Add(new MenuCheckBox("healing.useEX1", "Heal EX1", true));
                Healing.Add(new MenuIntSlider("healing.useEX1.minEnergyBars", "    ^ Min energy bars", 2, 4, 1));
                Healing.Add(new MenuCheckBox("healing.useEX2", "Heal EX2", true));
                Healing.Add(new MenuIntSlider("healing.useEX2.minEnergyBars", "    ^ Min energy bars", 2, 4, 1));
            }
            //Anti-Gapclosing
            // {
            //     AntiGapclosing = new Menu("ulric.gapclosing", "Anti-Gapclosing", false);
            // }
            //Misc
            {
                Misc = new Menu("ulric.misc", "Misc", false);
                // Misc.Add(new MenuCheckBox("misc.targetOrb", "Attack the Orb", true));
                Misc.Add(new MenuCheckBox("misc.antiJump", "Anti Jumps with R", true));
                Misc.Add(new MenuCheckBox("misc.useSpaceexec", "Use Space to execute", true));
                Misc.Add(new MenuCheckBox("misc.useEX1exec", "Use EX1 to execute", true));
            }
            //Drawings
            {
                Drawings = new Menu("ulric.drawings", "Drawings", true);
                Drawings.Add(new MenuCheckBox("draw.rangeM1.safeRange", "Draw Left Mouse Range", false));
                Drawings.Add(new MenuCheckBox("draw.Target", "Draw Target", false));
            }
            //Finally
            {
                Menu.Add(Keys);
                Menu.Add(Combo);
                Menu.Add(Healing);
                // Menu.Add(AntiGapclosing);
                Menu.Add(Misc);
                Menu.Add(Drawings);
                rootMenu.Add(Menu);
            }
        }

        public void InitializeAbilities()
        {
            M1 = new Ability(AbilityKey.M1, 2.5f);
            M2 = new Ability(AbilityKey.M2, 9f, 28f, 0.75f);
            EX1 = new Ability(AbilityKey.EX1, 9f, 22.5f, 0.3f);
            EX2 = new Ability(AbilityKey.EX2, 4.25f);
            Space = new Ability(AbilityKey.Space, 7f, 22.5f, 0.3f);
            Q = new Ability(AbilityKey.Q, 9f, 25.5f, 0.3f);
            E = new Ability(AbilityKey.E, 8.5f, 21.5f, 0.3f);
            R = new Ability(AbilityKey.R, 6f, 22.5f, 0.45f);
            F = new Ability(AbilityKey.F, 12f, 27f, 0.45f);
        }

        public static bool HasVindicatorUpgrade
        {
            get
            {
                var Battlerites = new List<Battlerite>(5);
                if (Battlerites.Any()) Battlerites.Clear();

                for (var i = 0; i < 5; i++)
                {
                    var br = HeroPlayer.BattleriteSystem.GetEquippedBattlerite(i);
                    if (br != null) Battlerites.Add(br);
                }

                var Vindicator = Battlerites.Any(x => x.Name.Equals("VindicatorUpgrade"));
                if (Vindicator) return true;

                return false;
            }
        }

        public Ulric()
        {
            Initialize();
        }

        private void Game_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame) return;

            if (HeroPlayer.Living.IsDead) return;

            if (Drawings.GetBoolean("draw.rangeM1.safeRange")) Drawing.DrawCircle(Utility.MyPos, M1.Range, Color.white);

            if (Drawings.GetBoolean("draw.Target")) Drawing.DrawCircle(LocalPlayer.AimPosition, 1f, Color.red);
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInRoundPhase)
                //|| HeroPlayer.IsMounted
                return;

            if (Keys.GetBoolean("keys.autoCombo") &&
                Keys.GetKeybind("keys.M1") || Keys.GetKeybind("keys.M2") || Keys.GetKeybind("keys.Space") ||
                Keys.GetKeybind("keys.EX1"))
            {
                LocalPlayer.EditAimPosition = false;
                CastingAbility = null;
                return;
            }

            var enemiesToTargetBase = EntitiesManager.EnemyTeam.Where(x => x.IsValid && !x.Living.IsDead && !x.HasBuff("OtherSideBuff") &&
                !x.HasBuff("AscensionBuff") && !x.HasBuff("AscensionTravelBuff") && !x.HasBuff("Fleetfoot") && !x.HasBuff("TempestRushBuff") &&
                !x.HasBuff("ValiantLeap") &&!x.HasBuff("FrogLeap") && !x.HasBuff("FrogLeapRecast") && !x.HasBuff("ElusiveStrikeCharged") &&
                !x.HasBuff("ElusiveStrikeWall2") && !x.HasBuff("BurrowAlternate") && !x.HasBuff("JetPack") && !x.HasBuff("ProwlBuff") &&
                !x.HasBuff("Dive") && !x.HasBuff("InfestingBuff") && !x.HasBuff("PortalBuff") && !x.HasBuff("CrushingBlow") &&
                !x.HasBuff("TornadoBuff") && !x.HasBuff("LawBringerInAir") && !x.HasBuff("LawBringerLeap"));

            if (!Combo.GetBoolean("combo.invisible"))
                enemiesToTargetBase = enemiesToTargetBase.Where(x => !x.CharacterModel.IsModelInvisible);

            var alliesToTargetBase =
                EntitiesManager.LocalTeam.Where(x => !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial);
            var alliesToTargetHeal = EntitiesManager.LocalTeam.Where(x =>
                !x.IsLocalPlayer && !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial);
            var selfToTargetHeal = EntitiesManager.LocalTeam.Where(x =>
                x.IsLocalPlayer && !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial);

            var enemiesToTargetProjs = enemiesToTargetBase.Where(x =>
                !x.IsCountering && !x.HasConsumeBuff && !x.HasBuff("ElectricShield") &&
                !x.HasBuff("BarbedHuskBuff") && !x.HasBuff("BulwarkBuff") && !x.HasBuff("DivineShieldBuff") &&
                !x.HasBuff("GustBuff") && !x.HasBuff("TimeBenderBuff"));

            // var ennemiesToTargetMelee = enemiesToTargetBase.Where(x => x.);

            var M2SafeRange = Healing.GetSlider("healing.useM2.safeRange");
            var SpaceDamage = 12;
            var EX1Damage = 12;
            var M2Heal = 14;
            var SpaceHeal = 9;
            var EX2Heal = 18;

            var enemiesToExecuteSpace = enemiesToTargetProjs.Where(x => x.Living.Health <= SpaceDamage);
            var enemiesToExecuteEX1 = enemiesToTargetProjs.Where(x => x.Living.Health <= EX1Damage);

            var selfToTargetM2 = selfToTargetHeal.Where(x => x.Living.Health <= x.Living.MaxRecoveryHealth - M2Heal);
            var alliesToTargetM2 =
                alliesToTargetHeal.Where(x => x.Living.Health <= x.Living.MaxRecoveryHealth - M2Heal);
            var alliesToTargetSpace =
                alliesToTargetHeal.Where(x => !x.HasBuff("Lightbringer") &&
                                              (x.Living.Health <= x.Living.MaxRecoveryHealth - SpaceHeal ||
                                               HeroPlayer.Living.Health <=
                                               HeroPlayer.Living.MaxRecoveryHealth - SpaceHeal));
            var alliesToTargetEX1 = alliesToTargetSpace.Where(x => x.Distance(HeroPlayer) > Space.Range);
            var alliesToTargetEX2 =
                alliesToTargetBase.Where(x => x.Living.Health <= x.Living.MaxRecoveryHealth - EX2Heal);

            var CrushingBlowRange = 3.75f;
            var BurrowRange = 2.5f;
            var enemiesToaAntiJump =
                EntitiesManager.EnemyTeam.Where(x => (x.HasBuff("CrushingBlow") || x.HasBuff("BurrowAlternate"))
                                                     && (x.Distance(HeroPlayer) <= CrushingBlowRange ||
                                                         x.Distance(HeroPlayer) <= BurrowRange));

            // var targetSelf = TargetSelector.GetTarget(alliesSelf, targetMode, 1f);
            var M1Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M1.Range);
            var M2Self = TargetSelector.GetTarget(selfToTargetM2, TargetingMode.NearMouse, M2.Range);
            var M2Allies = TargetSelector.GetTarget(alliesToTargetM2, TargetingMode.LowestHealth, M2.Range);
            var SpaceAllies = TargetSelector.GetTarget(alliesToTargetSpace, TargetingMode.Closest, Space.Range);
            var SpaceExecute = TargetSelector.GetTarget(enemiesToExecuteSpace, TargetingMode.LowestHealth, Space.Range);
            var QTarget = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, Q.Range);
            var ETarget = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, E.Range);
            var RTarget = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, R.Range);
            var RAntiJump = TargetSelector.GetTarget(enemiesToaAntiJump, TargetingMode.NearMouse, R.Range);
            var FTarget = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.LowestHealth, F.Range);
            var EX1Allies = TargetSelector.GetTarget(alliesToTargetEX1, TargetingMode.Closest, EX1.Range);
            var EX1Execute = TargetSelector.GetTarget(enemiesToExecuteEX1, TargetingMode.LowestHealth, EX1.Range);
            var EX2Allies = TargetSelector.GetTarget(alliesToTargetEX2, TargetingMode.Closest, EX2.Range);

            //Self Healing
            if (Keys.GetKeybind("keys.healSelf") && !Healing.GetBoolean("healing.autohealSelf"))
            {
                if (Channeling)
                {
                    LocalPlayer.EditAimPosition = true;
                    switch (CastingAbility.Key)
                    {
                        case AbilityKey.M2:
                            LocalPlayer.Aim(HeroPlayer.MapObject.Position);
                            break;
                    }
                }
                else
                {
                    LocalPlayer.EditAimPosition = false;
                    CastingAbility = null;
                }

                if (M2.CanCast && CastingAbility == null &&
                    HeroPlayer.Living.Health <= HeroPlayer.Living.MaxRecoveryHealth - M2Heal / 2)
                {
                    LocalPlayer.PressAbility(M2.Slot, true);
                    CastingAbility = M2;
                    return;
                }
            }

            // if (IsAntiGapclosing)
            //     return;
            if (Keys.GetKeybind("keys.combo") || Keys.GetBoolean("keys.autoCombo") && !Keys.GetKeybind("keys.healSelf"))
            {
                //Combo
                if (!Channeling)
                {
                    LocalPlayer.EditAimPosition = false;
                    CastingAbility = null;

                    if (QTarget != null && HeroPlayer.HasBuff("DivineShieldBuff"))
                    {
                        var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, QTarget, Q.Range, Q.Speed,
                            Q.Radius);
                        if (pred.CanHit)
                            LocalPlayer.Aim(pred.CastPosition);
                    }

                    if (RTarget != null && HeroPlayer.HasBuff("LawBringerLeap"))
                    {
                        var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, RTarget, R.Range, R.Speed,
                            R.Radius);
                        if (pred.CanHit)
                            LocalPlayer.Aim(pred.CastPosition);
                        else
                            LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                    }

                    if (EX1Execute != null && EX1.CanCast && Misc.GetBoolean("misc.useEX1exec"))
                    {
                        LocalPlayer.PressAbility(EX1.Slot, true);
                        CastingAbility = EX1;
                        return;
                    }

                    if (SpaceExecute != null && Space.CanCast && Misc.GetBoolean("misc.useSpaceexec"))
                    {
                        LocalPlayer.PressAbility(Space.Slot, true);
                        CastingAbility = Space;
                        return;
                    }

                    var SpaceCD = LocalPlayer.GetAbilityHudData(AbilitySlot.Ability3);
                    if (RTarget != null && (R.CanCast || HeroPlayer.HasBuff("LawBringerLeap")) &&
                        SpaceCD.CooldownLeft > 4 && HasVindicatorUpgrade &&
                        !HeroPlayer.HasBuff("DivineShieldBuff") && Combo.GetBoolean("combo.useR") && Combo.GetBoolean("combo.useRS"))
                    {
                        var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, RTarget, R.Range, R.Speed,
                            R.Radius);
                        if (pred.CanHit)
                        {
                            LocalPlayer.EditAimPosition = true;
                            LocalPlayer.Aim(pred.CastPosition);
                            LocalPlayer.PressAbility(R.Slot, true);
                            CastingAbility = R;
                        }
                    }

                    if ((EX2Allies != null) & EX2.CanCast && Healing.GetBoolean("healing.useEX2"))
                    {
                        var energyRequired = Healing.GetIntSlider("healing.useEX2.minEnergyBars") * 25;
                        if (energyRequired <= HeroPlayer.Energized.Energy)
                        {
                            LocalPlayer.PressAbility(EX2.Slot, true);
                            CastingAbility = EX2;
                            return;
                        }
                    }

                    if (EX1Allies != null && EX1.CanCast && !HeroPlayer.HasBuff("DivineShieldBuff") && Healing.GetBoolean("healing.useEX1"))
                    {
                        var energyRequired = Healing.GetIntSlider("healing.useEX1.minEnergyBars") * 25;
                        if (energyRequired <= HeroPlayer.Energized.Energy)
                        {
                            LocalPlayer.PressAbility(EX1.Slot, true);
                            CastingAbility = EX1;
                            return;
                        }
                    }

                    if (SpaceAllies != null && Space.CanCast && !HeroPlayer.HasBuff("DivineShieldBuff") && Healing.GetBoolean("healing.useSpace"))
                    {
                        LocalPlayer.PressAbility(Space.Slot, true);
                        CastingAbility = Space;
                        return;
                    }

                    if (RAntiJump != null && R.CanCast && Misc.GetBoolean("misc.antiJump"))
                    {
                        LocalPlayer.PressAbility(R.Slot, true);
                        CastingAbility = R;
                        return;
                    }

                    if (FTarget != null && F.CanCast && !HeroPlayer.HasBuff("DivineShieldBuff") && Combo.GetBoolean("combo.useF"))
                    {
                        LocalPlayer.PressAbility(F.Slot, true);
                        CastingAbility = F;
                        return;
                    }

                    if (RTarget != null && R.CanCast && !HeroPlayer.HasBuff("DivineShieldBuff") && Combo.GetBoolean("combo.useR") &&
                        !Combo.GetBoolean("combo.useRS"))
                    {
                        var energyRequired = Combo.GetIntSlider("combo.useR.minEnergyBars") * 25;
                        if (energyRequired <= HeroPlayer.Energized.Energy)
                        {
                            LocalPlayer.PressAbility(R.Slot, true);
                            CastingAbility = R;
                            return;
                        }
                    }

                    if (M1Target != null && M1.CanCast && Combo.GetBoolean("combo.useM1"))
                    {
                        LocalPlayer.PressAbility(M1.Slot, true);
                        CastingAbility = M1;
                        return;
                    }

                    if (ETarget != null && M2.CanCast && HeroPlayer.HasBuff("SmiteBuff") &&
                        Combo.GetBoolean("combo.useE"))
                    {
                        LocalPlayer.PressAbility(M2.Slot, true);
                        CastingAbility = M2;
                        return;
                    }

                    //Cancel Smite if no target
                    if (ETarget == null && (M2Self != null || M2Allies != null) && E.CanCast && M2.CanCast &&
                        HeroPlayer.HasBuff("SmiteBuff") && Combo.GetBoolean("combo.useE") && Combo.GetBoolean("combo.cancelE"))
                    {
                        LocalPlayer.PressAbility(E.Slot, true);
                        CastingAbility = E;
                    }

                    if (M2Self != null && M2.CanCast && !HeroPlayer.HasBuff("SmiteBuff") &&
                        !HeroPlayer.HasBuff("DivineShieldBuff") &&
                        !HeroPlayer.HasBuff("LawBringerLeap") && HeroPlayer.EnemiesAroundAlive(M2SafeRange) < 1 &&
                        Healing.GetBoolean("healing.useM2") && Healing.GetBoolean("healing.autohealSelf"))
                    {
                        LocalPlayer.PressAbility(M2.Slot, true);
                        CastingAbility = M2;
                        return;
                    }

                    if (M2Allies != null && M2.CanCast && !HeroPlayer.HasBuff("SmiteBuff") &&
                        !HeroPlayer.HasBuff("DivineShieldBuff") &&
                        !HeroPlayer.HasBuff("LawBringerLeap") && HeroPlayer.EnemiesAroundAlive(M2SafeRange) < 1 &&
                        Healing.GetBoolean("healing.useM2"))
                    {
                        LocalPlayer.PressAbility(M2.Slot, true);
                        CastingAbility = M2;
                        return;
                    }

                    if (ETarget != null && E.CanCast && !HeroPlayer.HasBuff("DivineShieldBuff") && !HeroPlayer.HasBuff("SmiteBuff") &&
                        Combo.GetBoolean("combo.useE"))
                    {
                        LocalPlayer.PressAbility(E.Slot, true);
                        CastingAbility = E;
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
                        case AbilityKey.EX1:
                            if (EX1Execute != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, EX1Execute, EX1.Range,
                                    EX1.Speed, EX1.Radius, true);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            else if (EX1Allies != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, EX1Allies, EX1.Range,
                                    EX1.Speed, EX1.Radius, true);
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
                            if (SpaceExecute != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, SpaceExecute,
                                    Space.Range, Space.Speed, Space.Radius, true);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            else if (SpaceAllies != null)
                            {
                                var pred = TestPrediction.GetPrediction(Utility.MyPos, SpaceAllies, Space.Range,
                                    Space.Speed, Space.Radius, 0.2f, 0f, true);
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

                        case AbilityKey.F:
                            if (FTarget != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, FTarget, F.Range,
                                    F.Speed, F.Radius, true);
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
                                LocalPlayer.Aim(M1Target.MapObject.Position);
                            else if (Combo.GetBoolean("combo.interruptM1"))
                                LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            break;

                        case AbilityKey.M2:
                            if (ETarget != null && HeroPlayer.HasBuff("SmiteBuff"))
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, ETarget, E.Range,
                                    E.Speed, E.Radius, true);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                            }
                            else if (M2Self != null && !HeroPlayer.HasBuff("SmiteBuff") &&
                                     !HeroPlayer.HasBuff("LawBringerLeap") &&
                                     HeroPlayer.EnemiesAroundAlive(M2SafeRange) < 1 &&
                                     Healing.GetBoolean("healing.autohealSelf"))
                            {
                                LocalPlayer.Aim(HeroPlayer.MapObject.Position);
                            }
                            else if (M2Allies != null && !HeroPlayer.HasBuff("SmiteBuff") &&
                                     !HeroPlayer.HasBuff("LawBringerLeap") &&
                                     HeroPlayer.EnemiesAroundAlive(M2SafeRange) < 1)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, M2Allies, M2.Range,
                                    M2.Speed, M2.Radius, true);
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
            MainMenu.GetMenu("kavey_series").Get<Menu>("ulric").Hidden = true;
            Game.OnUpdate -= Game_OnUpdate;
            Game.OnDraw -= Game_OnDraw;
            // AntiGapcloser.OnGapcloser -= AntiGapcloser_OnGapcloser;
        }
    }
}