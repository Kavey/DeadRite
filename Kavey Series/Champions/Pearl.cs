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
    internal class Pearl : IChampion
    {
        public bool Initialized { get; set; }
        public string ChampionName { get; set; } = "Pearl";
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
            MainMenu.GetMenu("kavey_series").Get<Menu>("pearl").Hidden = false;
            Utility.Log("[Kavey Series] {0} loaded!", ConsoleColor.Cyan, ChampionName);
            Initialized = true;
        }

        public void InitializeMenu()
        {
            var rootMenu = MainMenu.GetMenu("kavey_series");
            Menu = new Menu("pearl", "Pearl", true);
            //Keys
            {
                Keys = new Menu("pearl.keys", "Keys", true);
                Keys.Add(new MenuKeybind("keys.combo", "Combo Key", KeyCode.Y));
                Keys.Add(new MenuKeybind("keys.healSelf", "Heal self", KeyCode.LeftControl));
                Keys.Add(new MenuCheckBox("keys.autoCombo", "Auto Combo Mode", true));
                Keys.Add(new MenuKeybind("keys.M1", "Left Mouse keybind to pause Auto Combo", KeyCode.Mouse2));
                Keys.Add(new MenuKeybind("keys.M2", "Right Mouse keybind to pause Auto Combo", KeyCode.Mouse1));
                Keys.Add(new MenuKeybind("keys.Space", "Space keybind to pause Auto Combo", KeyCode.Space));
                Keys.Add(new MenuKeybind("keys.E", "E keybind to pause Auto Combo", KeyCode.Alpha2));
                Keys.Add(new MenuKeybind("keys.EX2", "EX1 keybind to pause Auto Combo", KeyCode.G));
            }
            //Combo
            {
                Combo = new Menu("pearl.combo", "Combo", true);
                // Combo.Add(new MenuSlider("combo.autoSafeRange", "Auto Combo safe range", 2f, 10f, 0f));
                Combo.Add(new MenuCheckBox("combo.invisible", "Attack invisible enemies", true));
                Combo.Add(new MenuCheckBox("combo.useM1", "Use Left Mouse", true));
                Combo.Add(new MenuCheckBox("combo.interruptM1", " ^ Interrupt Left Mouse if needed", true));
                // Combo.Add(new MenuSlider("combo.useM1.safeRange", "    ^ Safe range", 2.5f, 5f, 0f));
                Combo.Add(new MenuCheckBox("combo.useF", "Use F", true));
            }
            //Healing
            {
                Healing = new Menu("pearl.healing", "Healing", true);
                Healing.Add(new MenuCheckBox("healing.autohealSelf", "Auto Self Healing", true));
                Healing.Add(new MenuCheckBox("healing.useM2", "Heal M2", true));
                Healing.Add(new MenuSlider("healing.useM2.safeRange", "    ^ Safe range", 3.25f, 5f, 2.5f));
                Healing.Add(new MenuCheckBox("healing.useR", "Dispel with R", true));
            }
            //Anti-Gapclosing
            // {
            //     AntiGapclosing = new Menu("pearl.gapclosing", "Anti-Gapclosing", false);
            // }
            //Misc
            {
                Misc = new Menu("pearl.misc", "Misc", false);
                // Misc.Add(new MenuCheckBox("misc.targetOrb", "Attack the Orb", true));
                Misc.Add(new MenuCheckBox("misc.antiJump", "Anti Jumps with Space", true));
                Misc.Add(new MenuCheckBox("misc.useE.melee", "Use Bubble on self on melee range", false));
                Misc.Add(new MenuCheckBox("misc.useE.RipplingWaters", "Use Bubble on self with Rippling Waters", false));
            }
            //Drawings
            {
                Drawings = new Menu("pearl.drawings", "Drawings", true);
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
            M1 = new Ability(AbilityKey.M1, 7.6f, 0.25f, 22f);
            M2 = new Ability(AbilityKey.M2, 9f, 28f, 0.75f);
            EX1 = new Ability(AbilityKey.EX1);
            EX2 = new Ability(AbilityKey.EX2);
            Space = new Ability(AbilityKey.Space);
            Q = new Ability(AbilityKey.Q);
            E = new Ability(AbilityKey.E, 8f);
            R = new Ability(AbilityKey.R, 10f);
            F = new Ability(AbilityKey.F, 10f, 15f, 1.25f);
        }

        private static bool IsInDive
        {
            get
            {
                var DiveBuff = HeroPlayer.HasBuff("Dive");
                if (DiveBuff)
                {
                    return true;
                }

                return false;
            }
        }

        public static bool HasRipplingWaters
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
        
                var RipplingWaters = Battlerites.Any(x => x.Name.Equals("RipplingWatersUpgrade"));
                if (RipplingWaters) return true;
        
                return false;
            }
        }

        public static bool HasTastyFishUpgrade
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

                var RipplingWaters = Battlerites.Any(x => x.Name.Equals("TastyFishUpgrade"));
                if (RipplingWaters) return true;

                return false;
            }
        }

        public Pearl()
        {
            Initialize();
        }

        private void Game_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame) return;

            if (HeroPlayer.Living.IsDead) return;

            // if (HeroPlayer.IsWeaponCharged)
            // {
            //     Drawing.DrawString(new Vector2(1920f / 2f, (1080f / 2f) - 45f).ScreenToWorld(),
            //         "WEAPON IS CHARGED!", Color.magenta);
            // }

            if (Drawings.GetBoolean("draw.rangeM1.safeRange")) Drawing.DrawCircle(Utility.MyPos, M1.Range, Color.white);

            if (Drawings.GetBoolean("draw.Target")) Drawing.DrawCircle(LocalPlayer.AimPosition, 1f, Color.red);
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInRoundPhase)
                //|| HeroPlayer.IsMounted
                return;

            if (IsInDive || Keys.GetBoolean("keys.autoCombo") && (Keys.GetKeybind("keys.M1") || Keys.GetKeybind("keys.M2") || Keys.GetKeybind("keys.Space") || 
                                                                  Keys.GetKeybind("keys.E") || Keys.GetKeybind("keys.EX2")))
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
            var TastyFishHeal = 10;
            var M2Heal = 16;

            var alliesToTargetE =
                alliesToTargetHeal.Where(x => x.Living.Health <= x.Living.MaxRecoveryHealth - TastyFishHeal);
            var selfToTargetM2 = selfToTargetHeal.Where(x => x.Living.Health <= x.Living.MaxRecoveryHealth - M2Heal);
            var alliesToTargetM2 =
                alliesToTargetHeal.Where(x => x.Living.Health <= x.Living.MaxRecoveryHealth - M2Heal);

            var CrushingBlowRange = 3.75f;
            var BurrowRange = 2.5f;
            var enemiesToaAntiJump =
                EntitiesManager.EnemyTeam.Where(x => (x.HasBuff("CrushingBlow") || x.HasBuff("BurrowAlternate"))
                                                     && (x.Distance(HeroPlayer) <= CrushingBlowRange ||
                                                         x.Distance(HeroPlayer) <= BurrowRange));

            // Dispelling Debuff (allies):
            //
            var alliesToTargetR = alliesToTargetBase.Where(x => // x.Get<AgeObject>().Age > 0.75f &&
                x.HasBuff("Weaken") ||
                //!x.IsLocalPlayer && x.HasBuff("Stun") || 
                !x.IsLocalPlayer && x.HasBuff("LunarStrikePetrify") || !x.IsLocalPlayer && x.HasBuff("Panic") ||
                !x.IsLocalPlayer && x.HasBuff("Incapacitate") || x.HasBuff("DeadlyInjectionBuff") ||
                !x.IsLocalPlayer && x.HasBuff("Fear") || !x.IsLocalPlayer && x.HasBuff("Petrify") ||
                x.HasBuff("PhantomCutBuff") || x.HasBuff("EntanglingRootsBuff") || // x.HasBuff("FrostDebuff") ||
                !x.IsLocalPlayer && x.HasBuff("Frozen") || !x.IsLocalPlayer && x.HasBuff("StormStruckDebuff") ||
                !x.IsLocalPlayer && x.HasBuff("BrainBugDebuff") || x.HasBuff("HandOfJudgementBuff") ||
                // x.HasBuff("HandOfCorruptionBuff) ||
                !x.IsLocalPlayer && x.HasBuff("SheepTrickDebuff") || x.HasBuff("SludgeSpitDebuff") ||
                x.HasBuff("BlindingLightBlind"));
            //

            // var targetSelf = TargetSelector.GetTarget(alliesSelf, targetMode, 1f);
            var M1Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M1.Range);
            var M2Self = TargetSelector.GetTarget(selfToTargetM2, TargetingMode.NearMouse, M2.Range);
            var M2Allies = TargetSelector.GetTarget(alliesToTargetM2, TargetingMode.LowestHealth, M2.Range);
            var EAllies = TargetSelector.GetTarget(alliesToTargetE, TargetingMode.NearMouse, E.Range);
            var RAllies = TargetSelector.GetTarget(alliesToTargetR, TargetingMode.NearMouse, R.Range);
            var SpaceAntiJump = TargetSelector.GetTarget(enemiesToaAntiJump, TargetingMode.NearMouse, R.Range);
            var FTarget = TargetSelector.GetTarget(enemiesToTargetBase, TargetingMode.LowestHealth, F.Range);

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

                    if (SpaceAntiJump != null && R.CanCast && Misc.GetBoolean("misc.antiJump"))
                    {
                        LocalPlayer.PressAbility(Space.Slot, true);
                        CastingAbility = Space;
                        return;
                    }

                    if (RAllies != null && R.CanCast && Healing.GetBoolean("healing.useR"))
                    {
                        var energyRequired = 25;
                        if (energyRequired <= HeroPlayer.Energized.Energy)
                        {
                            LocalPlayer.PressAbility(R.Slot, true);
                            CastingAbility = R;
                            return;
                        }
                    }

                    if (HeroPlayer.EnemiesAroundAlive(2.5f) > 0 && E.CanCast && Misc.GetBoolean("misc.useE.melee"))
                    {
                        LocalPlayer.Aim(HeroPlayer.MapObject.Position);
                        LocalPlayer.PressAbility(E.Slot, true);
                        CastingAbility = E;
                        return;
                    }

                    if (EAllies != null && HasTastyFishUpgrade && E.CanCast)
                    {
                        LocalPlayer.PressAbility(E.Slot, true);
                        CastingAbility = E;
                        return;
                    }

                    if (FTarget != null && F.CanCast && Combo.GetBoolean("combo.useF"))
                    {
                        LocalPlayer.PressAbility(F.Slot, true);
                        CastingAbility = F;
                        return;
                    }

                    if (M1Target != null && E.CanCast && HasRipplingWaters && Misc.GetBoolean("misc.useE.RipplingWaters"))
                    {
                        LocalPlayer.Aim(HeroPlayer.MapObject.Position);
                        LocalPlayer.PressAbility(E.Slot, true);
                        CastingAbility = E;
                        return;
                    }

                    if (M1Target != null && M1.CanCast && Combo.GetBoolean("combo.useM1"))
                    {
                        LocalPlayer.PressAbility(M1.Slot, true);
                        CastingAbility = M1;
                        return;
                    }

                    if (M2Self != null && M2.CanCast && HeroPlayer.EnemiesAroundAlive(M2SafeRange) < 1 &&
                        Healing.GetBoolean("healing.useM2") && Healing.GetBoolean("healing.autohealSelf"))
                    {
                        LocalPlayer.PressAbility(M2.Slot, true);
                        CastingAbility = M2;
                        return;
                    }

                    if (M2Allies != null && M2.CanCast && HeroPlayer.EnemiesAroundAlive(M2SafeRange) < 1 &&
                        Healing.GetBoolean("healing.useM2"))
                    {
                        LocalPlayer.PressAbility(M2.Slot, true);
                        CastingAbility = M2;
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

                        case AbilityKey.R:
                            if (RAllies != null)
                            {
                                LocalPlayer.Aim(RAllies.MapObject.Position);
                            }
                            else
                            {
                                LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }

                            break;

                        case AbilityKey.E:
                            if (EAllies != null)
                            {
                                LocalPlayer.Aim(EAllies.MapObject.Position);
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
                            if (M2Self != null && HeroPlayer.EnemiesAroundAlive(M2SafeRange) < 1 &&
                                Healing.GetBoolean("healing.autohealSelf"))
                            {
                                LocalPlayer.Aim(HeroPlayer.MapObject.Position);
                            }
                            else if (M2Allies != null && HeroPlayer.EnemiesAroundAlive(M2SafeRange) < 1)
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
            MainMenu.GetMenu("kavey_series").Get<Menu>("pearl").Hidden = true;
            Game.OnUpdate -= Game_OnUpdate;
            Game.OnDraw -= Game_OnDraw;
            // AntiGapcloser.OnGapcloser -= AntiGapcloser_OnGapcloser;
        }
    }
}