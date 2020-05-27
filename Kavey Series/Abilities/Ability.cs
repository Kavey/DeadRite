using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.SDK.Enumeration;
using Kavey_Series.Utilities;

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
        public Ability(AbilityKey key, float range = float.MaxValue, float speed = float.MaxValue, float radius = float.MaxValue, SkillType type = SkillType.Line)
        {
            Key = key;
            Range = range;
            Speed = speed;
            Radius = radius;
            Type = type;
        }
        private bool CanCastAbility()
        {
            var hud = LocalPlayer.GetAbilityHudData(GetSlot());
            return hud != null && hud.CooldownLeft <= 0 && hud.EnergyCost <= Utility.Player.Energized.Energy;
        }
        private AbilitySlot GetSlot()
        {
            return (AbilitySlot)Key;
        }
    }
}