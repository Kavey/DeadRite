using System;
using System.Linq;
using BattleRight.Core;
using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.SDK;
using BattleRight.SDK.Enumeration;
using BattleRight.SDK.Events;
using BattleRight.SDK.EventsArgs;
using BattleRight.SDK.UI;
using BattleRight.SDK.UI.Values;
using Pearl.Utils;
using UnityEngine;
using Vector2 = BattleRight.Core.Math.Vector2;
using BattleRight.Sandbox;

namespace Pearl
{
    internal class Program : IAddon
    {
        private static float drawDebug;

        private static bool enabled;
        private static bool useQ;
        private static SkillBase RmbSkill = new SkillBase(AbilitySlot.Ability2, SkillType.Circle, 9.2f, 5.6f, 1.15f);
        private static SkillBase ESkill = new SkillBase(AbilitySlot.Ability5, SkillType.Circle, 10f, 4.4f, 2f);
        private static SkillBase QSkill = new SkillBase(AbilitySlot.Ability4, SkillType.Circle, 10f, 0, 0.7f);
        private static SkillBase LmbSkill = new SkillBase(AbilitySlot.Ability1, SkillType.Line, 8.6f, 8f, 0.6f);

        public static Projectile GetEnemyProjectiles(float worldDistance)
        {
            return EntitiesManager.ActiveProjectiles.FirstOrDefault(p =>
                p.TeamId != LocalPlayer.Instance.TeamId && p.Distance(LocalPlayer.Instance) <= worldDistance);
        }
        //TODO Maybe try to merge Heal Combo and Combo together

        private static void HealCombo()
        {
            var allyTarget = TargetSelectorHelper.GetAlly(TargetingMode.LowestHealth, 11f);

            if (allyTarget == null)
                return;

            var predictRMB = LocalPlayer.Instance.GetPrediction(allyTarget, RmbSkill.Speed, RmbSkill.Range, RmbSkill.SpellCollisionRadius, RmbSkill.SkillType);

            if (RmbSkill.Data.ChargeCount > 0 && !allyTarget.IsImmaterial && allyTarget.Health != allyTarget.MaxRecoveryHealth &&
                predictRMB.HitChancePercent >= 75f)
            {
                LocalPlayer.UpdateCursorPosition(predictRMB.MoveMousePosition);
                LocalPlayer.CastAbility(AbilitySlot.Ability2);
            }
        }
        private static void Combo()
        {
            var enemyTarget = TargetSelectorHelper.GetTarget(TargetingMode.NearLocalPlayer, 9f);
            var enemyProjectiles = GetEnemyProjectiles(10f);

            if (enemyTarget == null)
                return;

            if (enemyProjectiles == null)
            {
                return;
            }

            var hasBuff = Helper.HasBuff(LocalPlayer.Instance, "RecastBuff");
            var predictLMB = LocalPlayer.Instance.GetPrediction(enemyTarget, LmbSkill.Speed, LmbSkill.Range, LmbSkill.SpellCollisionRadius, LmbSkill.SkillType);
            var intersectingWithProjectile = Helper.IsProjectileColliding(LocalPlayer.Instance, enemyProjectiles);
            var intersectPoint = Geometry.GetClosestPointOnLineSegment(enemyProjectiles.StartPosition,
                enemyProjectiles.CalculatedEndPosition, LocalPlayer.Instance.WorldPosition);


            if (useQ && QSkill.IsReady && intersectingWithProjectile)
            {
                // if (Player.Distance(enemyProjectiles.WorldPosition) <= Player.MapCollisionRadius * 5)
                {
                    LocalPlayer.CastAbility(QSkill.Slot);
                }
            }

            if ((ESkill.IsReady) || hasBuff)
            {
                LocalPlayer.UpdateCursorPosition(intersectPoint, true); // TODO Experimental. Maybe try + direction
                LocalPlayer.CastAbility(ESkill.Slot);
            }

            if (!enemyTarget.IsDead && !enemyTarget.HasConsumeBuff && !enemyTarget.IsCountering &&
                !enemyTarget.IsImmaterial && !Helper.HasBuff(enemyTarget, "GustBuff") &&
                !Helper.HasBuff(enemyTarget, "BulwarkBuff") && !Helper.HasBuff(enemyTarget, "Incapacitate") &&
                !Helper.HasBuff(enemyTarget, "PetrifyStone") && !Helper.IsCollidingWithWalls(LocalPlayer.Instance, enemyTarget))
            {
                if (predictLMB.HitChancePercent >= 10f)
                {
                    LocalPlayer.UpdateCursorPosition(predictLMB.MoveMousePosition);
                    LocalPlayer.CastAbility(AbilitySlot.Ability1);
                }
            }
        }


        private static void Game_OnUpdate()
        {
            if (!Game.IsInGame) return;
            if (!enabled) return;


            if (Input.GetKeyDown(KeyCode.X) && EntitiesManager.ActiveGameObjects != null)
            {
                foreach (var o in EntitiesManager.ActiveGameObjects)
                    GameConsole.Write(o.ObjectName); //
                GameConsole.Write("\n");
            }

            if (Input.GetMouseButton(4))
                Combo();
            if (Input.GetKey(KeyCode.LeftAlt))
                HealCombo();

        }

        public void OnInit()
        {
            var aTeaSpoonOfGameInstance = Game.Instance;
            var menu = MainMenu.AddMenu("Pearl", "Pearl the Inquisitor");

            var menuDrawDebug = menu.Add(new MenuSlider("drawDebug", "Draw Debug", 1f, 25f));
            var useQMenuValue = menu.Add(new MenuCheckBox("useQskill", "Use Q On Projectiles", false));

            Game.OnUpdate += delegate
            {
                drawDebug = menuDrawDebug.CurrentValue;
                useQ = useQMenuValue.CurrentValue;
                Game_OnUpdate();
            };
            Game.OnMatchEnd += delegate
            {
                enabled = false;
            };
            Game.OnMatchStart += delegate
            {
                if (LocalPlayer.Instance.CharName != Champion.Pearl.ToString())
                    return;

                enabled = true;
            };

            Game.OnDraw += OnDraw;
        }

        private static void OnDraw(EventArgs obj)
        {
            if (LocalPlayer.Instance == null)
                return;

            if (Game.IsInRoundPhase && !Game.IsInPostRoundPhase)
            {
                if (RmbSkill != null && ESkill != null)
                {
                    Drawing.DrawCircle(LocalPlayer.Instance.WorldPosition, drawDebug, Color.yellow);
                    Drawing.DrawCircle(LocalPlayer.Instance.WorldPosition, RmbSkill.Range - 4.8f,
                        RmbSkill.Data.ChargeCount > 0 ? Color.green : Color.gray);
                    Drawing.DrawCircle(LocalPlayer.Instance.WorldPosition, ESkill.Range, ESkill.IsReady ? Color.blue : Color.gray);
                    Drawing.DrawString(LocalPlayer.Instance.WorldPosition, RmbSkill.Data.Cooldown.ToString(), Color.white);
                }

                var enemyProjectile = GetEnemyProjectiles(11f);
                if (enemyProjectile != null)
                {
                    var intersect = Helper.IsProjectileColliding(LocalPlayer.Instance, enemyProjectile);

                    if (intersect)
                    {
                        var intersectPoint = Geometry.GetClosestPointOnLineSegment(enemyProjectile.StartPosition, enemyProjectile.CalculatedEndPosition, LocalPlayer.Instance.WorldPosition);
                        var direction = LocalPlayer.Instance.WorldPosition - intersectPoint;
                        Drawing.DrawLine(enemyProjectile.WorldPosition, enemyProjectile.CalculatedEndPosition, Color.white);
                        Drawing.DrawLine(LocalPlayer.Instance.WorldPosition, intersectPoint, Color.green);
                    }
                    //TODO DrawRect projectile path?
                }
            }
        }

        public void OnUnload()
        {
            throw new NotImplementedException();
        }
    }
}