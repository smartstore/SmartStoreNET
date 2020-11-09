using System;

namespace SmartStore.Core.IO
{
    /// <summary>
    /// Represents a Lock File acquired on the file system
    /// </summary>
    /// <remarks>
    /// The instance needs to be disposed in order to release the lock explicitly
    /// </remarks>
    public interface ILockFile : IDisposable
    {
        void Release();
    }
}
