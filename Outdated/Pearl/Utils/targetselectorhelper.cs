﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BattleRight.Core;
using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.Core.Math;
using BattleRight.Core.Models;

using BattleRight.SDK;
using BattleRight.SDK.Enumeration;
using BattleRight.SDK.Events;
using BattleRight.SDK.UI;
using BattleRight.SDK.UI.Models;
using BattleRight.SDK.UI.Values;

namespace Pearl.Utils
{
    internal class TargetSelectorHelper
    {
		var Player = LocalPlayer.Instance;
		
        public static Player GetTarget(TargetingMode targetingMode, float worldDistance)
        {
            var enemies = EntitiesManager.EnemyTeam;
            var player = LocalPlayer.Instance;
            var aliveEnemies = enemies.Where(e =>
                !e.IsDead &&
                e.Distance(LocalPlayer.Instance) <= worldDistance);

            switch (targetingMode)
            {
                case TargetingMode.LowestHealth:
                    return aliveEnemies.OrderBy(e => e.Health).FirstOrDefault(Helper.IsValidTarget);

                case TargetingMode.NearLocalPlayer:
                    return aliveEnemies.OrderBy(e => e.Distance(player.WorldPosition))
                        .FirstOrDefault(Helper.IsValidTarget);

                case TargetingMode.NearMouse:
                    return aliveEnemies.OrderBy(e => e.Distance(InputManager.MousePosition))
                        .FirstOrDefault(Helper.IsValidTarget);

                default:
                    return null;
            }
        }
        public static Player GetAlly(TargetingMode targetingMode, float worldDistance)
        {
            var allies = EntitiesManager.LocalTeam;
            var player = LocalPlayer.Instance;
            var aliveAlies = allies.Where(e => !e.IsDead && e.Distance(LocalPlayer.Instance) <= worldDistance && !Helper.IsCollidingWithWalls(player, e));
            switch (targetingMode)
            {
                case TargetingMode.LowestHealth:
                    return aliveAlies.OrderByDescending(e => e.MaxRecoveryHealth - e.Health)
                        .FirstOrDefault();

                case TargetingMode.NearLocalPlayer:
                    return aliveAlies.OrderBy(e => e.Distance(player.WorldPosition))
                        .FirstOrDefault();

                case TargetingMode.NearMouse:
                    return aliveAlies.OrderBy(e => e.Distance(InputManager.MousePosition))
                        .FirstOrDefault();

                default:
                    return null;
            }
        }
    }
}