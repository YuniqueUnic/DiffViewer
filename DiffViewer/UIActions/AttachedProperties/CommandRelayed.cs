using System.Windows;

namespace DiffViewer.UIActions;

class CommandRelayed : DependencyObject
{
    public static string GetCommandName(DependencyObject obj)
    {
        return (string)obj.GetValue(CommandNameProperty);
    }

    public static void SetCommandName(DependencyObject obj , object value)
    {
        obj.SetValue(CommandNameProperty , value.ToString());
    }

    // Using a DependencyProperty as the backing store for CommandName.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CommandNameProperty =
        DependencyProperty.RegisterAttached("CommandName" ,
            typeof(string) ,
            typeof(CommandRelayed) ,
            new PropertyMetadata(""));

}
