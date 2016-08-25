using System;
using System.Threading;

namespace SmartStore.Utilities.Threading
{
    public sealed class WriteLockDisposable : IDisposable
    {
        // Fields
        private readonly ReaderWriterLockSlim _rwLock;

        // Methods
        public WriteLockDisposable(ReaderWriterLockSlim rwLock)
        {
            this._rwLock = rwLock;
        }

        void IDisposable.Dispose()
        {
            this._rwLock.ExitWriteLock();
        }
    }

}
