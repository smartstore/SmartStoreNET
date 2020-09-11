using System;
using System.Globalization;
using System.IO;
using System.Threading;
using SmartStore.Utilities.Threading;

namespace SmartStore.Core.IO
{
    public class LockFileManager : ILockFileManager
    {
        private readonly IApplicationEnvironment _env;
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        static LockFileManager()
        {
            Expiration = TimeSpan.FromMinutes(10);
        }

        public LockFileManager(IApplicationEnvironment env)
        {
            _env = env;
        }

        public static TimeSpan Expiration
        {
            get;
            private set;
        }

        public bool TryAcquireLock(string path, out ILockFile lockFile)
        {
            lockFile = null;

            if (!_rwLock.TryEnterWriteLock(0))
            {
                return false;
            }

            try
            {
                if (IsLockedInternal(path))
                {
                    return false;
                }

                lockFile = new LockFile(_env.TenantFolder, path, DateTime.UtcNow.ToString("u"), _rwLock);
                return true;
            }
            catch
            {
                // an error occured while reading/creating the lock file
                return false;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public bool IsLocked(string path)
        {
            using (_rwLock.GetWriteLock())
            {
                try
                {
                    return IsLockedInternal(path);
                }
                catch
                {
                    // an error occured while reading the file
                    return true;
                }
            }
        }

        private bool IsLockedInternal(string path)
        {
            // INFO: VirtualPathProvider caches file existence info, so not very reliable here.
            if (File.Exists(_env.TenantFolder.MapPath(path)))
            {
                var content = _env.TenantFolder.ReadFile(path);

                if (DateTime.TryParse(content, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var creationUtc))
                {
                    // if expired the file is not removed
                    // it should be automatically as there is a finalizer in LockFile
                    // or the next taker can do it, unless it also fails, again
                    return creationUtc.ToUniversalTime().Add(Expiration) > DateTime.UtcNow;
                }
            }

            return false;
        }
    }
}
