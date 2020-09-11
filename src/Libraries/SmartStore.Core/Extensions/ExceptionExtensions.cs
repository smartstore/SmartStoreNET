using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace SmartStore
{
    public static class ExceptionExtensions
    {
        public static bool IsFatal(this Exception ex)
        {
            return ex is StackOverflowException ||
                ex is OutOfMemoryException ||
                ex is AccessViolationException ||
                ex is AppDomainUnloadedException ||
                ex is ThreadAbortException ||
                ex is SecurityException ||
                ex is SEHException;
        }
    }
}
