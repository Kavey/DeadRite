﻿using System.Collections.Generic;
using System.Linq;
using BattleRight.Core;
using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.Core.GameObjects.Models;
using BattleRight.Core.Math;
using BattleRight.SDK;
using Hoyer.Base.Extensions;
using Hoyer.Base.Prediction;

// ReSharper disable InlineOutVariableDeclaration

namespace Hoyer.Champions.Varesh.Systems
{
    public static class Casting
    {
        public static void CastLogic()
        {
            if (MenuHandler.SkillBool("close_a3") && EnemiesInRange(2.5f).Count > 0)
            {
                if (AbilitySlot.Ability3.IsReady())
                {
                    Cast(AbilitySlot.Ability3);
                    return;
                }
            }

            if (OrbLogic()) return;

            var enemyTeam = EntitiesManager.EnemyTeam;
            if (TargetSelection.UseMaxCursorDist)
            {
                enemyTeam = enemyTeam.Where(e => e.Pos().Distance(Base.Main.MouseWorldPos) <= TargetSelection.MaxCursorDist).ToArray();
            }
            var validEnemies = enemyTeam.Where(e => e.IsValidTarget() && e.Pos().Distance(Vector2.Zero) > 0.3f).ToList();

            if (validEnemies.Any())
            {
                var anyValidForProjectiles = validEnemies.Any(e => e.IsValidTargetProjectile());
                var anyValidForBigProjectiles = validEnemies.Any(e => e.IsValidTargetProjectile(true));
                bool anyWithCorruption, anyWithJudgement;
                Main.BuffCheck(validEnemies, out anyWithCorruption, out anyWithJudgement);

                var closestRange = validEnemies.OrderBy(e => e.Distance(LocalPlayer.Instance)).First().Distance(LocalPlayer.Instance);

                if (MenuHandler.UseSkill(AbilitySlot.Ability5) && AbilitySlot.Ability5.IsReadyHasCharges() && AbilitySlot.Ability5.InRange(closestRange) && 
                    anyWithJudgement)
                {
                    Cast(AbilitySlot.Ability5);
                    return;
                }

                if (MenuHandler.UseSkill(AbilitySlot.Ability7) && AbilitySlot.Ability7.IsReady() && AbilitySlot.Ability7.InRange(closestRange))
                {
                    Cast(AbilitySlot.Ability7);
                    return;
                }

                if (MenuHandler.UseSkill(AbilitySlot.Ability2) && AbilitySlot.Ability2.IsReady() && AbilitySlot.Ability2.InRange(closestRange) &&
                    anyValidForBigProjectiles)
                {
                    if (EnemiesInRange(6).Count == 0)
                    {
                        Cast(AbilitySlot.Ability2);
                        return;
                    }
                }

                if (MenuHandler.UseSkill(AbilitySlot.Ability5) && AbilitySlot.Ability5.IsReadyHasCharges(2) && AbilitySlot.Ability5.InRange(closestRange) &&
                    anyWithCorruption)
                {
                    Cast(AbilitySlot.Ability5);
                    return;
                }

                if (MenuHandler.UseSkill(AbilitySlot.EXAbility1) && AbilitySlot.Ability2.IsReady() &&
                    LocalPlayer.Instance.Energized.Energy >= 25 && AbilitySlot.EXAbility1.InRange(closestRange) && anyValidForBigProjectiles && !anyWithJudgement)
                {
                    if (!MenuHandler.SkillBool("save_a6") || LocalPlayer.Instance.Energized.Energy >= 50)
                    {
                        Cast(AbilitySlot.EXAbility1);
                        return;
                    }
                }
                if (MenuHandler.UseSkill(AbilitySlot.Ability1) && AbilitySlot.Ability1.InRange(closestRange) && anyValidForProjectiles)
                {
                    Cast(AbilitySlot.Ability1);
                    return;
                }
                Main.DebugOutput = "No valid targets";
            }
        }

        private static List<Character> EnemiesInRange(float distance)
        {
            return EntitiesManager.EnemyTeam.Where(e => e.Distance(LocalPlayer.Instance) < distance).ToList();
        }

        private static bool OrbLogic()
        {
            var orb = EntitiesManager.CenterOrb;
            if (orb == null || !orb.IsValid || !orb.IsActiveObject) return false;
            var livingObj = orb.Get<LivingObject>();
            if (livingObj.IsDead) return false;

            var orbMapObj = orb.Get<MapGameObject>();
            if (!TargetSelection.CursorDistCheck(orbMapObj.Position)) return false;
            if (livingObj.Health <= 14)
            {
                Cast(AbilitySlot.Ability1);
                return true;
            }
            if (livingObj.Health <= 22 && AbilitySlot.Ability2.IsReady())
            {
                Cast(AbilitySlot.Ability2);
                return true;
            }
            if (!orbMapObj.IsHoveringNear()) return false;
            Cast(AbilitySlot.Ability1);
            return true;
        }

        private static void Cast(AbilitySlot slot)
        {
            LocalPlayer.PressAbility(slot, true);
        }
    }
}