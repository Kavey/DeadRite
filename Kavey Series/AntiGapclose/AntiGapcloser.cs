using System;
using BattleRight.Core;
using BattleRight.Core.Enumeration;
using BattleRight.Core.GameObjects;
using BattleRight.Core.Math;
using Kavey_Series.Utilities;

namespace Kavey_Series.AntiGapclose
{
    public static class AntiGapcloser
    {
        public static event AntiGapcloseHandler OnGapcloser;
        public delegate void AntiGapcloseHandler(GapcloseEventArgs args);
        public class GapcloseEventArgs : EventArgs
        {
            public bool WillHit;
            public Vector2 StartPosition;
            public Vector2 EndPosition;
            public GapcloseEventArgs(bool willHit, Vector2 startPos, Vector2 endPos)
            {
                WillHit = willHit;
                StartPosition = startPos;
                EndPosition = endPos;
            }
        }
        public static void Initialize()
        {
            Character.OnDash += Character_OnDash;
        }

        private static void Character_OnDash(DashEventArgs args)
        {
            if (!args.Caster.IsEnemy)
                return;
            var dashStart = args.Dash.StartPosition;
            var dashEnd = args.Dash.TargetPosition;
            var radius = args.Caster.MapCollision.MapCollisionRadius + Utility.MyRadius;
            var collision = CollisionSolver.CheckThickLineCollision(dashStart, dashEnd, radius, CollisionFlags.Bush | CollisionFlags.NPCBlocker | CollisionFlags.Team2);
            var realEndPos = collision.IsColliding ? collision.CollisionPoint : dashEnd;
            var willHitMe = CollisionSolver.CheckThickLineCollision(Utility.MyPos, realEndPos, Utility.MyRadius).IsColliding;
            // Utility.Log($"[Anti-Gapcloser][Dash] - [Hero: {args.Caster.CharName} | Buff: {args.BuffInstance.ObjectName} || Will Hit: {willHitMe}] ", ConsoleColor.Green);
            OnGapcloser?.Invoke(new GapcloseEventArgs(willHitMe, dashStart, realEndPos));
        }
    }
}
