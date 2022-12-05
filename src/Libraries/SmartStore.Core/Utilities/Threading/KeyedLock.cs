using AsyncKeyedLock;
using System;

namespace SmartStore.Utilities.Threading
{
    public sealed class KeyedLock
    {
        private static readonly Lazy<AsyncKeyedLocker<string>> lazy = new Lazy<AsyncKeyedLocker<string>>(() => new AsyncKeyedLocker<string>());
        public static AsyncKeyedLocker<string> Instance
        {
            get
            {
                return lazy.Value;
            }
        }
    }
}
