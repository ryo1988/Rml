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
        /// <param name="decimals"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static double Round(double value, int decimals, MidpointRounding mode)
        {
            var pow = System.Math.Pow(10, decimals);
            return System.Math.Round(value * pow, mode) / pow;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimals"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static int Round(int value, int decimals, MidpointRounding mode)
        {
            var pow = System.Math.Pow(10, decimals);
            return (int)(System.Math.Round(value * pow, mode) / pow);
        }
    }
}