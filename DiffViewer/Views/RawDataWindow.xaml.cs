using MvvmDialogs;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DiffViewer.Views;

/// <summary>
/// RawDataWindow.xaml 的交互逻辑
/// </summary>
public partial class RawDataWindow : Window, IWindow
{
    public RawDataWindow( )
    {
        InitializeComponent();
    }

    ContentControl IWindow.Owner { get => this.Owner; set => this.Owner = (Window)value; }
    protected override void OnClosing(CancelEventArgs e)
    {
        //base.OnClosing(e);
        this.Visibility = Visibility.Hidden;
        e.Cancel = true;
    }

    private void RawTextBox_PreviewMouseWheel(object sender , System.Windows.Input.MouseWheelEventArgs e)
    {
        if( Keyboard.Modifiers == ModifierKeys.Control )
        {
            if( e.Delta > 0 )
            {
                RawTextBox.FontSize++;
            }
            else
            {
                RawTextBox.FontSize--;
            }

            e.Handled = true;
        }
    }
}
