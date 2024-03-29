﻿using System;
using System.Linq;
using BattleRight.Core.Enumeration;
using BattleRight.Core.Math;
using Hoyer.Base.Data.Abilites;

namespace Hoyer.Base.Extensions
{
    public static class MiscExtensions
    {
        public static bool Includes(this int[] array, int target)
        {
            return array.Any(o => o == target);
        }

        public static bool Includes(this string[] array, string target)
        {
            return array.Any(o => o == target);
        }

        public static bool Includes(this float[] array, float target)
        {
            return array.Any(o => o == target);
        }

        public static bool Includes(this Vector2[] array, Vector2 target)
        {
            return array.Any(o => o == target);
        }

        public static bool Includes(this AbilitySlot[] array, AbilitySlot target)
        {
            return array.Any(o => o == target);
        }

        public static string ToFriendlyString(this AbilitySlot slot)
        {
            switch (slot)
            {
                case AbilitySlot.Ability1:
                    return "M1";
                case AbilitySlot.Ability2:
                    return "M2";
                case AbilitySlot.Ability3:
                    return "Space";
                case AbilitySlot.Ability4:
                    return "Q";
                case AbilitySlot.Ability5:
                    return "E";
                case AbilitySlot.Ability6:
                    return "R";
                case AbilitySlot.Ability7:
                    return "F";
                case AbilitySlot.EXAbility1:
                    return "EX1";
                case AbilitySlot.EXAbility2:
                    return "EX2";
            }
            Console.WriteLine("Error in parsing of AbilitySlot");
            return "Error";
        }

        public static string ToFriendlyString(this AbilityType type)
        {
            switch (type)
            {
                case AbilityType.LineProjectile:
                    return "Projectile";
                case AbilityType.CircleJump:
                    return "Jump";
                case AbilityType.CircleThrowObject:
                    return "Throw";
                case AbilityType.Dash:
                    return "Dash";
                case AbilityType.Melee:
                    return "Melee";
                case AbilityType.CurveProjectile:
                    return "CurveProjectile";
            }
            Console.WriteLine("Error in parsing of AbilityType " + type);
            return "Error";
        }

        public static string ToFriendlyString(this DodgeAbilityType type)
        {
            switch (type)
            {
                case DodgeAbilityType.Counter:
                    return "Counter";
                case DodgeAbilityType.Jump:
                    return "Jump";
                case DodgeAbilityType.Shield:
                    return "Shield";
                case DodgeAbilityType.Ghost:
                    return "Ghost";
                case DodgeAbilityType.HealthShield:
                    return "HP Shield";
                case DodgeAbilityType.AoEHealthShield:
                    return "AOE HP Shield";
                case DodgeAbilityType.KnockAway:
                    return "Anti Dash";
                case DodgeAbilityType.Obstacle:
                    return "Obstacle";
            }
            Console.WriteLine("Error in parsing of DodgeAbilityType");
            return "Error";
        }
    }
}