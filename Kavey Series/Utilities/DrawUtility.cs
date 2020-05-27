using System;
using BattleRight.Core;
using UnityEngine;
using Vector2 = BattleRight.Core.Math.Vector2;

namespace Kavey_Series.Utilities
{
    public static class DrawUtility
    {
        public static void DrawRectangle(Vector2 start, Vector2 end, float radius, Color color)
        {
            var w = end.X - start.X;
            var h = end.Y - start.Y;
            var l = Math.Sqrt(w * w + h * h);
            var xS = (radius * h / l);
            var yS = (radius * w / l);

            var rightStartPos = new Vector2((float)(start.X - xS), (float)(start.Y + yS));
            var leftStartPos = new Vector2((float)(start.X + xS), (float)(start.Y - yS));
            var rightEndPos = new Vector2((float)(end.X - xS), (float)(end.Y + yS));
            var leftEndPos = new Vector2((float)(end.X + xS), (float)(end.Y - yS));

            Drawing.DrawLine(rightStartPos, rightEndPos, color);
            Drawing.DrawLine(leftStartPos, leftEndPos, color);
            Drawing.DrawLine(rightStartPos, leftStartPos, color);
            Drawing.DrawLine(leftEndPos, rightEndPos, color);
        }
    }
}
