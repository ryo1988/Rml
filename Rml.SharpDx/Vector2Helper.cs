using SharpDX;

namespace Rml.SharpDx
{
    public static class Vector2Helper
    {
        private const float EpsilonNormalSqrt = 1e-15f;

        /// <summary>
        ///
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float Angle(Vector2 from, Vector2 to)
        {
            var denominator = (float)System.Math.Sqrt(from.LengthSquared() * to.LengthSquared());
            if (denominator < EpsilonNormalSqrt)
                return 0;

            var dot = MathUtil.Clamp(Vector2.Dot(from, to) / denominator, -1, 1);
            return (float)System.Math.Acos(dot);
        }

        public static bool NearEqual(this Vector2 left, Vector2 right, Vector2 epsilon)
        {
            return MathUtil.WithinEpsilon(left.X, right.X, epsilon.X) &&
                   MathUtil.WithinEpsilon(left.Y, right.Y, epsilon.Y);
        }

        public static bool NearEqual(this Vector2 left, Vector2 right, float epsilon)
        {
            return NearEqual(left, right, new Vector2(epsilon));
        }
        
        public static (Vector2 closestOnLine, float distance) DistanceToLineSegment(Vector2 a, Vector2 b, Vector2 p)
        {
            var ap = p - a;
            var ab = b - a;
        
            var ab2 = ab.X * ab.X + ab.Y * ab.Y;
            var apAb = ap.X * ab.X + ap.Y * ab.Y;
            var t = apAb / ab2;

            t = t switch
            {
                < 0.0f => 0.0f,
                > 1.0f => 1.0f,
                _ => t
            };

            var closest = a + ab * t;
            return (closest, Vector2.Distance(closest, p));
        }
        
        public static (Vector2 closestPoint, Vector2 closestPointOnLine) FindClosestPoint(Vector2 linePoint1, Vector2 linePoint2, Vector2[] points)
        {
            var closestPoint = points[0];
            var (minClosestOnLine, minDistance) = DistanceToLineSegment(linePoint1, linePoint2, points[0]);
        
            foreach (var point in points)
            {
                var (closestOnLine, distance) = DistanceToLineSegment(linePoint1, linePoint2, point);
                if (distance < minDistance)
                {
                    minClosestOnLine = closestOnLine;
                    minDistance = distance;
                    closestPoint = point;
                }
            }
        
            return (closestPoint, minClosestOnLine);
        }
    }
}