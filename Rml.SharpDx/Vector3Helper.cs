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
    }
}