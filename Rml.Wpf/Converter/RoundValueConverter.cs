using System;
using System.Globalization;
using System.Windows.Data;

namespace Rml.Wpf.Converter
{
    /// <summary>
    /// XAML上で四捨五入や切り上げ切り捨てを指定できるコンバータ
    /// </summary>
    public class RoundValueConverter : IValueConverter
    {
        /// <summary>
        /// double値を受け取り、指定した桁数による四捨五入(R)か切り捨て(D)を行った結果を返します
        /// </summary>
        /// <param name="value">入力値</param>
        /// <param name="targetType">doubleです</param>
        /// <param name="parameter">"D:N"で小数点第N位までの値を保持して以下切り捨て、"R:N"で小数点第N+1位以下を四捨五入</param>
        /// <param name="culture">使用しません</param>
        /// <returns>変換後の出力値</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string)
                throw new ArgumentException(nameof(parameter));
            if (value is not double)
                throw new ArgumentException(nameof(value));

            var setting = ((string)parameter).Split(':');
            var dValue = (double)value;
            return setting[0] switch
            {
                "R" => System.Math.Round(dValue, int.Parse(setting[1]), MidpointRounding.AwayFromZero),
                "D" => (int)(dValue * System.Math.Pow(10, int.Parse(setting[1]))) / System.Math.Pow(10, int.Parse(setting[1])),
                _ => dValue
            };
        }

        /// <summary>
        /// 逆変換は禁止です(View用の値でViewModel側を更新するべきではない)
        /// </summary>
        /// <param name="value">使用しません</param>
        /// <param name="targetType">使用しません</param>
        /// <param name="parameter">使用しません</param>
        /// <param name="culture">使用しません</param>
        /// <returns>使用しません</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
