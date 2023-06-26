using CommunityToolkit.Mvvm.Messaging;
using DiffViewer.Messages;
using DiffViewer.ViewModels;
using MvvmDialogs;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DiffViewer.Views;

/// <summary>
/// VSTSSettingWindow.xaml 的交互逻辑
/// </summary>
public partial class VSTSSettingWindow : Window, IWindow
{
    public VSTSSettingWindow( )
    {
        InitializeComponent();

        RegisteMessengers();
    }

    ContentControl IWindow.Owner { get => this.Owner; set => this.Owner = (Window)value; }


    /// <summary>
    /// Registe all messengers.
    /// </summary>
    private void RegisteMessengers( )
    {
        // Set the RichText Content
        WeakReferenceMessenger.Default.Register<RefreshAccessInfoMessage>(this , (o , m) =>
        {
            if( m is null || m.Message != "RefreshAccessInfos" ) return;
            (this.DataContext as VSTSSettingViewModel)?.RefreshAccessInfos();
            App.Logger.Information<RefreshAccessInfoMessage>($"Refresh Access Infos Message Done: {m.Message}" , m);
        });
    }


    protected override void OnClosing(CancelEventArgs e)
    {
        this.Visibility = Visibility.Hidden;
        e.Cancel = true;
    }

}
