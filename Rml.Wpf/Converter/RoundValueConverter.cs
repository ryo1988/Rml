using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Rml.Wpf.Converter
{
    /// <summary>
    /// XAML上で四捨五入や切り上げ切り捨てを指定できるコンバータ
    /// </summary>
    public class RoundValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var setting = ((string)parameter).Split(':');
            var dValue = (double)value;
            return setting[0] switch
            {
                "R" => System.Math.Round(dValue, int.Parse(setting[1]), MidpointRounding.AwayFromZero),
                "D" => (int)(dValue * System.Math.Pow(10, int.Parse(setting[1]))) / System.Math.Pow(10, int.Parse(setting[1])),
                _ => dValue
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
