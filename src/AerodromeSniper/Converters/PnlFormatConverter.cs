using System.Globalization;
using System.Windows.Data;

namespace AerodromeSniper.Converters;

public class PnlFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            var sign = d > 0 ? "+" : string.Empty;
            return sign + d.ToString("0.00", CultureInfo.InvariantCulture) + "%";
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
