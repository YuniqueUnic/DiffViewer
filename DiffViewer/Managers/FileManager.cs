using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffViewer.Managers;

class FileManager
{
    public static bool CheckFileExists(string filePath , bool throwExpection = true)
    {
        try
        {
            if( !File.Exists(filePath) )
            {

                App.Logger.Error($"File does not exist: `{filePath}`");
                if( throwExpection ) { throw new FileNotFoundException($"File can't be found!: {filePath}"); }
                return false;
            }
            return true;
        }
        catch( Exception e )
        {
            App.Logger.Error(e , $"Error on checking file exists: `{filePath}`");
            throw;
        }
    }

    public static async Task<(int, string?)> GetTextInfoAsync(string filePath)
    {
        int lineCount = 0;

        try
        {
            App.Logger.Information($"Reading content from file `{filePath}`.");
            using( var fileStream = new FileStream(filePath , FileMode.Open , FileAccess.Read , FileShare.ReadWrite , 4096 , true) )
            using( var streamReader = new StreamReader(fileStream) )
            {
                // 创建缓冲区和 StringBuilder 对象
                int bufferSize = 1024;
                char[] buffer = new char[bufferSize];
                StringBuilder stringBuilder = new StringBuilder();

                // 分块读取文件并异步处理每个块
                while( streamReader.Peek() >= 0 )
                {
                    int readSize = await streamReader.ReadAsync(buffer , 0 , bufferSize);
                    stringBuilder.Append(buffer , 0 , readSize);
                    // 计算换行符数量
                    lineCount += buffer.Take(readSize).Count(c => c == '\n');
                }
                App.Logger.Information($"Content read from file `{filePath}` Done. Total line: '{lineCount}'");
                // 返回文件内容字符串和总行数
                return (lineCount, stringBuilder.ToString());
            }
        }
        catch( Exception )
        {
            App.Logger.Error($"Error on reading content from file `{filePath}` . Current line: '{lineCount}'");
            throw;
        }
    }

    public static async Task WriteToAsync(string context , string path)
    {
        try
        {
            // if the directory does not exist, create it.
            string directoryName = Path.GetDirectoryName(path);

            if( !Directory.Exists(directoryName) )
            {
                Directory.CreateDirectory(directoryName);
            }

            App.Logger.Information($"Writing content to file `{path}`.");
            using( var fileStream = new FileStream(path , FileMode.OpenOrCreate , FileAccess.Write , FileShare.ReadWrite , 4096 , true) )
            using( var streamWriter = new StreamWriter(fileStream , Encoding.Default , 1024) )
            {
                await streamWriter.WriteAsync(context).ConfigureAwait(false);
            }
        }
        catch( Exception ex )
        {
            App.Logger.Error($"Failed to write content to file `{path}`. Error message: {ex.Message}");
            throw;
        }
        App.Logger.Information($"Content written to file `{path}`.");
    }

    public static string GetFileName(string path)
    {
        return System.IO.Path.GetFileName(path);
    }
    public static string GetFileContent(string path)
    {
        return System.IO.File.ReadAllText(path);
    }
}
