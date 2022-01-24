using System;

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    public static class Math
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digits"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static float Round(this float value, int digits, MidpointRounding mode)
        {
            return (float) System.Math.Round(value, digits, mode);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digits"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static double Round(this double value, int digits, MidpointRounding mode)
        {
            return System.Math.Round(value, digits, mode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digits"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static int Round(this int value, int digits, MidpointRounding mode)
        {
            var pow = System.Math.Pow(10.0, digits);
            return (int) (System.Math.Round(value / pow, digits, mode) * pow);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static int RoundInt(this float value, MidpointRounding mode = MidpointRounding.AwayFromZero)
        {
            return (int)Round(value, 0, mode);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static int RoundInt(this double value, MidpointRounding mode = MidpointRounding.AwayFromZero)
        {
            return (int)Round(value, 0, mode);
        }
    }
}