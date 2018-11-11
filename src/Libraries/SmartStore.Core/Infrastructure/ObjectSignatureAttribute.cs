using System;

namespace SmartStore
{ 
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)] 
    public sealed class ObjectSignatureAttribute : Attribute
    {
    }
}
