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
    }
}