using CommunityToolkit.Mvvm.Messaging;
using DiffViewer.Messages;
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
        WeakReferenceMessenger.Default.Register<SetRichTextBoxDocumentMessage>(this , (o , m) =>
        {
            //if( m is null || m.Message != "LoadRawContent" || m.ObjReplied is not TestCase tc ) return;
            //this.DataContext = tc;
            ////RawRichFlowDocumentReader.Document = FlowDocumentManager.CreateFlowDocument(tc.Raw ?? string.Empty);
            App.Logger.Information<SetRichTextBoxDocumentMessage>($"Show VSTS Setting window Done with msg: {m.Message}" , m);
        });
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        //base.OnClosing(e);
        this.Visibility = Visibility.Hidden;
        e.Cancel = true;
    }

}
