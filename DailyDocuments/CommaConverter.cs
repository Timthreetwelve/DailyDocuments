using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace DailyDocuments
{
    public class CommaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !value.ToString().Contains(' '))
            {
                return value;
            }
            else
            {
                List<string> chunks = value.ToString().Split(new char[] { ' ', ',' })
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s.Trim()))
                    .ToList();
                return string.Join(", ", chunks);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
