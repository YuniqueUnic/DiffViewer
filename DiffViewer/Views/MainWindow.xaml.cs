using CommunityToolkit.Mvvm.Messaging;
using DiffViewer.Messages;
using DiffViewer.Models;
using System;
using System.Windows;
using System.Windows.Controls;

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
        UpdateSearchTextLanguage();
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
                    App.Current.Shutdown();
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

        if( e.ClickCount == 2 )
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        this.DragMove();

        e.Handled = true;
    }

    private void ListView_SelectionChanged(object sender , System.Windows.Controls.SelectionChangedEventArgs e)
    {
        DiffTestCase Add = new();
        DiffTestCase Remove = new();
        try
        {
            Add = e.AddedItems[0] as DiffTestCase;
            Remove = e.RemovedItems[0] as DiffTestCase;
        }
        catch( Exception )
        {

        }
        App.Logger.Information($"ListBox_SelectionChanged.From({Remove?.Name}).To({Add?.Name})");
        if( sender is ListBox listbox )
        {
            if( listbox.SelectedItem != null )
            {
                listbox.Dispatcher.BeginInvoke((Action)delegate
                {
                    listbox.UpdateLayout();
                    listbox.ScrollIntoView(listbox.SelectedItem);
                });
            }
        }

    }

    private void UpdateSearchTextLanguage( )
    {
        SearchTextBox.Text = SearchTextBox.Text switch
        {
            "Search..." => "搜 索...",
            "搜 索..." => "Search...",
            _ => SearchTextBox.Text,
        };
    }

    private void SearchTextBox_GotFocus(object sender , RoutedEventArgs e)
    {
        if( SearchTextBox.Text == App.Current.Resources.MergedDictionaries[0]["Search"].ToString() + "..." )
        {
            SearchTextBox.Text = "";
        }
    }

    private void SearchTextBox_LostFocus(object sender , RoutedEventArgs e)
    {
        if( SearchTextBox.Text == "" )
        {
            SearchTextBox.Text = App.Current.Resources.MergedDictionaries[0]["Search"].ToString() + "...";
        }
    }

    private void SearchTextBox_KeyDown(object sender , System.Windows.Input.KeyEventArgs e)
    {
        if( e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return )
        {
            SearchButton_Click(sender , e);
        }
    }

    private void SearchButton_Click(object sender , RoutedEventArgs e)
    {
        if( DiffTestCasesListView.Items.Count > 0 )
        {
            if( SearchTextBox.Text != App.Current.Resources.MergedDictionaries[0]["Search"].ToString() + "..." )
            {
                DiffTestCasesListView.Items.Filter = o =>
                {
                    if( o is DiffTestCase diffTestCase )
                    {
                        return diffTestCase.Name?.IndexOf(SearchTextBox.Text , StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    else
                    {
                        return false; // 删除非 DiffTestCase 类型的对象
                    }
                };
            }
            else
            {
                DiffTestCasesListView.Items.Filter = null; // 取消筛选
            }
        }
    }

}
