using CommunityToolkit.Mvvm.Messaging;
using DiffViewer.Messages;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
            if( m is null || m.Message != "LoadRawContent" || m.ObjReplied is not Models.DiffTestCase tc ) return;
            this.DataContext = tc;
            //RawRichFlowDocumentReader.Document = FlowDocumentManager.CreateFlowDocument(tc.Raw ?? string.Empty);
            App.Logger.Information<SetRichTextBoxDocumentMessage>($"Show {tc.Name} Raw data window Done" , m);
        });
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        //base.OnClosing(e);
        this.Visibility = Visibility.Hidden;
        e.Cancel = true;
    }

    public static int currentSearchIndex = 0;
    public static List<int> AllSearchTextStartIndexes;
    public static int selectionLength = 0;

    // 定义一个函数来搜索文本块中的字符串
    private async void SearchButton_Click(object sender , RoutedEventArgs e)
    {
        // 获取 TextBox 中指定字符串的位置
        if( string.IsNullOrEmpty(SearchTextBox.Text) || string.IsNullOrEmpty(RawTextBox.Text) ) { return; }
        AllSearchTextStartIndexes = await SearchTextByAsync(RawTextBox.Text , SearchTextBox.Text);
        currentSearchIndex = 0;
        selectionLength = SearchTextBox.Text.Length;
        NextSearchButton_Click(sender , e);
    }

    private void LastSearchButton_Click(object sender , RoutedEventArgs e)
    {
        if( AllSearchTextStartIndexes is not null && AllSearchTextStartIndexes.Count > 0 )
        {
            currentSearchIndex = Math.Max(0 , Math.Min(currentSearchIndex , AllSearchTextStartIndexes.Count - 1));

            GoToTextLine(RawTextBox , AllSearchTextStartIndexes[currentSearchIndex] , selectionLength);

            currentSearchIndex--;
        }
    }

    private void NextSearchButton_Click(object sender , RoutedEventArgs e)
    {
        if( AllSearchTextStartIndexes is not null && AllSearchTextStartIndexes.Count > 0 )
        {
            currentSearchIndex = Math.Max(0 , Math.Min(currentSearchIndex , AllSearchTextStartIndexes.Count - 1));

            GoToTextLine(RawTextBox , AllSearchTextStartIndexes[currentSearchIndex] , selectionLength);

            currentSearchIndex++;
        }
    }

    private void GoToTextLine(TextBox textBox , int textStartindex , int selectionlength)
    {
        if( textStartindex < 0 ) { return; }

        // 获取包含字符串的行号和列号
        int lineNumber = textBox.GetLineIndexFromCharacterIndex(textStartindex);
        int columnNumber = textStartindex - textBox.GetCharacterIndexFromLineIndex(lineNumber);

        // 将 TextBox 滚动到包含字符串的行
        textBox.ScrollToLine(lineNumber);

        // 将 TextBox 滚动到包含字符串的列
        double charWidth = new FormattedText(" " ,
            System.Globalization.CultureInfo.CurrentCulture ,
            FlowDirection.LeftToRight ,
            new Typeface(textBox.FontFamily ,
                                 textBox.FontStyle ,
                                 textBox.FontWeight ,
                                 textBox.FontStretch) ,
            textBox.FontSize ,
            Brushes.Black).Width;

        double offset = columnNumber * charWidth;

        textBox.SelectionBrush = Brushes.Cyan;
        textBox.Select(textStartindex , selectionlength);
        textBox.SelectionOpacity = 0.5;
        textBox.ScrollToHorizontalOffset(offset);
    }

    public async Task<List<int>> SearchTextByAsync(string originalText , string searchingText , int num_Segments = 4)
    {
        // Divide the text into segments for concurrent processing
        //int num_Segments = 4; // Number of threads to use
        int segment_size = originalText.Length / num_Segments; // Size of each segment
        string[] segments = new string[num_Segments]; // String for each segment
        int[] segment_starts = new int[num_Segments]; // Starting index for each segment
        string buffer = ""; // String buffer to hold the leftover text from the previous segment

        for( int i = 0; i < num_Segments; i++ )
        {
            int segment_end;
            if( i < num_Segments - 1 )
            {
                segment_starts[i] = i * segment_size;
                segment_end = segment_starts[i] + segment_size;
            }
            else
            {
                segment_starts[num_Segments - 1] = originalText.Length - (num_Segments - 1) * segment_size;
                segment_end = originalText.Length;
            }

            if( i > 0 )
            {
                // Add the leftover text from the previous segment to the beginning of the current segment
                int buffer_index = buffer.LastIndexOf('\n') + 1;
                segments[i] = buffer.Substring(buffer_index) + originalText.Substring(segment_starts[i] , segment_end - segment_starts[i]);
                buffer = buffer.Substring(0 , buffer_index);
            }
            else
            {
                segments[i] = originalText.Substring(segment_starts[i] , segment_end - segment_starts[i]);
            }
            if( i < num_Segments - 1 )
            {
                // Save the leftover text from the end of the current segment to the buffer
                int buffer_index = segments[i].LastIndexOf('\n') + 1;
                buffer += segments[i].Substring(buffer_index);
                segments[i] = segments[i].Substring(0 , buffer_index);
            }
        }

        // Use Task.WhenAll to search for the string concurrently
        List<int>[] results = await Task.WhenAll(
            Enumerable.Range(0 , num_Segments)
                .Select(async i =>
                {
                    List<int> segment_results = new List<int>();
                    int segment_start = segment_starts[i];
                    int segment_end = i == num_Segments - 1 ? originalText.Length : segment_starts[i + 1];
                    int result_index = segments[i].IndexOf(searchingText);
                    while( result_index != -1 )
                    {
                        segment_results.Add(segment_start + result_index);
                        result_index = segments[i].IndexOf(searchingText , result_index + 1);
                    }
                    await Task.Delay(0); // add this line to avoid the warning
                    return segment_results;
                })
        );

        // Merge the results from each segment
        List<int> all_results = new List<int>();
        for( int i = 0; i < num_Segments; i++ ) { all_results.AddRange(results[i]); }

        return all_results;
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


    private async void OpenTextByNotepadButton_Click(object sender , RoutedEventArgs e)
    {
        if( !string.IsNullOrEmpty(RawTextBox.Text) )
        {
            await ShowTextInNotepadByTempAsync(RawTextBox.Text , waitNotepadExit: false);
        }

    }

    public static async Task ShowTextInNotepadDirectAsync(string text , bool waitNotepadExit = false)
    {
        await Task.Run(( ) =>
        {
            // 启动 Notepad 进程
            Process notepad = new Process();
            notepad.StartInfo.FileName = "notepad.exe";

            // 将字符串作为命令行参数传递给 Notepad
            notepad.StartInfo.Arguments = "\"" + text + "\"";

            notepad.Start();
            if( waitNotepadExit )
            {
                notepad.WaitForExit();
            }
        });
    }

    public static async Task ShowTextInNotepadByTempAsync(string text , bool waitNotepadExit = false)
    {
        await Task.Run(( ) =>
        {
            // 将字符串写入临时文本文件中
            string tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath , text);

            // 启动 Notepad 进程
            Process notepad = new Process();
            notepad.StartInfo.FileName = "notepad.exe";

            // 将临时文件路径作为命令行参数传递给 Notepad
            notepad.StartInfo.Arguments = "\"" + tempFilePath + "\"";

            try
            {
                notepad.Start();
                notepad.WaitForExit();
            }
            catch( Win32Exception ex )
            {
                if( ex.NativeErrorCode == 2 ) // 文件不存在
                {
                    // 弹窗提示用户选择是否创建新文件
                    MessageBoxResult result = MessageBox.Show("找不到临时文件，是否创建新文件？" , "错误" , MessageBoxButton.YesNo , MessageBoxImage.Error);

                    if( result == MessageBoxResult.Yes )
                    {
                        // 创建新文件并重试
                        tempFilePath = Path.Combine(Path.GetTempPath() , Path.GetRandomFileName());
                        File.WriteAllText(tempFilePath , text);

                        notepad.StartInfo.Arguments = "\"" + tempFilePath + "\"";
                        notepad.Start();
                        if( waitNotepadExit )
                        {
                            notepad.WaitForExit();
                        }
                    }
                    else
                    {
                        // 用户取消操作，删除临时文件
                        File.Delete(tempFilePath);
                    }
                }
                else
                {
                    // 其他错误，删除临时文件
                    File.Delete(tempFilePath);
                    throw;
                }
            }
            finally
            {
                // 删除临时文件
                File.Delete(tempFilePath);
            }
        });
    }


    //private void RawRichFlowDocumentReader_PreviewMouseWheel(object sender , System.Windows.Input.MouseWheelEventArgs e)
    //{
    //    if( Keyboard.Modifiers == ModifierKeys.Control )
    //    {
    //        if( e.Delta > 0 )
    //        {
    //            RawRichFlowDocumentReader.IncreaseZoom();
    //        }
    //        else
    //        {
    //            RawRichFlowDocumentReader.DecreaseZoom();
    //        }

    //        e.Handled = true;
    //    }
    //}

}
