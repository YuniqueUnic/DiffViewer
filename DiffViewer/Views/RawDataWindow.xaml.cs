using CommunityToolkit.Mvvm.Messaging;
using DiffViewer.Managers;
using DiffViewer.Messages;
using DiffViewer.Models;
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
            if( m is null || m.Message != "LoadRawContent" || m.ObjReplied is not DiffTestCase tc ) return;
            this.DataContext = tc;
            RawRichFlowDocumentReader.Document = FlowDocumentManager.CreateFlowDocument(tc.Raw ?? string.Empty);
            App.Logger.Information<SetRichTextBoxDocumentMessage>($"Show {tc.Name} Raw data window Done" , m);
        });
    }

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

    private void RawRichFlowDocumentReader_PreviewMouseWheel(object sender , System.Windows.Input.MouseWheelEventArgs e)
    {
        if( Keyboard.Modifiers == ModifierKeys.Control )
        {
            if( e.Delta > 0 )
            {
                RawRichFlowDocumentReader.IncreaseZoom();
            }
            else
            {
                RawRichFlowDocumentReader.DecreaseZoom();
            }

            e.Handled = true;
        }
    }


}
