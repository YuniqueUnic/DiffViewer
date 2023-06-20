using CommunityToolkit.Mvvm.Messaging;
using DiffViewer.Messages;
using System.Windows;
using System.Windows.Documents;

namespace DiffViewer.Views
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow( )
        {
            InitializeComponent();

            //this.DataContext = App.ViewModelLocator.About_ViewModel;

            RegisteMessengers();

            App.LanguageChanged += App_LanguageChanged;
        }

        private void App_LanguageChanged(object? sender , LanguageChangedEventArgs e)
        {

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
                    case $"Show{nameof(AboutWindow)}":
                        this.Activate();
                        break;
                    default:
                        App.Logger.Warning<WindowActionMessage>($"Unknown WindowActionMessage: {m.Message}" , m);
                        break;
                }

                App.Logger.Debug<WindowActionMessage>($"WindowActionMessage: {m.Message} Done" , m);
            });

            // Set the RichText Content
            WeakReferenceMessenger.Default.Register<SetRichTextBoxDocumentMessage>(this , (o , m) =>
            {
                if( m is null || m.Sender != this.DataContext ) return;
                AboutContentRichFlowDocumentReader.Document = (m.ObjReplied as FlowDocument);
                App.Logger.Information<SetRichTextBoxDocumentMessage>($"SetRichTextBoxDocumentMessage Done" , m);
            });
        }



    }
}
