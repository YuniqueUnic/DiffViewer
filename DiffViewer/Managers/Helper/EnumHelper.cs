using DiffViewer.Managers.Interfaces;
using System;

namespace DiffViewer.Managers.Helper;

public enum LicenseType
{
    [EnumString("MIT")]
    MIT,

    [EnumString("Apache-2.0")]
    Apache2,
}

public static class EnumHelper
{
    public static string GetString<T>(this Enum value) where T : IAttribute
    {
        var type = value.GetType();
        var name = Enum.GetName(type , value);
        var field = type.GetField(name);
        var enumStringAttribute = field.GetCustomAttributes(typeof(T) , false) as T[];

        if( enumStringAttribute != null && enumStringAttribute.Length > 0 )
        {
            return enumStringAttribute[0].StringValue;
        }

        return name;
    }

    public static T GetEnum<T>(this string stringValue) where T : IAttribute
    {
        var type = typeof(T);
        if( !type.IsEnum )
        {
            throw new ArgumentException("Type provided must be an Enum");
        }

        foreach( var field in type.GetFields() )
        {
            var enumStringAttribute = field.GetCustomAttributes(typeof(T) , false) as T[];

            if( enumStringAttribute != null && enumStringAttribute.Length > 0 && enumStringAttribute[0].StringValue == stringValue )
            {
                return (T)field.GetValue(null);
            }

            if( field.Name == stringValue )
            {
                return (T)field.GetValue(null);
            }
        }

        throw new ArgumentException("Invalid string value for enum");
    }

    public static string GetName<T>(this Enum value , string stringValue) where T : IAttribute
    {
        var type = value.GetType();
        var name = Enum.GetName(type , value);
        var field = type.GetField(name);
        var enumStringAttributes = field.GetCustomAttributes(typeof(T) , false) as T[];

        if( enumStringAttributes != null && enumStringAttributes.Length > 0 )
        {
            foreach( var enumStringAttribute in enumStringAttributes )
            {
                if( enumStringAttribute.StringValue == stringValue )
                {
                    return enumStringAttribute.Name;
                }
            }
        }

        return name;
    }

    public static string GetString<T>(this Enum value , string attributeName) where T : IAttribute
    {
        var type = value.GetType();
        var name = Enum.GetName(type , value);
        var field = type.GetField(name);
        var enumStringAttributes = field.GetCustomAttributes(typeof(T) , false) as T[];

        if( enumStringAttributes != null && enumStringAttributes.Length > 0 )
        {
            foreach( var enumStringAttribute in enumStringAttributes )
            {
                if( enumStringAttribute.Name == attributeName )
                {
                    return enumStringAttribute.StringValue;
                }
            }
        }

        return name;
    }

    public static T GetEnum<T>(this string stringValue , string attributeName) where T : IAttribute
    {
        var type = typeof(T);
        if( !type.IsEnum )
        {
            throw new ArgumentException("Type provided must be an Enum");
        }

        foreach( var field in type.GetFields() )
        {
            var enumStringAttributes = field.GetCustomAttributes(typeof(T) , false) as T[];

            if( enumStringAttributes != null && enumStringAttributes.Length > 0 )
            {
                foreach( var enumStringAttribute in enumStringAttributes )
                {
                    if( enumStringAttribute.Name == attributeName && enumStringAttribute.StringValue == stringValue )
                    {
                        return (T)field.GetValue(null);
                    }
                }
            }

            if( field.Name == stringValue )
            {
                return (T)field.GetValue(null);
            }
        }

        throw new ArgumentException("Invalid string value for enum");
    }

}