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
    class Pestilus : IChampion
    {
        public bool Initialized { get; set; }
        public string ChampionName { get; set; } = "Pestilus";
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

        // internal MenuComboBox ComboOrder;

        // public string InsertBeforeUpperCase(string str, string toInsert)
        // {
        //     var sb = new StringBuilder();
        //
        //     var previousChar = char.MinValue;
        //
        //     foreach (var c in str)
        //     {
        //         if (char.IsUpper(c))
        //             if (sb.Length != 0 && previousChar != ' ')
        //                 foreach (var t in toInsert)
        //                     sb.Append(t);
        //
        //         sb.Append(c);
        //
        //         previousChar = c;
        //     }
        //
        //     return sb.ToString();
        // }

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
            MainMenu.GetMenu("kavey_series").Get<Menu>("pestilus").Hidden = false;
            Utility.Log("[Kavey Series] {0} loaded!", ConsoleColor.Green, ChampionName);
            Initialized = true;
        }

        // InGameObject.OnCreate += OnCreate;
        // InGameObject.OnDestroy += OnDestroy;

        public void InitializeMenu()
        {
            var rootMenu = MainMenu.GetMenu("kavey_series");
            Menu = new Menu("pestilus", "Pestilus", true);
            //Keys
            {
                Keys = new Menu("pestilus.keys", "Keys", true);
                Keys.Add(new MenuKeybind("keys.combo", "Combo Key", UnityEngine.KeyCode.Mouse0));
                Keys.Add(new MenuCheckBox("keys.autoCombo", "Auto Combo Mode", true));
                Keys.Add(new MenuKeybind("keys.M1", "Left Mouse keybind to pause Auto Combo", UnityEngine.KeyCode.Mouse2));
                Keys.Add(new MenuKeybind("keys.M2", "Right Mouse keybind to pause Auto Combo", UnityEngine.KeyCode.Mouse1));
                Keys.Add(new MenuKeybind("keys.Q", "Q keybind to pause Auto Combo", UnityEngine.KeyCode.Alpha1));
                Keys.Add(new MenuKeybind("keys.Space", "Space keybind to pause Auto Combo", UnityEngine.KeyCode.Space));
                Keys.Add(new MenuKeybind("keys.R", "R keybind to pause Auto Combo", UnityEngine.KeyCode.R));
            }
            //Combo
            {
                Combo = new Menu("pestilus.combo", "Combo", true);
                // Combo.Add(new MenuSlider("combo.autoSafeRange", "Auto Combo safe range", 2f, 10f, 0f));
                Combo.Add(new MenuCheckBox("combo.invisible", "Attack invisible enemies", true));
                // ComboOrder = Combo.Add(new MenuComboBox("combo.order", "Combo Target Order", 1,
                //     Enum.GetNames(typeof(TargetingOrder))
                //         .Select(s => InsertBeforeUpperCase(s, " > ")).ToArray()));
                Combo.Add(new MenuCheckBox("combo.useM1", "Use Left Mouse", true));
                Combo.Add(new MenuCheckBox("combo.interruptM1", " ^ Interrupt Left Mouse if needed", true));
                // Combo.Add(new MenuSlider("combo.useM1.safeRange", "    ^ Safe range", 2.5f, 5f, 0f));
                Combo.Add(new MenuCheckBox("combo.useM2", "Use Right Mouse", true));
                Combo.Add(new MenuCheckBox("combo.interruptM2", " ^ Interrupt Right Mouse if needed", true));
                Combo.Add(new MenuCheckBox("combo.useE", "Use E", true));
                Combo.Add(new MenuCheckBox("combo.interruptE", " ^ Interrupt E if needed", true));
                Combo.Add(new MenuCheckBox("combo.useR", "Use R on allies to dispel", true));
                Combo.Add(new MenuCheckBox("combo.useEX1", "Use EX1", true));
                Combo.Add(new MenuIntSlider("combo.useEX1.minEnergyBars", "    ^ Min energy bars", 3, 4, 1));
                Combo.Add(new MenuCheckBox("combo.useEX2", "Use EX2", true));
                Combo.Add(new MenuIntSlider("combo.useEX2.minEnergyBars", "    ^ Min energy bars", 2, 4, 1));
                Combo.Add(new MenuCheckBox("combo.useF", "Use F", true));
            }
            //Anti-Gapclosing
            {
                AntiGapclosing = new Menu("pestilus.gapclosing", "Anti-Gapclosing", true);
                AntiGapclosing.Add(new MenuCheckBox("gapclosing.Q", "Block with Q", true));
                AntiGapclosing.Add(new MenuCheckBox("gapclosing.EX1", "Block with EX1", true));
            }
            //Misc
            {
                Misc = new Menu("pestilus.misc", "Misc", true);
                // Misc.Add(new MenuCheckBox("misc.targetOrb", "Attack the Orb", true));
                Misc.Add(new MenuCheckBox("misc.useQ.melee", "Use Q on self in melee range", true));
                Misc.Add(new MenuCheckBox("misc.useM2exec", "Use M2 to execute", true));
            }
            //Drawings
            {
                Drawings = new Menu("pestilus.drawings", "Drawings", true);
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
            M1 = new Ability(AbilityKey.M1, 7f, 18f, 0.25f);
            M2 = new Ability(AbilityKey.M2, 9f, 30f, 0.4f);
            EX1 = new Ability(AbilityKey.EX1, 3f);
            EX2 = new Ability(AbilityKey.EX2, 7.3f, 27f, 0.4f);
            Space = new Ability(AbilityKey.Space);
            Q = new Ability(AbilityKey.Q, 20f);
            E = new Ability(AbilityKey.E, 8f, 10f, 1.5f, SkillType.Circle);
            R = new Ability(AbilityKey.R, 10f);
            F = new Ability(AbilityKey.F, 12f, 27f, 0.45f);
        }

        private static bool IsInUltimate
        {
            get
            {
                var ScarabPack = HeroPlayer.HasBuff("ScarabPackBuff");
                if (ScarabPack)
                {
                    return true;
                }

                return false;
            }
        }

        private static bool IsInInfest
        {
            get
            {
                var Infested = HeroPlayer.HasBuff("RecastBuff");
                if (Infested)
                {
                    return true;
                }

                return false;
            }
        }

        // private static bool HasBattlerite
        // {
        //     get
        //     {
        //         List<Battlerite> Battlerites = new List<Battlerite>(5);
        //         if (Battlerites.Any())
        //         {
        //             Battlerites.Clear();
        //         }
        //
        //         for (var i = 0; i < 5; i++)
        //         {
        //             var br = HeroPlayer.BattleriteSystem.GetEquippedBattlerite(i);
        //             if (br != null)
        //             {
        //                 Battlerites.Add(br);
        //             }
        //         }
        //
        //         var HyperspeedUpgrade = Battlerites.Any(x => x.Name.Equals("InsertBattleriteHere"));
        //         if (HyperspeedUpgrade)
        //         {
        //             return true;
        //         }
        //
        //         return false;
        //     }
        // }

        public Pestilus()
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
            if (args.WillHit || args.EndPosition.Distance(Utility.Player) <= Q.Range)
            {
                if (!HeroPlayer.HasBuff("QueenRecastBuff") && Q.CanCast && AntiGapclosing.GetBoolean("gapclosing.Q"))
                {
                    IsAntiGapclosing = true;
                    LocalPlayer.EditAimPosition = true;
                    LocalPlayer.PressAbility(Q.Slot, true);
                    LocalPlayer.Aim(args.EndPosition);
                    IsAntiGapclosing = false;
                    LocalPlayer.EditAimPosition = false;
                }
            }

            if (args.WillHit || args.EndPosition.Distance(Utility.Player) <= EX1.Range)
            {
                if (EX1.CanCast && AntiGapclosing.GetBoolean("gapclosing.EX1"))
                {
                    IsAntiGapclosing = true;
                    LocalPlayer.EditAimPosition = true;
                    LocalPlayer.PressAbility(EX1.Slot, true);
                    LocalPlayer.Aim(args.StartPosition);
                    IsAntiGapclosing = false;
                    LocalPlayer.EditAimPosition = false;
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

            if (HeroPlayer.IsMounted)
            {
                Drawing.DrawString(new Vector2(1920f / 2f, (1080f / 2f) - 45f).ScreenToWorld(),
                    "Nice mount bro :^)", Color.yellow);
            }

            if (!Game.IsInRoundPhase)
            {
                Drawing.DrawString(new Vector2(1920f / 2f, (1080f / 2f) - 5f).ScreenToWorld(),
                    "don't feed :-)", Color.white);
            }

            if (Drawings.GetBoolean("draw.rangeM1.safeRange"))
            {
                Drawing.DrawCircle(Utility.MyPos, M1.Range, UnityEngine.Color.white);
            }

            if (Drawings.GetBoolean("draw.Target"))
            {
                Drawing.DrawCircle(LocalPlayer.AimPosition, 1f, Color.green);
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInRoundPhase || HeroPlayer.IsMounted)
            {
                return;
            }

            if (Keys.GetBoolean("keys.autoCombo") && IsInInfest ||
                Keys.GetKeybind("keys.M1") || Keys.GetKeybind("keys.M2") || Keys.GetKeybind("keys.Q") ||
                 Keys.GetKeybind("keys.Space") || Keys.GetKeybind("keys.R"))
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

                var alliesToTargetBase =
                    EntitiesManager.LocalTeam.Where(x => !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial);
                var alliesToTargetHeal = EntitiesManager.LocalTeam.Where(x =>
                    !x.IsLocalPlayer && !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial);

                var enemiesToTargetProjs = enemiesToTargetBase.Where(x =>
                    !x.IsCountering && !x.HasConsumeBuff && !x.HasBuff("ElectricShield") &&
                    !x.HasBuff("BarbedHuskBuff") && !x.HasBuff("BulwarkBuff") && !x.HasBuff("DivineShieldBuff") &&
                    !x.HasBuff("GustBuff") && !x.HasBuff("TimeBenderBuff"));

                var M2Damage = 30;
                var EX2Damage = 50;
                var M1Heal = 6;
                var M2Heal = 26;
                var FHeal = 30;

                var enemiesToExecuteM2 = enemiesToTargetProjs.Where(x => x.Living.Health <= M2Damage);
                var enemiesToExecuteEX2 = enemiesToTargetProjs.Where(x => x.Living.Health <= EX2Damage);
                var alliesToTargetM1 = alliesToTargetHeal.Where(x =>
                    HeroPlayer.Living.Health < HeroPlayer.Living.MaxRecoveryHealth - M1Heal || !x.HasBuff("MothBuff") ||
                    x.Living.Health < x.Living.MaxRecoveryHealth - M1Heal * 2);
                var alliesToTargetM2 =
                    alliesToTargetHeal.Where(x => x.Living.Health < x.Living.MaxRecoveryHealth - M2Heal);
                var alliesToTargetF =
                    alliesToTargetHeal.Where(x => x.Living.Health < x.Living.MaxRecoveryHealth - FHeal);

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

                // var queenToTargetBase = EntitiesManager.LocalTeam.Where(x => x.ObjectName == "Queen" && x.IsValid && x.IsActiveObject);

                // var targetSelf = TargetSelector.GetTarget(alliesSelf, targetMode, 1f);
                var M1Allies = TargetSelector.GetTarget(alliesToTargetM1, TargetingMode.NearMouse, M1.Range);
                var M1Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M1.Range);
                var M2Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M2.Range);
                var M2Allies = TargetSelector.GetTarget(alliesToTargetM2, TargetingMode.NearMouse, M2.Range);
                // var M2Queen = TargetSelector.GetTarget(queenToTargetBase, TargetingMode.NearMouse, M2.Range);
                var ETarget = TargetSelector.GetTarget(enemiesToTargetBase, TargetingMode.NearMouse, E.Range);
                var RAllies = TargetSelector.GetTarget(alliesToTargetR, TargetingMode.NearMouse, R.Range);
                var FAllies = TargetSelector.GetTarget(alliesToTargetF, TargetingMode.NearMouse, F.Range);
                var FTarget = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, F.Range);
                var EX1Target = TargetSelector.GetTarget(enemiesToTargetBase, TargetingMode.NearMouse, EX1.Range);
                var EX2Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, EX2.Range);
                var M2Execute = TargetSelector.GetTarget(enemiesToExecuteM2, TargetingMode.NearMouse, M2.Range);
                var EX2Execute = TargetSelector.GetTarget(enemiesToExecuteEX2, TargetingMode.NearMouse, EX2.Range);

                if (!Channeling)
                {
                    LocalPlayer.EditAimPosition = false;
                    CastingAbility = null;
                    if (IsInUltimate)
                    {
                        LocalPlayer.PressAbility(M1.Slot, true);
                        CastingAbility = F;
                        return;
                    }

                    if (RAllies != null && R.CanCast && Combo.GetBoolean("combo.useR"))
                    {
                        {
                            LocalPlayer.PressAbility(R.Slot, true);
                            CastingAbility = R;
                            return;
                        }
                    }
                    if (EX2Execute != null && EX2.CanCast && M2.CanCast && Misc.GetBoolean("misc.useM2exec"))
                    {
                        LocalPlayer.PressAbility(EX2.Slot, true);
                        CastingAbility = M2;
                        return;
                    }

                    if (M2Execute != null && M2.CanCast && Misc.GetBoolean("misc.useM2exec"))
                    {
                        LocalPlayer.PressAbility(M2.Slot, true);
                        CastingAbility = M2;
                        return;
                    }

                    if (FTarget != null && F.CanCast && M1.CanCast && Combo.GetBoolean("combo.useF"))
                    {
                        LocalPlayer.PressAbility(F.Slot, true);
                        CastingAbility = F;
                        return;
                    }

                    if (FAllies != null && F.CanCast && M1.CanCast && Combo.GetBoolean("combo.useF"))
                    {
                        LocalPlayer.PressAbility(F.Slot, true);
                        CastingAbility = F;
                        return;
                    }

                    if (M2Target != null && M2.CanCast && M2Target.HasBuff("BrainBugDebuff") && Combo.GetBoolean("combo.useM2"))
                    {
                        LocalPlayer.PressAbility(M2.Slot, true);
                        CastingAbility = M2;
                        return;
                    }

                    // if (M2Queen != null && M2.CanCast)
                    // {
                    //     LocalPlayer.PressAbility(M2.Slot, true);
                    //     CastingAbility = M2;
                    //     return;
                    // }

                    if (HeroPlayer.EnemiesAroundAlive(2.5f) > 0 && Q.CanCast && Misc.GetBoolean("misc.useQ.melee"))
                    {
                        LocalPlayer.Aim(HeroPlayer.MapObject.Position);
                        LocalPlayer.PressAbility(Q.Slot, true);
                        CastingAbility = Q;
                        return;
                    }

                    if (EX1Target != null && EX1.CanCast && Combo.GetBoolean("combo.useEX1"))
                    {
                        var energyRequired = Combo.GetIntSlider("combo.useEX1.minEnergyBars") * 25;
                        if (energyRequired <= HeroPlayer.Energized.Energy)
                        {
                            LocalPlayer.PressAbility(EX1.Slot, true);
                            CastingAbility = EX1;
                            return;
                        }
                    }

                    if (EX2Target != null & EX2.CanCast && M2.CanCast && !F.CanCast && Combo.GetBoolean("combo.useEX2"))
                    {
                        var energyRequired = Combo.GetIntSlider("combo.useEX2.minEnergyBars") * 25;
                        if (energyRequired <= HeroPlayer.Energized.Energy)
                        {
                            LocalPlayer.PressAbility(EX2.Slot, true);
                            CastingAbility = EX2;
                            return;
                        }
                    }

                    if (ETarget != null && ETarget.Distance(HeroPlayer) >= M1.Range / 2 && E.CanCast && Combo.GetBoolean("combo.useE"))
                    {
                        LocalPlayer.PressAbility(E.Slot, true);
                        CastingAbility = E;
                        return;
                    }

                    if (M2Allies != null && M2.CanCast && Combo.GetBoolean("combo.useM2"))
                    {
                        LocalPlayer.PressAbility(M2.Slot, true);
                        CastingAbility = M2;
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
                        return;
                    }

                    if (M1Allies != null && M1.CanCast && Combo.GetBoolean("combo.useM1"))
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
                                    else
                                        LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                                }
                                else if (FAllies != null)
                                {
                                    var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, FAllies, F.Range, F.Speed, F.Radius, true);
                                    if (pred.CanHit)
                                        LocalPlayer.Aim(pred.CastPosition);
                                }
                                else
                                {
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                                }
                                break;
                        }
                    }

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
                            if (EX1Target != null)
                            {
                                LocalPlayer.Aim(EX1Target.MapObject.Position);
                            }
                            break;

                        case AbilityKey.E:
                            if (ETarget != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, ETarget, E.Range, E.Speed, E.Radius);
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
                            break;

                        case AbilityKey.M2:
                            // if (M2Queen != null)
                            // {
                            //     var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, M2Queen, M2.Range, M2.Speed, M2.Radius, true);
                            //     if (pred.CanHit)
                            //         LocalPlayer.Aim(pred.CastPosition);
                            //     else if (Combo.GetBoolean("combo.interruptM2"))
                            //         LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            // }
                            if (M2Allies != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, M2Allies, M2.Range, M2.Speed, M2.Radius, true);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else if (Combo.GetBoolean("combo.interruptM2"))
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            else if (M2Target != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, M2Target, M2.Range, M2.Speed, M2.Radius, true);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else if (Combo.GetBoolean("combo.interruptM2"))
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            else if (Combo.GetBoolean("combo.interruptM2"))
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
                            else if (M1Allies != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, M1Allies, M1.Range, M1.Speed, M1.Radius, true);
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
            MainMenu.GetMenu("kavey_series").Get<Menu>("pestilus").Hidden = true;
            Game.OnUpdate -= Game_OnUpdate;
            Game.OnDraw -= Game_OnDraw;
            AntiGapcloser.OnGapcloser -= AntiGapcloser_OnGapcloser;
        }

        // InGameObject.OnCreate -= OnCreate;
        // InGameObject.OnDestroy -= OnDestroy;
    }
}
