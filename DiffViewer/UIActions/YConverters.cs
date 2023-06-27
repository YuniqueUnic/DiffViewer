using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace DiffViewer.UIActions;

/// <summary>
/// 将 double 类型的值按比例缩放。
/// </summary>
public class DoubleValueScaleConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// 将 double 类型的值按比例缩放。
    /// </summary>
    /// <param name="value">要缩放的值。</param>
    /// <param name="targetType">缩放后的类型。</param>
    /// <param name="parameter">缩放比例。可以是 double 类型的值或表示 double 值的字符串。</param>
    /// <param name="culture">转换器的区域性。</param>
    /// <returns>缩放后的值。</returns>
    public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
    {
        // 将参数 value 强制转换为 double? 类型，如果转换失败，则将其设置为 null。
        double? fontSize = value as double? ?? (double.TryParse(value?.ToString() , out double parsedValue) ? parsedValue : default(double?));

        if( fontSize == null )
        {
            return null!;
        }

        switch( parameter )
        {
            // 如果 parameter 是表示 double 值的字符串，则将其解析为 double 类型并缩放 fontSize 的值。
            case string scaleString:
                if( double.TryParse(scaleString , out double scaleTimes) )
                {
                    return fontSize * scaleTimes;
                }
                break;
            case double scaleTimes_1:
                return fontSize * scaleTimes_1;
            // 如果 parameter 为空或不是上述两种类型，则将 fontSize 的值乘以 1.5 缩放。
            default:
                return fontSize * 1.5;
        }

        return null!;
    }

    public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}

public class BooleanInverterConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
    {
        if( value is bool )
        {
            return !(bool)value;
        }
        return value;
    }

    public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
    {
        if( value is bool )
        {
            return !(bool)value;
        }
        return value;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}

[MarkupExtensionReturnType(typeof(IValueConverter))]
public class BoolenNullableToVisibility : MarkupExtension, IValueConverter
{
    public bool Reverse { get; set; }
    public bool IsEnabled { get; set; }

    public BoolenNullableToVisibility( )
    {
        Reverse = false;
        IsEnabled = true;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    /// <summary>
    /// 将 bool? 转换为 Visibility 值。
    /// </summary>
    /// <param name="value">要转换的值。</param>
    /// <param name="targetType">转换后的类型。</param>
    /// <param name="parameter">转换器的参数。</param>
    /// <param name="culture">转换器的区域性。</param>
    /// <returns>转换后的值。</returns>
    public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
    {
        // 根据 IsEnabled 属性返回适当的 Visibility 值。
        if( !IsEnabled )
        {
            return Visibility.Hidden;
        }

        // 将参数 value 强制转换为 bool? 类型。
        bool? isVisible = (bool?)value;

        if( !isVisible.HasValue ) { return Visibility.Hidden; }

        // 根据 Reverse 属性反转布尔值。
        if( isVisible.HasValue && Reverse )
        {
            isVisible = !isVisible;
        }

        // 如果 isVisible 为 true，则返回 Visibility.Visible，否则返回 Visibility.Collapsed。
        return isVisible.HasValue && isVisible.Value ? Visibility.Visible : Visibility.Collapsed;
    }


    /// <summary>
    /// 将 Visibility 的三种状态（Visible、Collapsed 和 Hidden）转换为 bool? 类型的值。
    /// </summary>
    /// <param name="value">要转换的值。</param>
    /// <param name="targetType">转换后的类型。</param>
    /// <param name="parameter">转换器的参数。</param>
    /// <param name="culture">转换器的区域性。</param>
    /// <returns>转换后的值。</returns>
    public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
    {
        // 将参数 value 强制转换为 Visibility 类型。
        Visibility visibility = (Visibility)value;

        // 如果 IsEnabled 为 false，返回 null。
        if( !IsEnabled )
        {
            return null;
        }

        // 根据 Reverse 属性反转布尔值。
        switch( visibility )
        {
            case Visibility.Visible:
                return Reverse ? (bool?)false : true;
            case Visibility.Hidden:
                return null;
            case Visibility.Collapsed:
                return Reverse ? (bool?)true : false;
            default:
                throw new NotSupportedException($"Invalid visibility value {value}.");
        }
    }
}

/// <summary>
/// NullableBool To Color Converter
/// Default: TrueBrush = Transparent, FalseBrush = Red, NullBrush = Red
/// </summary>
public class NullableBoolToColorConverter : MarkupExtension, IValueConverter
{
    public Brush TrueBrush { get; set; } = Brushes.Transparent;
    public Brush FalseBrush { get; set; } = Brushes.Red;
    public Brush NullBrush { get; set; } = Brushes.Red;
    public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
    {
        return value switch
        {
            bool a => a ? TrueBrush : FalseBrush,
            _ => NullBrush,
        };


        //if( value is null )
        //{
        //    return Brushes.Red;
        //}

        //if( value is bool isIdentical )
        //{
        //    if( isIdentical == true )
        //    {
        //        return Brushes.Transparent;
        //    }
        //    else
        //    {
        //        return Brushes.Red;
        //    }
        //}

        //return Brushes.Transparent;
    }

    public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}


/// <summary>
/// NullableBool To FontWeight
/// Default: TrueFontWeight = Normal, FalseFontWeight = Normal, NullFontWeight = Bold
/// </summary>
public class NullableBoolToFontWeightConverter : MarkupExtension, IValueConverter
{
    public FontWeight TrueFontWeight { get; set; } = FontWeights.Normal;
    public FontWeight FalseFontWeight { get; set; } = FontWeights.Normal;
    public FontWeight NullFontWeight { get; set; } = FontWeights.Bold;


    public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
    {
        return value switch
        {
            bool a => a ? TrueFontWeight : FalseFontWeight,
            _ => NullFontWeight,
        };

        //if( value is null )
        //{
        //    return NullFontWeight;
        //}

        //if( value is bool isIdentical )
        //{
        //    if( isIdentical == false )
        //    {
        //        return FalseFontWeight;
        //    }
        //}

        //return TrueFontWeight;
    }

    public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}


/// <summary>
/// For three state Image checkbox
/// </summary>
public class IsCheckedToVisibilityConverter : IValueConverter
{
    public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
    {
        var isChecked = value as bool?;

        if( isChecked == null )
        {
            return parameter.ToString() == "Null" ? Visibility.Visible : Visibility.Collapsed;
        }
        else if( isChecked == true )
        {
            return parameter.ToString() == "True" ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            return parameter.ToString() == "False" ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


/// <summary>
/// Work with <i:Interaction.Triggers>
/// <i:EventTrigger EventName = "MouseDoubleClick" > can bind to ViewModel
/// EventArgsConverter="{StaticResource MouseButtonClickToBool}"
/// Command="{Binding Command}"
/// PassEventArgsToCommand="True"
/// </summary>
public class MouseButtonEventArgsToBoolConverter : MarkupExtension, IValueConverter
{
    public MouseButton ClickButton { get; set; } = MouseButton.Left;
    public bool IsReverse { get; set; } = false;

    public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
    {

        if( value is not MouseButtonEventArgs mouseEventArgs ) { throw new NotSupportedException(); }

        var location = $"({nameof(MouseButtonEventArgsToBoolConverter)}).";

        bool result = false;

        if( mouseEventArgs.ChangedButton == this.ClickButton )
        {
            if( !IsReverse )
            {
                result = true;
            }
            else
            {
                result = false;
            }
        }

        location += $"(EventName: {mouseEventArgs.RoutedEvent.Name}).(ClickButton: {this.ClickButton}).(Return bool: {result})";
        App.Logger.Information<MouseButtonEventArgsToBoolConverter>(location , this);

        return result;
    }


    public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}

/// <summary>
/// For ListView SelectedItem Background
/// </summary>
public class IndexAndTextToBackgroundConverter : MarkupExtension, IMultiValueConverter
{
    System.Windows.Media.SolidColorBrush EqualBrush { get; set; } = (SolidColorBrush)App.Current.FindResource("ThemePrimary") ?? Brushes.LightSeaGreen;
    System.Windows.Media.SolidColorBrush NotEqualBrush { get; set; } = Brushes.Transparent;

    public object Convert(object[] values , Type targetType , object parameter , CultureInfo culture)
    {

        if( values.Length == 2 && values[0] is int index && values[1] is string text )
        {
            return index + 1 == int.Parse(text) ? EqualBrush : NotEqualBrush;
        }

        return NotEqualBrush;
    }

    public object[] ConvertBack(object value , Type[] targetTypes , object parameter , CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}


[MarkupExtensionReturnType(typeof(IValueConverter))]
public class BoolenNullableToProgressVisibility : MarkupExtension, IValueConverter
{
    public bool Reverse { get; set; }
    public bool IsEnabled { get; set; }

    public BoolenNullableToProgressVisibility( )
    {
        Reverse = false;
        IsEnabled = true;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    /// <summary>
    /// 将 bool? 转换为 Visibility 值。
    /// </summary>
    /// <param name="value">要转换的值。</param>
    /// <param name="targetType">转换后的类型。</param>
    /// <param name="parameter">转换器的参数。</param>
    /// <param name="culture">转换器的区域性。</param>
    /// <returns>转换后的值。</returns>
    public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
    {
        // 根据 IsEnabled 属性返回适当的 Visibility 值。
        if( !IsEnabled )
        {
            return Visibility.Hidden;
        }

        // 将参数 value 强制转换为 bool? 类型。
        bool? isVisible = (bool?)value;

        if( !isVisible.HasValue ) { return Visibility.Visible; }

        // 根据 Reverse 属性反转布尔值。
        if( Reverse )
        {
            isVisible = !isVisible;
        }

        // 如果 isVisible 为 true，则返回 Visibility.Visible，否则返回 Visibility.Collapsed。
        return isVisible.Value ? Visibility.Visible : Visibility.Collapsed;
    }


    public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}