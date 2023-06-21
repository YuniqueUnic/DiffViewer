using MvvmDialogs;
using System.Windows;

namespace DiffViewer.Managers.Helper;

/// <summary>
/// Set the DataContext and Owner Window for a Window.
/// </summary>
public static class WindowHelper
{
    public static T SetDataContext<T>(this T window , object dataContext) where T : Window, IWindow
    {
        window.DataContext = dataContext;
        return window;
    }
    public static T SetOwnerWindow<T>(this T window , T owner) where T : Window, IWindow
    {
        window.Owner = owner;
        return window;
    }
}
