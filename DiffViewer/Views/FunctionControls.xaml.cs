
using CommunityToolkit.Mvvm.Messaging;
using DiffViewer.Messages;
using System.Windows;
using System.Windows.Controls;

namespace DiffViewer.Views;

/// <summary>
/// FunctionControls.xaml 的交互逻辑
/// </summary>
public partial class FunctionControls : UserControl
{
    public FunctionControls( )
    {
        InitializeComponent();

        RegisteMessengers();
    }
    /// <summary>
    /// Registe all messengers.
    /// </summary>
    private void RegisteMessengers( )
    {
        // Set the RichText Content
        WeakReferenceMessenger.Default.Register<UpdateBarChartMessage>(this , (o , m) =>
        {
            if( m is null || m.Sender != this.DataContext ) return;
            if( m.Message == "UpdateBarChart" )
            {
                UpdateBarChart();
            }
        });
    }

    public void Grid_SizeChanged(object sender , SizeChangedEventArgs e)
    {
        UpdateBarChart();
    }

    private void UpdateBarChart( )
    {
        double passCount = GetCount(PassedRect);
        double failCount = GetCount(FailedRect);
        double errorCount = GetCount(SevereErrorRect);
        double totalCount = passCount + failCount + errorCount;

        double restWidth = ActualWidth - ExportMenu.ActualWidth - OpenRawButton.ActualWidth;
        double widthUnit = restWidth / totalCount;

        if( double.IsInfinity(widthUnit) )
        {
            passCount = failCount = errorCount = 1;
            widthUnit = restWidth / 3;
        }

        this.PassColumn.Width = new(widthUnit * passCount);
        this.FailColumn.Width = new(widthUnit * failCount);
        this.ErrorColumn.Width = new(widthUnit * errorCount);

        //PassedRect.Width = widthUnit * passCount;
        //FailedRect.Width = widthUnit * failedCount;
        //SevereErrorRect.Width = widthUnit * severeErrorCount;
    }

    private double GetCount(FrameworkElement element)
    {
        if( element.Tag is null ) { return 0; }

        bool succeed = double.TryParse(element.Tag.ToString() , out double count);

        if( succeed ) { return count; }
        else { return 0; }
    }
}
