using System.Text;

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
}