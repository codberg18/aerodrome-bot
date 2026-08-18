using System.Windows;
using System.Windows.Media;

namespace AerodromeSniper.ViewModels;

internal static class IconRes
{
    public static Geometry Get(string key) => (Geometry)Application.Current.FindResource(key);
}
