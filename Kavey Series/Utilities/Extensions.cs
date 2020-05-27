using System;
using System.Collections.Generic;
using System.Linq;
using BattleRight.Core;
using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.Core.GameObjects.Models;
using BattleRight.SDK;
using BattleRight.SDK.UI.Models;
using BattleRight.SDK.UI.Values;
using Kavey_Series.Abilities;
using Vector2 = BattleRight.Core.Math.Vector2;

namespace Kavey_Series.Utilities
{
    public static class Extensions
    {
        public static bool IsFrozen(this Character character)
        {
            return character.Buffs.Any(x => x.ObjectName == "Frozen");
        }
        public static bool IsChilled(this Character character)
        {
            return character.Buffs.Any(x => x.ObjectName == "FrostDebuff");
        }
        public static bool IsCCd(this Character character)
        {
            return character.IsFrozen() || character.HasCCOfType(CCType.Stun) || character.HasCCOfType(CCType.Root);
        }
        public static MapGameObject ClosestToMouse(this List<MapGameObject> gameObjects)
        {
            return gameObjects.OrderBy(x => x.Position.Distance(Utility.Player)).FirstOrDefault();
        }
        public static List<MapGameObject> GetMapObjects(this List<InGameObject> gameObjects)
        {
            return gameObjects.Select(x => x.Get<MapGameObject>()).ToList();
        }
        public static List<Character> EnemiesWillHit(this Ability ability, Character target)
        {
            var enemies = new List<Character>();
            foreach (var enemy in EntitiesManager.EnemyTeam.Where(x => x.IsValid && !x.Living.IsDead))
            {
                if (Geometry.CircleVsThickLine(new Vector2(enemy.MapObject.Position.X, enemy.MapObject.Position.Y), enemy.MapCollision.MapCollisionRadius,
                    Utility.Player.MapObject.Position, Utility.Player.MapObject.Position.Extend(target.MapObject.Position, ability.Range), ability.Radius, true))
                    enemies.Add(enemy);
            }
            return enemies;
        }
        public static List<Vector2> RotateAround(this Vector2 pos, float radius, int points)
        {
            var vectors = new List<Vector2>();
            for (var i = 1; i <= points; i++)
            {
                var angle = i * 2 * Math.PI / points;
                var point = new Vector2(pos.X + radius * (float)Math.Cos(angle), pos.Y + radius * (float)Math.Sin(angle));
                vectors.Add(point);
            }
            return vectors;
        }

        public static bool GetBoolean(this Menu menu, string menuItem)
        {
            var item = menu.Get<MenuCheckBox>(menuItem);

            if (item == null)
            {
                throw new Exception("GetBoolean: menuItem '" + menuItem + "' doesn't exist in " + menu.Name);
            }
            else
            {
                return item.CurrentValue;
            }
        }

        public static void SetBoolean(this Menu menu, string menuItem, bool value)
        {
            var item = menu.Get<MenuCheckBox>(menuItem);

            if (item == null)
            {
                throw new Exception("SetBoolean: menuItem '" + menuItem + "' doesn't exist in " + menu.Name);
            }
            else
            {
                item.CurrentValue = value;
            }
        }

        public static float GetSlider(this Menu menu, string menuItem)
        {
            var item = menu.Get<MenuSlider>(menuItem);

            if (item == null)
            {
                throw new Exception("GetSlider: menuItem '" + menuItem + "' doesn't exist in " + menu.Name);
            }
            else
            {
                return item.CurrentValue;
            }
        }

        public static void SetSlider(this Menu menu, string menuItem, float value)
        {
            var item = menu.Get<MenuSlider>(menuItem);

            if (item == null)
            {
                throw new Exception("SetSlider: menuItem '" + menuItem + "' doesn't exist in " + menu.Name);
            }
            else
            {
                item.CurrentValue = value;
            }
        }

        public static bool GetKeybind(this Menu menu, string menuItem)
        {
            var item = menu.Get<MenuKeybind>(menuItem);

            if (item == null)
            {
                throw new Exception("GetKeybind: menuItem '" + menuItem + "' doesn't exist in " + menu.Name);
            }
            else
            {
                return item.CurrentValue;
            }
        }

        public static int GetIntSlider(this Menu menu, string menuItem)
        {
            var item = menu.Get<MenuIntSlider>(menuItem);

            if (item == null)
            {
                throw new Exception("GetIntSlider: menuItem '" + menuItem + "' doesn't exist in " + menu.Name);
            }
            else
            {
                return item.CurrentValue;
            }
        }

        public static void SetIntSlider(this Menu menu, string menuItem, int value)
        {
            var item = menu.Get<MenuIntSlider>(menuItem);

            if (item == null)
            {
                throw new Exception("SetIntSlider: menuItem '" + menuItem + "' doesn't exist in " + menu.Name);
            }
            else
            {
                item.CurrentValue = value;
            }
        }

        public static int GetComboBox(this Menu menu, string menuItem)
        {
            var item = menu.Get<MenuComboBox>(menuItem);

            if (item == null)
            {
                throw new Exception("GetComboBox: menuItem '" + menuItem + "' doesn't exist in " + menu.Name);
            }
            else
            {
                return item.CurrentValue;
            }
        }

        public static Menu GetSubmenu(this Menu menu, string submenuName)
        {
            var submenu = menu.Get<Menu>(submenuName);

            if (submenu == null)
            {
                throw new Exception("GetSubmenu: submenu '" + submenuName + "' doesn't exist in " + menu.Name);
            }
            else
            {
                return submenu;
            }
        }
    }
}
