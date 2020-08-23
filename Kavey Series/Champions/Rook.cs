using System;
using System.Linq;
using BattleRight.Core;
using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.SDK;
using BattleRight.SDK.Enumeration;
using BattleRight.SDK.UI;
using BattleRight.SDK.UI.Models;
using BattleRight.SDK.UI.Values;
using Kavey_Series.Abilities;
using Kavey_Series.Prediction;
using Kavey_Series.Utilities;
using UnityEngine;

namespace Kavey_Series.Champions
{
    class Rook : IChampion
    {
        public bool Initialized { get; set; }
        public string ChampionName { get; set; } = "Rook";
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
        private static Character HeroPlayer => LocalPlayer.Instance;

        private static bool Channeling => Utility.Player.IsChanneling || Utility.Player.AbilitySystem.IsCasting;
        private static bool IsBerserk => Utility.Player.Buffs.Any(x => x.ObjectName == "BerserkBuff");

        public Ability GetAbilityFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                case 1:
                    return M1;
                case 2:
                    return M2;
                case 3:
                    return EX2;
                case 4:
                    return Space;
                case 12:
                    return E;
                case 13:
                    return R;
                case 14:
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
            }
            Game.OnUpdate += Game_OnUpdate;
            MainMenu.GetMenu("kavey_series").Get<Menu>("rook").Hidden = false;
            Utility.Log("[Kavey Series] {0} loaded!", ConsoleColor.Gray, ChampionName);
            Initialized = true;
        }

        public void InitializeMenu()
        {
            var rootMenu = MainMenu.GetMenu("kavey_series");
            Menu = new Menu("rook", "Rook", true);
            //Keys
            {
                Keys = new Menu("rook.keys", "Keys", true);
                Keys.Add(new MenuKeybind("keys.combo", "Combo", KeyCode.Mouse3));
                Keys.Add(new MenuCheckBox("keys.autoCombo", "Auto Combo"));
            }
            //Combo
            {
                Combo = new Menu("rook.combo", "Combo", true);
                Combo.Add(new MenuCheckBox("combo.invisible", "Attack invisible targets"));
                Combo.Add(new MenuCheckBox("combo.heal", "Heal"));
                Combo.Add(new MenuCheckBox("combo.m2", "Use M2"));
                Combo.Add(new MenuCheckBox("combo.m2.ex", "Use EX-M2"));
                Combo.Add(new MenuSlider("combo.m2.ex.minenergy", "     ^Minimum energy", 2f, 4f, 1f));
                Combo.Add(new MenuCheckBox("combo.space", "Use Space"));
                Combo.Add(new MenuCheckBox("combo.space.checkdist", "     ^Don't cast if close"));
                Combo.Add(new MenuCheckBox("misc.useQ", "Try to counter melees with Q", true));
                Combo.Add(new MenuCheckBox("combo.e", "Use E"));
                Combo.Add(new MenuCheckBox("combo.e.checkdist", "     ^Don't cast if close"));
                Combo.Add(new MenuCheckBox("combo.r", "Use R"));
                Combo.Add(new MenuCheckBox("combo.f", "Use F"));
            }
            //Finally
            {
                Menu.Add(Keys);
                Menu.Add(Combo);
                rootMenu.Add(Menu);
            }
        }

        public void InitializeAbilities()
        {
            M1 = new Ability(AbilityKey.M1, 2.6f);
            M2 = new Ability(AbilityKey.M2, 3f, 10f, 1.90f, SkillType.Circle);
            EX1 = new Ability(AbilityKey.EX1);
            EX2 = new Ability(AbilityKey.EX2, 10f, 25f, 0.5f);
            Q = new Ability(AbilityKey.Q, 10f);
            Space = new Ability(AbilityKey.Space, 10f, 25f, 0.5f);
            E = new Ability(AbilityKey.E, 10f, 20f, 2.35f, SkillType.Circle);
            R = new Ability(AbilityKey.R, 4f, float.MaxValue, 3f, SkillType.Circle);
            F = new Ability(AbilityKey.F, 10f, float.MaxValue, 1f);
        }

        public Rook()
        {
            Initialize();
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInRoundPhase)
            {
                return;
            }

            if (Keys.Get<MenuKeybind>("keys.combo") || Keys.Get<MenuCheckBox>("keys.autoCombo"))
            {
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

                var MeleeRange = 2.5f;

                var enemiesToTargetProjs = enemiesToTargetBase.Where(x =>
                    !x.IsCountering && !x.HasConsumeBuff && !x.HasBuff("ElectricShield") && 
                    !x.HasBuff("BarbedHuskBuff") && !x.HasBuff("BulwarkBuff") && !x.HasBuff("DivineShieldBuff") &&
                    !x.HasBuff("GustBuff") && !x.HasBuff("TimeBenderBuff"));

                var selfToTargetQ = enemiesToTargetBase.Where(x => (x.ChampionEnum == Champion.Bakko || x.ChampionEnum == Champion.Croak ||
                                                                            x.ChampionEnum == Champion.Freya || x.ChampionEnum == Champion.Jamila ||
                                                                            x.ChampionEnum == Champion.Raigon || x.ChampionEnum == Champion.Rook ||
                                                                            x.ChampionEnum == Champion.RuhKaan || x.ChampionEnum == Champion.Shifu ||
                                                                            x.ChampionEnum == Champion.Thorn)
                                                   && x.Distance(HeroPlayer) <= MeleeRange && x.AbilitySystem.IsCasting);

                var M1Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M1.Range);
                var M2Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, M2.Range);
                var EXM2Target = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, EX2.Range);
                var QSelf = TargetSelector.GetTarget(selfToTargetQ, TargetingMode.NearMouse, Q.Range);
                var SpaceTarget = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, Space.Range);
                var ETarget = TargetSelector.GetTarget(enemiesToTargetBase, TargetingMode.NearMouse, E.Range);
                var RTarget = TargetSelector.GetTarget(enemiesToTargetProjs, TargetingMode.NearMouse, R.Range);
                var FTarget = TargetSelector.GetTarget(enemiesToTargetBase, TargetingMode.NearMouse, F.Range);

                if (!Channeling)
                {
                    LocalPlayer.EditAimPosition = false;
                    CastingAbility = null;
                    if (FTarget != null && F.CanCast && Combo.Get<MenuCheckBox>("combo.f"))
                    {
                        LocalPlayer.PressAbility(F.Slot, true);
                        CastingAbility = F;
                        return;
                    }

                    if (QSelf != null && Q.CanCast && Combo.GetBoolean("misc.useQ"))
                    {
                        LocalPlayer.PressAbility(Q.Slot, true);
                        CastingAbility = Q;
                        return;
                    }

                    if (EX1.CanCast && Combo.Get<MenuCheckBox>("combo.heal") && !IsBerserk)
                    {
                        var IsEating = LocalPlayer.Instance.HasBuff("EatBuff");
                        var Fish = 30;
                        var EatFish = !IsEating && (LocalPlayer.Instance.Living.Health <= (LocalPlayer.Instance.Living.MaxRecoveryHealth - Fish));
                        if (EatFish)
                        {
                            LocalPlayer.PressAbility(AbilitySlot.EXAbility1, true);
                            return;
                        }
                    }

                    if (ETarget != null && E.CanCast && Combo.Get<MenuCheckBox>("combo.e") && !IsBerserk)
                    {
                        if (Combo.Get<MenuCheckBox>("combo.e.checkdist"))
                        {
                            if (ETarget.Distance(Utility.Player) >= 7f)
                            {
                                LocalPlayer.PressAbility(E.Slot, true);
                                CastingAbility = E;
                                return;
                            }
                        }
                        else
                        {
                            LocalPlayer.PressAbility(E.Slot, true);
                            CastingAbility = E;
                            return;
                        }
                    }
                    if (EXM2Target != null && M2.CanCast && Combo.Get<MenuCheckBox>("combo.m2.ex") && !IsBerserk)
                    {
                        if (Utility.MyEnergy >= Combo.Get<MenuSlider>("combo.m2.ex.minenergy").CurrentValue * 25 && EXM2Target.Distance(Utility.Player) >= 5f)
                        {
                            LocalPlayer.PressAbility(M2.Slot, true);
                            CastingAbility = EX2;
                            return;
                        }
                    }
                    if (SpaceTarget != null && Space.CanCast && Combo.Get<MenuCheckBox>("combo.space") && !IsBerserk)
                    {
                        if (Combo.Get<MenuCheckBox>("combo.space.checkdist"))
                        {
                            if (SpaceTarget.Distance(Utility.Player) >= 5f)
                            {
                                LocalPlayer.PressAbility(Space.Slot, true);
                                CastingAbility = Space;
                                return;
                            }
                        }
                        else
                        {
                            LocalPlayer.PressAbility(Space.Slot, true);
                            CastingAbility = Space;
                            return;
                        }

                    }
                    if (M2Target != null && M2.CanCast && Combo.Get<MenuCheckBox>("combo.m2") && !IsBerserk)
                    {
                        LocalPlayer.PressAbility(M2.Slot, true);
                        CastingAbility = M2;
                        return;
                    }
                    if (RTarget != null && R.CanCast && Combo.Get<MenuCheckBox>("combo.r") && !IsBerserk)
                    {
                        LocalPlayer.PressAbility(R.Slot, true);
                        CastingAbility = R;
                        //var direction = (Utility.Player.MapObject.Position - RTarget.MapObject.Position).Normalized;
                        //var endPos = Utility.Player.MapObject.Position + (direction * 4f);
                        //Drawing.DrawLine(RTarget.MapObject.ScreenPosition, endPos.WorldToScreen(), Color.red);
                        return;
                    }
                    if (M1Target != null)
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
                    LocalPlayer.EditAimPosition = true;
                    switch (CastingAbility.Key)
                    {
                        case AbilityKey.M1:
                            if (M1Target != null)
                            {
                                LocalPlayer.Aim(M1Target.MapObject.Position);
                            }
                            else
                            {
                                LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            break;
                        case AbilityKey.M2:
                            if (M2Target != null)
                            {
                                var pred = TestPrediction.GetPrediction(Utility.MyPos, M2Target, M2.Range, M2.Speed, M2.Radius);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            break;
                        case AbilityKey.EX2:
                            if (EXM2Target != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, EXM2Target, EX2.Range, EX2.Speed, EX2.Radius, true);
                                if (pred.CanHit)
                                    LocalPlayer.Aim(pred.CastPosition);
                                else
                                    LocalPlayer.PressAbility(AbilitySlot.Interrupt, true);
                            }
                            break;
                        case AbilityKey.Space:
                            if (SpaceTarget != null)
                            {
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, SpaceTarget, Space.Range, Space.Speed, Space.Radius, true);
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
                                var pred = TestPrediction.GetPrediction(Utility.MyPos, ETarget, E.Range, E.Speed, E.Radius);
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
                                var pred = TestPrediction.GetNormalLinePrediction(Utility.MyPos, FTarget, F.Range, F.Speed, F.Radius, true);
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
                CastingAbility = null;
                LocalPlayer.EditAimPosition = false;
            }
        }

        public void Destroy()
        {
            MainMenu.GetMenu("kavey_series").Get<Menu>("rook").Hidden = true;
            Game.OnUpdate -= Game_OnUpdate;
        }
    }
}