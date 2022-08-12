using System;
using System.Collections.Immutable;
using System.Linq;

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

        private static readonly char[] Base36String = new[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
            'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't',
            'u', 'v', 'w', 'x', 'y', 'z'
        };

        private static readonly ImmutableDictionary<char, int> Base36ToInt = Base36String
            .Select((o, i) => (value: o, index: i))
            .ToImmutableDictionary(o => o.value, o => o.index);

        private static readonly ImmutableDictionary<int, char> IntToBase36 = Base36String
            .Select((o, i) => (value: o, index: i))
            .ToImmutableDictionary(o => o.index, o => o.value);

        public static int ConvertBase36ToInt(this ReadOnlySpan<char> base36)
        {
            int value = 0;
            
            foreach (var c in base36.Trim())
            {
                value = value * Base36String.Length + Base36ToInt[c];
            }

            return value;
        }

        public static string ConvertIntToBase36(this int intValue)
        {
            var length = (int)System.Math.Log(intValue, Base36String.Length) + 1;
            
            Span<char> base36 = stackalloc char[length];

            for (int i = length - 1; i >= 0; i--)
            {
                base36[i] = IntToBase36[intValue % Base36String.Length];
                intValue /= Base36String.Length;
            }

            return base36.ToString();
        }
    }
}