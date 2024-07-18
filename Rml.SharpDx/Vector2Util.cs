using SharpDX;

namespace Rml.SharpDx
{
    public static class Vector2Util
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

        public static bool Approximately(float a, float b)
        {
            return System.Math.Abs(b - a) <
                   System.Math.Max(1E-06f * System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)),
                       float.Epsilon * 8f);
        }

        public static bool IsPointOnSegment(Vector2 point, (Vector2 from, Vector2 to) segment)
        {
            var ac = (segment.from - point).LengthSquared();
            var cb = (point - segment.to).LengthSquared();
            var ab = (segment.from - segment.to).LengthSquared();
         
            return ac + cb - ab < double.Epsilon;
        }

        public static bool IsLineIntersectingSegment(
            (Vector2 from, Vector2 to) line,
            (Vector2 from, Vector2 to) segment,
            out Vector2 intersection)
        {
            intersection = Vector2.Zero;

            var a1 = line.to.Y - line.from.Y;
            var b1 = line.from.X - line.to.X;
            var c1 = a1 * line.from.X + b1 * line.from.Y;

            var a2 = segment.to.Y - segment.from.Y;
            var b2 = segment.from.X - segment.to.X;
            var c2 = a2 * segment.from.X + b2 * segment.from.Y;

            var determinant = a1 * b2 - a2 * b1;

            if (Approximately(determinant, 0))
            {
                return false;
            }

            intersection = new Vector2(
                (b2 * c1 - b1 * c2) / determinant,
                (a1 * c2 - a2 * c1) / determinant
            );

            return IsPointOnSegment(intersection, segment);
        }

        public static bool AreLinesIntersecting(
            (Vector2 from, Vector2 to) line1,
            (Vector2 from, Vector2 to) line2,
            out Vector2 intersection)
        {
            intersection = Vector2.Zero;

            var a1 = line1.to.Y - line1.from.Y;
            var b1 = line1.from.X - line1.to.X;
            var c1 = a1 * line1.from.X + b1 * line1.from.Y;

            var a2 = line2.to.Y - line2.from.Y;
            var b2 = line2.from.X - line2.to.X;
            var c2 = a2 * line2.from.X + b2 * line2.from.Y;

            var determinant = a1 * b2 - a2 * b1;

            if (Approximately(determinant, 0))
            {
                return false;
            }

            intersection = new Vector2(
                (b2 * c1 - b1 * c2) / determinant,
                (a1 * c2 - a2 * c1) / determinant
            );
            return true;
        }
    }
}