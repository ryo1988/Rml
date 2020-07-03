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
        public static float Round(float value, int digits, MidpointRounding mode)
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
        public static double Round(double value, int digits, MidpointRounding mode)
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
        public static int Round(int value, int digits, MidpointRounding mode)
        {
            return (int) System.Math.Round((double)value, digits, mode);
        }
    }
}