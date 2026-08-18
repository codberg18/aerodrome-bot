using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AerodromeSniper.ViewModels;

public abstract partial class PageViewModel : ObservableObject
{
    public abstract string Title { get; }

    public abstract string Subtitle { get; }

    public abstract Geometry Icon { get; }
}
