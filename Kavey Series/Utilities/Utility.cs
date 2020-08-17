using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleRight.Core;
using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.Core.GameObjects.Models;
using BattleRight.Core.Math;
using BattleRight.Core.Models;
using BattleRight.SDK;

namespace Kavey_Series.Utilities
{
    public static class Utility
    {
        public static Character Player => EntitiesManager.LocalPlayer;
        public static Vector2 MyPos => Player.MapObject.Position;
        public static float MyRadius => Player.MapCollision.MapCollisionRadius;
        public static float MyEnergy => Player.Energized.Energy;

        public static List<Battlerite> Battlerites => GetBattlerites();
        public static List<InGameObject> Dummies => EntitiesManager.InGameObjects.Where(x => x.ObjectName == "ArenaDummy" && x.IsValid && x.IsActiveObject).ToList();

        public static Vector2 SafestPosition(Character target, float radius, float safeRadius = 2f)
        {
            var positions = MyPos.RotateAround(radius, 60);
            var bestPos = positions.OrderBy(x => x.CountEnemiesInRange(safeRadius)).ThenByDescending(x => x.CountAlliesInRange(safeRadius))
                .ThenByDescending(x => x.Distance(target)).FirstOrDefault();
            return bestPos != Vector2.Zero ? bestPos : InputManager.MousePosition.ScreenToWorld();
        }

        public static Vector2 SafestPosition(Vector2 pos, float radius, float safeRadius = 2f)
        {
            var positions = MyPos.RotateAround(radius, 60);
            var bestPos = positions.OrderBy(x => x.CountEnemiesInRange(safeRadius)).ThenByDescending(x => x.CountAlliesInRange(safeRadius))
                .ThenByDescending(x => x.Distance(pos)).FirstOrDefault();
            return bestPos != Vector2.Zero ? bestPos : InputManager.MousePosition.ScreenToWorld();
        }

        private static List<Battlerite> GetBattlerites()
        {
            var battlerites = new List<Battlerite>();
            for (var i = 0; i <= 4; i++)
            {
                var battlerite = Player.BattleriteSystem.GetEquippedBattlerite(i);
                if (battlerite != null && battlerite.Name != "{None}")
                    battlerites.Add(battlerite);
            }
            return battlerites;
        }

        public static int EnemiesAroundAlive(this InGameObject gameObj, float distance)
        {
            return EntitiesManager.EnemyTeam.Count(x => !x.Living.IsDead && x.Distance(gameObj.Get<MapGameObject>().Position) <= distance);
        }

        public static void DelayAction(Action action, float seconds)
        {
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer(obj =>
                {
                    action();
                    timer.Dispose();
                },
                null, (long)(seconds * 1000), System.Threading.Timeout.Infinite);
        }

        public static void Log(string msg, ConsoleColor c = ConsoleColor.Gray)
        {
            Console.ForegroundColor = c;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Log(string msg, ConsoleColor c = ConsoleColor.Gray, params object[] args)
        {
            Console.ForegroundColor = c;
            Console.WriteLine(msg, args);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        [Obsolete]
        public static Vector2 TempScreenToWorld(this Vector2 screenPos)
        {
            var cam = UnityEngine.Camera.main;
            var ray = cam.ScreenPointToRay(new UnityEngine.Vector3(screenPos.X, screenPos.Y));
            var plane = new UnityEngine.Plane(UnityEngine.Vector3.up, UnityEngine.Vector3.zero);

            float d;
            if (plane.Raycast(ray, out d))
            {
                return new Vector2(ray.GetPoint(d).x, ray.GetPoint(d).z);
            }

            return Vector2.Zero;
        }
    }
}
