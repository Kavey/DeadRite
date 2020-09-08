using System;
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
using Kavey_Series.AntiGapclose;
using Kavey_Series.Prediction;
using Kavey_Series.Utilities;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Vector2 = BattleRight.Core.Math.Vector2;

namespace Kavey_Series.Champions
{
    class Alysia : IChampion
    {
        public bool Initialized { get; set; }
        public string ChampionName { get; set; } = "Alysia";
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
        public Menu Drawings { get; set; }

        public Ability CastingAbility { get; set; }

        private static bool Channeling => Utility.Player.IsChanneling || Utility.Player.AbilitySystem.IsCasting;
        private static bool HasVandalism => Utility.Battlerites.Any(x => x.Name == "ShatteringBarrierUpgrade");
        private static bool IsAntiGapclosing;

        private static bool IsInTheAir
        {
            get
            {
                var SnowboardBuff = LocalPlayer.Instance.HasBuff("Snowboard");
                var LastMinuteBuff = LocalPlayer.Instance.HasBuff("LastMinute");
                if (SnowboardBuff || LastMinuteBuff)
                {
                    return true;
                }

                return false;
            }
        }

        public Ability GetAbilityFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                case 5:
                    return M1;
                case 2:
                    return M2;
                case 6:
                    return Space;
                case 7:
                    return Q;
                case 8:
                    return EX1;
                case 11:
                    return EX2;
                case 12:
                    return R;
                case 13:
                    return F;
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
                AntiGapcloser.OnGapcloser += AntiGapcloser_OnGapcloser;
            }
            Game.OnUpdate += Game_OnUpdate;
            Game.OnDraw += Game_OnDraw;
            MainMenu.GetMenu("kavey_series").Get<Menu>("alysia").Hidden = false;
            Utility.Log("[Kavey Series] {0} loaded!", ConsoleColor.Cyan, ChampionName);
            Initialized = true;
        }

        public void InitializeMenu()
        {
            var rootMenu = MainMenu.GetMenu("kavey_series");
            Menu = new Menu("alysia", "Alysia", true);
            //Keys
            {
                Keys = new Menu("alysia.keys", "Keys", true);
                Keys.Add(new MenuKeybind("keys.combo", "Combo", KeyCode.Mouse0));
                Keys.Add(new MenuCheckBox("keys.autoCombo", "Auto Combo", true));
                Keys.Add(new MenuKeybind("keys.toggleAiming", "Enable/Disable Aiming", UnityEngine.KeyCode.Y, true, true));
                Keys.Add(new MenuKeybind("keys.m1", "Left Mouse Keybind to stop Auto Combo", KeyCode.Mouse2));
                Keys.Add(new MenuKeybind("keys.q", "Q Keybind to stop Auto Combo", KeyCode.Alpha1));
                Keys.Add(new MenuKeybind("keys.e", "E Keybind to stop Auto Combo", KeyCode.Alpha2));
                Keys.Add(new MenuKeybind("keys.space", "Space Keybind to stop Auto Combo", KeyCode.Space));
                Keys.Add(new MenuKeybind("keys.ex1", "EX1 Keybind to stop Auto Combo", KeyCode.T));
            }
            //Combo
            {
                Combo = new Menu("alysia.combo", "Combo", true);
                Combo.Add(new MenuCheckBox("combo.invisible", "Attack invisible targets"));
                Combo.Add(new MenuCheckBox("combo.m2", "Use M2"));
                Combo.Add(new MenuCheckBox("combo.m2.cc", "     ^Only if target is CC'd", false));
                Combo.Add(new MenuCheckBox("combo.qallies", "Use Q self on allies in melee range"));
                Combo.Add(new MenuCheckBox("combo.e", "Use E"));
                Combo.Add(new MenuCheckBox("combo.e.chilled", "     ^Also if target is chilled", false));
                Combo.Add(new MenuCheckBox("combo.e.cc", "     ^Also if target is CC'd", false));
                Combo.Add(new MenuCheckBox("combo.space", "Space to escape (cast to ally)", false));
                Combo.Add(new MenuCheckBox("combo.r", "Use R"));
                Combo.Add(new MenuSlider("combo.r.energy", "     ^Minimum energy", 2, 4, 1));
                Combo.Add(new MenuCheckBox("combo.f", "Use F"));
                Combo.Add(new MenuSlider("combo.f.enemies", "     ^Enemies to hit", 2, 3, 1));
            }
            //Anti-Gapclosing
            {
                AntiGapclosing = new Menu("alysia.gapclosing", "Anti-Gapclosing", true);
                AntiGapclosing.Add(new MenuCheckBox("gapclosing.space", "Dodge with Space", false));
                AntiGapclosing.Add(new MenuCheckBox("gapclosing.r", "Block with R"));
                AntiGapclosing.Add(new MenuSlider("gapclosing.r.energy", "     ^Minimum energy", 2, 4, 1));
            }
            //Drawings
            {
                Drawings = new Menu("alysia.drawings", "Drawings", true);
                Drawings.Add(new MenuCheckBox("drawings.m1", "Draw M1", false));
                Drawings.Add(new MenuCheckBox("drawings.m2", "Draw M2", false));
            }
            //Finally
            {
                Menu.Add(Keys);
                Menu.Add(Combo);
                Menu.Add(AntiGapclosing);
                Menu.Add(Drawings);
                rootMenu.Add(Menu);
            }
        }

        public void InitializeAbilities()
        {
            M1 = new Ability(AbilityKey.M1, 7.4f, 15.8f, 0.25f);
            M2 = new Ability(AbilityKey.M2, 9.5f, 22f, 0.35f);
            EX1 = new Ability(AbilityKey.EX1);
            EX2 = new Ability(AbilityKey.EX2, 6f, float.MaxValue, 2.5f);
            Space = new Ability(AbilityKey.Space, 10f, float.MaxValue, 2f);
            Q = new Ability(AbilityKey.Q, 3f);
            E = new Ability(AbilityKey.E, 10f, float.MaxValue, 2.2f, SkillType.Circle);
            R = new Ability(AbilityKey.R, 7.5f, 3.5f, 0.6f);
            F = new Ability(AbilityKey.F, 12f, float.MaxValue, 4f);
        }

        public Alysia()
        {
            Initialize();
        }

        private void AntiGapcloser_OnGapcloser(AntiGapcloser.GapcloseEventArgs args)
        {
            if (args.WillHit || args.EndPosition.Distance(Utility.Player) <= R.Range)
            {
                if (Space.CanCast && AntiGapclosing.Get<MenuCheckBox>("gapclosing.space"))
                {
                    IsAntiGapclosing = true;
                    LocalPlayer.EditAimPosition = true;
                    LocalPlayer.PressAbility(Space.Slot, true);
                    var safePos = Utility.SafestPosition(args.EndPosition, Space.Range);
                    LocalPlayer.Aim(safePos);
                    Utility.DelayAction(() =>
                    {
                        IsAntiGapclosing = false;
                        LocalPlayer.EditAimPosition = false;
                    }, 0.15f);
                    return;
                }
                if (R.CanCast && AntiGapclosing.Get<MenuCheckBox>("gapclosing.r") && Utility.MyEnergy >= AntiGapclosing.Get<MenuSlider>("gapclosing.r.energy").CurrentValue * 25)
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
            Drawing.DrawString(new Vector2(1920f / 2f, 1080f / 2f - 5f).ScreenToWorld(),
            "Aiming: " + (Keys.GetKeybind("keys.toggleAiming") ? "ON" : "OFF"), Color.white);

            if (Drawings.Get<MenuCheckBox>("drawings.m1"))
                Drawing.DrawCircle(Utility.MyPos, M1.Range, Color.white);
            if (Drawings.Get<MenuCheckBox>("drawings.m2"))
                Drawing.DrawCircle(Utility.MyPos, M2.Range, Color.magenta);
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInRoundPhase)
            {
                return;
            }

            if (LocalPlayer.Instance.HasBuff("IceBlock") || IsInTheAir || Keys.GetBoolean("keys.autoCombo") &&
                (Keys.GetKeybind("keys.m1") || Keys.GetKeybind("keys.space") || Keys.GetKeybind("keys.q") ||
                                                                    Keys.GetKeybind("keys.e") || Keys.GetKeybind("keys.ex1")))
            {
                LocalPlayer.EditAimPosition = false;
                CastingAbility = null;
                return;
            }

            if (IsAntiGapclosing)
                return;
            LocalPlayer.EditAimPosition = false;
            if (Keys.Get<MenuKeybind>("keys.combo") || Keys.Get<MenuCheckBox>("keys.autoCombo"))
            {
                var toggle = Keys.GetKeybind("keys.toggleAiming");
                var toggleAiming = toggle ? true : false;

                var enemiesToTargetBase = EntitiesManager.EnemyTeam.Where(x => x.IsValid && !x.Living.IsDead && !x.IsTraveling && !x.IsDashing &&
                   !x.HasBuff("ArenaJumpPadSkyDive") && !x.HasBuff("OtherSideBuff") && !x.HasBuff("AscensionBuff") && !x.HasBuff("AscensionTravelBuff") && !x.HasBuff("WarStomp") &&
                   !x.HasBuff("ValiantLeap") && !x.HasBuff("FrogLeap") && !x.HasBuff("FrogLeapRecast") && !x.HasBuff("ElusiveStrikeCharged") && !x.HasBuff("ExtendedShieldDash") &&
                   !x.HasBuff("ElusiveStrikeWall2") && !x.HasBuff("BurrowAlternate") && !x.HasBuff("JetPack") && !x.HasBuff("ProwlBuff") && !x.HasBuff("Fleetfoot") &&
                   !x.HasBuff("Dive") && !x.HasBuff("InfestingBuff") && !x.HasBuff("PortalBuff") && !x.HasBuff("CrushingBlow") && !x.HasBuff("TempestRushBuff") &&
                   !x.HasBuff("TornadoBuff") && !x.HasBuff("LawBringerInAir") && !x.HasBuff("LawBringerLeap") && !x.HasBuff("JawsSwallowedDebuff") && !x.HasBuff("IceBlock"));

                if (!Combo.Get<MenuCheckBox>("combo.invisible"))
                {
                    enemiesToTargetBase = enemiesToTargetBase.Where(x => !x.CharacterModel.IsModelInvisible);
                }

                var enemiesToTargetProjs = enemiesToTargetBase.Where(x =>
                    !x.IsCountering && !x.HasConsumeBuff && !x.HasBuff("ElectricShield") &&
                    !x.HasBuff("BarbedHuskBuff") && !x.HasBuff("BulwarkBuff") && !x.HasBuff("DivineShieldBuff") &&
                    !x.HasBuff("GustBuff") && !x.HasBuff("TimeBenderBuff"));


                var MeleeRange = 2.5f;

                var selfToTarget = EntitiesManager.LocalTeam.Where(x => x.IsLocalPlayer && !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial);
                var alliesToTarget = EntitiesManager.LocalTeam.Where(x => !x.Living.IsDead && !x.PhysicsCollision.IsImmaterial);
                var alliesETarget = TargetSelector.GetTarget(alliesToTarget, TargetingMode.NearMouse, M2.Range);

                var enemiesToTargetA = enemiesToTargetBase.Where(x => x.Distance(alliesETarget) <= MeleeRange + 0.5f);
                var enemiesToTargetEA = enemiesToTargetBase.Where(x => x.Distance(alliesETarget) <= E.Radius * 1.5);

                var M1Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M1.Range);
                var M2Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M2.Range);
                var QAllies = TargetSelector.GetTarget(enemiesToTargetA, TargetingMode.NearMouse, Q.Range);
                var ETarget = TargetSelector.GetTarget(enemiesToTargetBase, TargetingMode.NearMouse, E.Range);
                var EATarget = TargetSelector.GetTarget(enemiesToTargetEA, TargetingMode.NearMouse, E.Range);
                var SpaceTarget = EntitiesManager.EnemyTeam.Where(x => x.Distance(Utility.Player) <= Space.Radius).OrderBy(x => x.Distance(Utility.Player)).FirstOrDefault();
                var RTarget = TargetSelector.GetTarget(enemiesToTargetProjs.Where(x => !x.IsChilled() && !x.IsFrozen()), TargetingMode.NearMouse, R.Range);
                var FTarget = TargetSelector.GetTarget(enemiesToTargetBase, TargetingMode.NearMouse, F.Range);


                if (!Channeling)
                {
                    LocalPlayer.EditAimPosition = false;
                    CastingAbility = null;
                    if (SpaceTarget != null && Space.CanCast && Combo.Get<MenuCheckBox>("combo.space"))
                    {
                        IsAntiGapclosing = true;
                        LocalPlayer.EditAimPosition = true;
                        LocalPlayer.PressAbility(Space.Slot, true);
                        var safePos = Utility.SafestPosition(SpaceTarget, Space.Range);
                        LocalPlayer.Aim(safePos);
                        Utility.DelayAction(() =>
                        {
                            IsAntiGapclosing = false;
                            LocalPlayer.EditAimPosition = false;
                        }, 0.15f);
                        return;
                    }
                    if (QAllies != null && Q.CanCast && Combo.Get<MenuCheckBox>("combo.qallies"))
                    {
                        LocalPlayer.PressAbility(Q.Slot, true);
                        CastingAbility = Q;
                        return;
                    }
                    if (FTarget != null && F.CanCast && Combo.Get<MenuCheckBox>("combo.f"))
                    {
                        var enemiesWillHit = F.EnemiesWillHit(FTarget);
                        if (enemiesWillHit.Count >= Combo.Get<MenuSlider>("combo.f.enemies").CurrentValue)
                        {
                            LocalPlayer.PressAbility(F.Slot, true);
                            CastingAbility = F;
                            return;
                        }
                    }
                    if (RTarget != null && R.CanCast && Combo.Get<MenuCheckBox>("combo.r") && Utility.MyEnergy >= Combo.Get<MenuSlider>("combo.r.energy").CurrentValue * 25)
                    {
                        LocalPlayer.PressAbility(R.Slot, true);
                        CastingAbility = R;
                        return;
                    }
                    if (ETarget != null && E.CanCast && Combo.Get<MenuCheckBox>("combo.e") && (Combo.Get<MenuCheckBox>("combo.e.chilled") || Combo.Get<MenuCheckBox>("combo.e.cc")))
                    {
                        if (ShouldCastE(ETarget))
                        {
                            LocalPlayer.PressAbility(E.Slot, true);
                            CastingAbility = E;
                            return;
                        }
                    }
                    if (EATarget != null && E.CanCast && Combo.Get<MenuCheckBox>("combo.e"))
                    {
                        LocalPlayer.PressAbility(E.Slot, true);
                        CastingAbility = E;
                        return;
                    }
                    if (M2Target != null && M2.CanCast && Combo.Get<MenuCheckBox>("combo.m2"))
                    {
                        var shouldCast = true;
                        if (Combo.Get<MenuCheckBox>("combo.m2.cc"))
                            shouldCast = M2Target.IsCCd();
                        if (shouldCast)
                        {
                            LocalPlayer.PressAbility(M2.Slot, true);
                            CastingAbility = M2;
                            return;
                        }
                    }
                    if (M1Target != null && M1.CanCast)
                    {
                        LocalPlayer.PressAbility(M1.Slot, true);
                        CastingAbility = M1;
                    }
                }
                else
                {
                    if (!toggleAiming)
                        return;
                    if (CastingAbility == null)
                        CastingAbility = GetAbilityFromIndex(Utility.Player.AbilitySystem.CastingAbilityIndex);
                    if (CastingAbility == null)
                        return;
                    LocalPlayer.EditAimPosition = true;
                    switch (CastingAbility.Key)
                    {
                        case AbilityKey.M1:
                            if (M1Target != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, M1Target, M1.Range, M1.Speed, M1.Radius, true);
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
                        case AbilityKey.Q:
                            if (QAllies != null)
                            {
                                LocalPlayer.Aim(QAllies.MapObject.Position);
                            }
                            else
                            {
                                LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            break;
                        case AbilityKey.E:
                            if (ETarget != null && ShouldCastE(ETarget))
                            {
                                var pred = TestPrediction.GetPrediction(Utility.MyPos, ETarget, E.Range, E.Speed, E.Radius);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            else if (EATarget != null)
                            {
                                var pred = TestPrediction.GetPrediction(Utility.MyPos, EATarget, E.Range, E.Speed, E.Radius);
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
                        case AbilityKey.R:
                            if (RTarget != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, RTarget, R.Range, R.Speed, R.Radius);
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
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, FTarget, F.Range, F.Speed, F.Radius);
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
        public bool ShouldCastE(Character target)
        {
            var shouldCast = true;
            if (Combo.Get<MenuCheckBox>("combo.e.chilled"))
                shouldCast = target.IsChilled();
            if (shouldCast)
            {
                if (Combo.Get<MenuCheckBox>("combo.e.cc"))
                    shouldCast = target.IsCCd();
            }
            return shouldCast;
        }
        public void Destroy()
        {
            MainMenu.GetMenu("kavey_series").Get<Menu>("alysia").Hidden = true;
            Game.OnUpdate -= Game_OnUpdate;
            Game.OnDraw -= Game_OnDraw;
            AntiGapcloser.OnGapcloser -= AntiGapcloser_OnGapcloser;
        }
    }
}
