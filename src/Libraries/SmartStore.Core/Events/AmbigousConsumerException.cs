using System;
using System.Reflection;

namespace SmartStore.Core.Events
{
    public sealed class AmbigousConsumerException : Exception
    {
        public AmbigousConsumerException(MethodInfo method1, MethodInfo method2)
            : base("Ambigous consumer methods detected in '{0}'. Method 1: '{1}', Method 2: '{2}'.".FormatInvariant(method1.DeclaringType.FullName, method1.ToString(), method2.ToString()))
        {
        }
    }
}
