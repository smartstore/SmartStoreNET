using System;
using System.IO;
using System.Threading.Tasks;

namespace SmartStore
{
    public static class IOExtensions
    {
        public static bool WaitForUnlock(this FileInfo file, int timeoutMs = 1000)
        {
            Guard.NotNull(file, nameof(file));

            var wait = TimeSpan.FromMilliseconds(50);
            var attempts = Math.Floor(timeoutMs / wait.TotalMilliseconds);

            try
            {
                for (var i = 0; i < attempts; i++)
                {
                    if (!IsFileLocked(file))
                    {
                        return true;
                    }

                    Task.Delay(wait).Wait();
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsFileLocked(this FileInfo file)
        {
            if (file == null)
                return false;

            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                // The file is unavailable because it is:
                // still being written to
                // or being processed by another thread
                // or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            // File is not locked
            return false;
        }
    }
}
