﻿using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.SDK.Enumeration;
using Kavey_Series.Utilities;
using System.Runtime.CompilerServices;

namespace Kavey_Series.Abilities
{
    public class Ability
    {
        public AbilityKey Key { get; }
        public float Range { get; set; }
        public float Speed { get; }
        public float Radius { get; }
        public SkillType Type { get; }
        public AbilitySlot Slot => GetSlot();
        public bool CanCast => CanCastAbility();
        public bool CanCastCharges => CanCastAbilityCharges();
        public Ability(AbilityKey key, float range = float.MaxValue, float speed = float.MaxValue, float radius = float.MaxValue, SkillType type = SkillType.Line)
        {
            Key = key;
            Range = range;
            Speed = speed;
            Radius = radius;
            Type = type;
        }
        public bool CanCastAbility(int cd = 0)
        {
            var hud = LocalPlayer.GetAbilityHudData(GetSlot());
            return hud != null && hud.CooldownLeft <= cd && hud.EnergyCost <= Utility.Player.Energized.Energy;
        }

        private bool CanCastAbilityCharges(int chargesRequired = 1)
        {
            var hud = LocalPlayer.GetAbilityHudData(GetSlot());
            return hud != null && hud.CooldownLeft <= 0 && hud.ChargeCount >= chargesRequired && hud.EnergyCost <= Utility.Player.Energized.Energy;
        }

        private AbilitySlot GetSlot()
        {
            return (AbilitySlot)Key;
        }
    }
}