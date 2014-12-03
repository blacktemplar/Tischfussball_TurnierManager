using System;
using System.Windows.Data;

namespace Tischfussball_TurnierManager.ValueConverters
{
    public class BooleanInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return convert(value);
        }

        private object convert(object value)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return convert(value);
        }
    }
}