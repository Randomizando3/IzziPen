using System;
using System.Globalization;
using System.Windows.Data;

namespace MultitouchAnnotations
{
    public class BottomConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double bottom)
            {
                return bottom - 100; // altura do menu
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
