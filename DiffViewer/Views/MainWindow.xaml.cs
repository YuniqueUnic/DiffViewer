using CommunityToolkit.Mvvm.Messaging;
using DiffViewer.Messages;
using System.Windows;

namespace DiffViewer.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    public MainWindow( )
    {
        InitializeComponent();

        RegisteMessengers();

        App.LanguageChanged += App_LanguageChanged;
    }

    private void App_LanguageChanged(object? sender , LanguageChangedEventArgs e)
    {
        SearchTextBox.Text = SearchTextBox.Text switch
        {
            "Search..." => "搜索...",
            "搜索..." => "Search...",
            _ => SearchTextBox.Text,
        };
    }

    /// <summary>
    /// Registe all messengers.
    /// </summary>
    private void RegisteMessengers( )
    {
        // Register WindowActionMessage
        // 1. Close
        // 2. Maximize
        // 3. Minimize
        WeakReferenceMessenger.Default.Register<WindowActionMessage>(this , (o , m) =>
        {
            if( m is null || m.Sender != this.DataContext ) return;

            switch( m.Message )
            {
                case "Close":
                    this.Close();
                    break;
                case "Maximize":
                    this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                    break;
                case "Minimize":
                    this.WindowState = WindowState.Minimized;
                    break;
                default:
                    App.Logger.Warning<WindowActionMessage>($"Unknown WindowActionMessage: {m.Message}" , m);
                    break;
            }

            App.Logger.Debug<WindowActionMessage>($"WindowActionMessage: {m.Message} Done" , m);
        });


    }

    /// <summary>
    /// Change the position of Window when mouse left button down on the top toolbar.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TopToolBar_MouseLeftButtonDown(object sender , System.Windows.Input.MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private void SearchTextBox_GotFocus(object sender , RoutedEventArgs e)
    {
        if( SearchTextBox.Text == OnlySearchTextBox.Text + "..." )
        {
            SearchTextBox.Text = "";
        }
    }

    private void SearchTextBox_LostFocus(object sender , RoutedEventArgs e)
    {
        if( SearchTextBox.Text == "" )
        {
            SearchTextBox.Text = OnlySearchTextBox.Text + "...";
        }
    }
}
