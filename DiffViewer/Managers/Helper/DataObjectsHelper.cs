using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffViewer.Managers.Helper;

public static class DataObjectsHelper
{
    public static T Repeat<T>(this T value , int count)
    {
        if( count <= 0 ) throw new System.NotSupportedException($"count should more than 0, current count is {count} <= 0");

        var sb = new StringBuilder();
        for( int i = 0; i < count; i++ )
        {
            sb.Append(value);
        }

        return (T)(object)sb.ToString();
    }

    public static bool IsDefault<T>(this T Tobject , bool isNullNeedException = false)
    {
        if( Tobject is null && !isNullNeedException ) return true;

        return Tobject.Equals(default(T));
    }

    public static bool IsZero<T>(this T value) where T : struct, System.IEquatable<T>
    {
        return value.Equals(default(T));
    }

    public static bool IsNullOrWhiteSpaceOrEmpty(this string value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static async Task WriteStringsToAsync<T>(this IEnumerable<T> strings , string path , bool isTurnNullToNewLine = true) where T : class?
    {
        var query = isTurnNullToNewLine
                                     ? strings.Select(s => s?.ToString() ?? Environment.NewLine)
                                     : strings.Where(s => s != null).Select(s => s!.ToString());

        await File.WriteAllLinesAsync(path , query).ConfigureAwait(false);
    }

    public static string GetFileName(this string fileFullPath , bool withoutExt = false)
    {
        return withoutExt ? Path.GetFileNameWithoutExtension(fileFullPath) : Path.GetFileName(fileFullPath);
    }

}