using System;
using SharpDX;

namespace Rml.SharpDx
{
    /// <summary>
    /// 
    /// </summary>
    public static class Vector3Helper
    {
        private const float EpsilonNormalSqrt = 1e-15f;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float Angle(Vector3 from, Vector3 to)
        {
            var denominator = (float)System.Math.Sqrt(from.LengthSquared() * to.LengthSquared());
            if (denominator < EpsilonNormalSqrt)
                return 0;

            var dot = MathUtil.Clamp(Vector3.Dot(from, to) / denominator, -1, 1);
            return (float)System.Math.Acos(dot);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="hint"></param>
        /// <returns></returns>
        public static Vector3 CalcPlaneNormal(Vector3[] positions, Vector3 hint)
        {
            var first = positions[1] - positions[0];
            var last = positions[2] - positions[0];

            var planeNormal = Vector3.Normalize(Vector3.Cross(first, last));
            if (Angle(planeNormal, hint) > Angle(-planeNormal, hint)) {
                planeNormal = -planeNormal;
            }

            return planeNormal;
        }

        public static Vector3 Round(this Vector3 from, int digit)
        {
            return new Vector3(
                Math.Round(from.X, digit, MidpointRounding.AwayFromZero),
                Math.Round(from.Y, digit, MidpointRounding.AwayFromZero),
                Math.Round(from.Z, digit, MidpointRounding.AwayFromZero));
        }

        public static Vector2 Round(this Vector2 from, int digit)
        {
            return new Vector2(
                Math.Round(from.X, digit, MidpointRounding.AwayFromZero),
                Math.Round(from.Y, digit, MidpointRounding.AwayFromZero));
        }

        public static bool IsRectangle(this Vector3[] positions)
        {
            if (positions.Length != 4)
                return false;

            if (System.Math.Abs(Angle(positions[0] - positions[1], positions[2] - positions[1]) - MathUtil.DegreesToRadians(90)) > 0.0001)
                return false;

            if (System.Math.Abs(Angle(positions[1] - positions[2], positions[3] - positions[2]) - MathUtil.DegreesToRadians(90)) > 0.0001)
                return false;

            if (System.Math.Abs(Angle(positions[2] - positions[3], positions[0] - positions[3]) - MathUtil.DegreesToRadians(90)) > 0.0001)
                return false;

            return true;
        }
    }
}