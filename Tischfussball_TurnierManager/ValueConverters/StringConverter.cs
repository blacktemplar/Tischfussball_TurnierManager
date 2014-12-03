using System;
using System.Windows.Data;

namespace Tischfussball_TurnierManager.ValueConverters
{
    public class StringConverter : IValueConverter
    {
        private string placeHolder = "%";

        public StringConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string input = "";

            if (value != null)
            {
                input = value.ToString();
            }

            if (parameter is string)
            {
                string s = (string)parameter;
                int ind = s.IndexOf(placeHolder);
                if (ind > -1)
                {
                    return s.Substring(0, ind) + input + s.Substring(ind + placeHolder.Length);
                }
                else
                {
                    return s + input;
                }
            }

            return input;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}