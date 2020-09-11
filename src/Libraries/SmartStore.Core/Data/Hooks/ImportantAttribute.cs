using System;

namespace SmartStore.Core.Data.Hooks
{
    /// <summary>
    /// Indicates that a hook instance should run in any case, even when hooking has been turned off.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ImportantAttribute : Attribute
    {
    }
}
