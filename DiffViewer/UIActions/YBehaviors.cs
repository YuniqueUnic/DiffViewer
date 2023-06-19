using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Input;

namespace DiffViewer.UIActions;


class ClickBehavior : Behavior<UIElement>
{
    protected bool IsBaseCommandExecuted = false;

    #region 依赖属性

    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty , value); }
    }
    public object CommandParameter
    {
        get { return (object)GetValue(CommandParameterProperty); }
        set { SetValue(CommandParameterProperty , value); }
    }

    public int ClickTimes
    {
        get { return (int)GetValue(ClickTimesProperty); }
        set { SetValue(ClickTimesProperty , value); }
    }

    public bool IsEnable
    {
        get { return (bool)GetValue(IsEnableProperty); }
        set { SetValue(IsEnableProperty , value); }
    }


    #endregion 依赖属性

    #region 注册依赖属性

    //注册一个 依赖属性 用来指定Command
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register("Command" ,
            typeof(ICommand) ,
            typeof(ClickBehavior) ,
            new PropertyMetadata(null));

    //注册一个 依赖属性 用来指定 CommandParameter
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            "CommandParameter" ,
            typeof(object) ,
            typeof(ClickBehavior) ,
            new PropertyMetadata(null));

    //定义一个 鼠标点击次数 的依赖属性
    public static readonly DependencyProperty ClickTimesProperty =
        DependencyProperty.Register(
            "ClickTimes" ,
            typeof(int) ,
            typeof(ClickBehavior) ,
            new PropertyMetadata(2));

    //定义一个 是否生效 的依赖属性
    public static readonly DependencyProperty IsEnableProperty =
        DependencyProperty.Register(
            "IsEnable" ,
            typeof(bool) ,
            typeof(ClickBehavior) ,
            new PropertyMetadata(true));


    #endregion 注册依赖属性


    protected override void OnAttached( )
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_MouseDoubleClick;
        AssociatedObject.PreviewMouseLeftButtonDown += ResetLocalVar;
    }

    protected override void OnDetaching( )
    {
        base.OnDetaching();
        AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_MouseDoubleClick;
        AssociatedObject.PreviewMouseLeftButtonDown -= ResetLocalVar;
    }

    protected virtual void AssociatedObject_MouseDoubleClick(object sender , MouseButtonEventArgs e)
    {
        string eventInfos = $"MouseButtonEventArgs:" +
                            $"{Environment.NewLine}Source: {(e.Source as FrameworkElement).Name}" +
                            $"{Environment.NewLine}LeftButton: {e.LeftButton}" +
                            $"{Environment.NewLine}MiddleButton: {e.MiddleButton}" +
                            $"{Environment.NewLine}RightButton: {e.RightButton}" +
                            $"{Environment.NewLine}ClickCount: {e.ClickCount}";

        string commandName = CommandRelayed.GetCommandName(this) ?? "No CommandName Attached.";

        string commandInfos = $"{Environment.NewLine}Command: {commandName}" +
                              $"{Environment.NewLine}CommandParameter: {CommandParameter.ToString()}";

        if( !IsEnable )
        {
            App.Logger.Debug<ClickBehavior>($"IsEnable is false" + eventInfos , this);
            return;
        }

        if( sender is null )
        {
            App.Logger.Error<ClickBehavior>($"Sender is null" + eventInfos , this);
            throw new NullReferenceException($"Sender is null" + eventInfos);
        }
        if( e.ClickCount % ClickTimes == 0 )
        {
            App.Logger.Information<ClickBehavior>(eventInfos , this);

            if( Command != null && Command.CanExecute(CommandParameter) )
            {
                App.Logger.Information<ClickBehavior>($"Click {ClickTimes} times" + commandInfos , this);
                Command.Execute(CommandParameter);
                IsBaseCommandExecuted = true;
            }
        }
    }

    protected virtual void ResetLocalVar(object sender , MouseButtonEventArgs e)
    {
        IsBaseCommandExecuted = false;
        e.Handled = true;
    }
}

public class CopyBehavior : Behavior<UIElement>
{
    #region 依赖属性
    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty , value); }
    }

    public bool IsEnabled
    {
        get { return (bool)GetValue(IsEnabledProperty); }
        set { SetValue(IsEnabledProperty , value); }
    }
    #endregion

    #region 注册依赖属性

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        "Text" ,
        typeof(string) ,
        typeof(CopyBehavior) ,
        new PropertyMetadata("")
    );

    // Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
        "IsEnabled" ,
        typeof(bool) ,
        typeof(CopyBehavior) ,
        new PropertyMetadata(true)
    );
    #endregion

    protected override void OnAttached( )
    {
        AssociatedObject.MouseLeftButtonDown += CopyTextFromTextBox;
    }

    private void CopyTextFromTextBox(object sender , RoutedEventArgs e)
    {

        if( !IsEnabled )
        {
            App.Logger.Warning<CopyBehavior>($"IsEnabled is {IsEnabled}" +
                                                              $"{Environment.NewLine}Failed to copy text to Clipboard" +
                                                              $"{Environment.NewLine}{Text}." , this);
            return;
        }
        Clipboard.SetText(Text);
        App.Logger.Information<CopyBehavior>($"Copy Text to Clipboard:" +
                                                              $"{Environment.NewLine}{Text}" , this);

    }

    protected override void OnDetaching( )
    {
        AssociatedObject.MouseLeftButtonDown -= CopyTextFromTextBox;
    }
}