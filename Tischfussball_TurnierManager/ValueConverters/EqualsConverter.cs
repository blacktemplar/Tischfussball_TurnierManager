using System;
using System.Windows.Data;

namespace Tischfussball_TurnierManager.ValueConverters
{
    public class EqualsConverter : IValueConverter
    {
        public object Comp
        {
            get
            {
                return comp;
            }
            set
            {
                compIsSet = true;
                comp = value;
            }
        }

        private object comp;

        private bool compIsSet;

        public EqualsConverter()
        {
            comp = null;
            compIsSet = false;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object toComp = compIsSet ? Comp : parameter;
            return (value == null && toComp == null) || ((value != null) && value.Equals(toComp));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}