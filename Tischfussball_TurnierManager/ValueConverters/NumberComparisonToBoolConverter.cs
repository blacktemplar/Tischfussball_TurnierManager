using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Tischfussball_TurnierManager.ValueConverters
{
    public class IntLEComparisonToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is string && value is int)
            {
                int res;
                if (Int32.TryParse((string)parameter, out res))
                {
                    return (int)value <= res;
                }
            }
            if (parameter is int && value is int)
            {
                return (int)value <= (int)parameter;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}