using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Tischfussball_TurnierManager.ValueConverters
{
    public class BoolToObjectConverter : IValueConverter
    {
        public object FalseObject { get; set; }

        public object TrueObject { get; set; }

        public BoolToObjectConverter()
        {
            FalseObject = null;
            TrueObject = null;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                bool val = (bool)value;
                if (val)
                {
                    return TrueObject;
                }
                else
                {
                    return FalseObject;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}