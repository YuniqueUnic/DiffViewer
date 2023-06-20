using DiffViewer.Managers.Interfaces;
using System;
using System.Runtime.CompilerServices;

namespace DiffViewer.Managers.Helper;

class AttributesManager
{
}



[AttributeUsage(AttributeTargets.Field , AllowMultiple = true)]
public class EnumStringAttribute : Attribute, IAttribute
{
    public string Name { get; private set; }
    public string StringValue { get; private set; }

    public EnumStringAttribute(string stringValue , [CallerMemberName] string name = "")
    {
        Name = name;
        StringValue = stringValue;
    }
}
