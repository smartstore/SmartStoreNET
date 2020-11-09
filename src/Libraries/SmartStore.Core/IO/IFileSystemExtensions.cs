using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.IO
{
    public static class IFileSystemExtensions
    {
        public static void WriteAllText(this IFileSystem fileSystem, string path, string contents, Encoding encoding = null)
        {
            Guard.NotEmpty(path, nameof(path));
            Guard.NotEmpty(contents, nameof(contents));

            if (fileSystem.FileExists(path))
            {
                fileSystem.DeleteFile(path);
            }

            var file = fileSystem.CreateFile(path);

            using (var stream = file.OpenWrite())
            using (var streamWriter = new StreamWriter(stream, encoding ?? new UTF8Encoding(false, true)))
            {
                streamWriter.Write(contents);
            }
        }

        public static async Task WriteAllTextAsync(this IFileSystem fileSystem, string path, string contents, Encoding encoding = null)
        {
            Guard.NotEmpty(path, nameof(path));
            Guard.NotEmpty(contents, nameof(contents));

            if (fileSystem.FileExists(path))
            {
                fileSystem.DeleteFile(path);
            }

            var file = fileSystem.CreateFile(path);
            using (var stream = file.OpenWrite())
            using (var streamWriter = new StreamWriter(stream, encoding ?? new UTF8Encoding(false, true)))
            {
                await streamWriter.WriteAsync(contents);
            }
        }

        public static string ReadAllText(this IFileSystem fileSystem, string path)
        {
            Guard.NotEmpty(path, nameof(path));

            if (!fileSystem.FileExists(path))
            {
                return String.Empty;
            }

            var file = fileSystem.GetFile(path);
            using (var stream = file.OpenRead())
            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
            {
                return streamReader.ReadToEnd();
            }
        }

        public static async Task<string> ReadAllTextAsync(this IFileSystem fileSystem, string path)
        {
            Guard.NotEmpty(path, nameof(path));

            if (!fileSystem.FileExists(path))
            {
                return String.Empty;
            }

            var file = fileSystem.GetFile(path);
            using (var stream = file.OpenRead())
            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
            {
                return await streamReader.ReadToEndAsync();
            }
        }


        public static void WriteAllBytes(this IFileSystem fileSystem, string path, byte[] contents)
        {
            Guard.NotEmpty(path, nameof(path));
            Guard.NotNull(contents, nameof(contents));

            if (fileSystem.FileExists(path))
            {
                fileSystem.DeleteFile(path);
            }

            var file = fileSystem.CreateFile(path);
            using (var stream = file.OpenWrite())
            {
                stream.Write(contents, 0, contents.Length);
            }
        }

        public static async Task WriteAllBytesAsync(this IFileSystem fileSystem, string path, byte[] contents)
        {
            Guard.NotEmpty(path, nameof(path));
            Guard.NotNull(contents, nameof(contents));

            if (fileSystem.FileExists(path))
            {
                fileSystem.DeleteFile(path);
            }

            var file = await fileSystem.CreateFileAsync(path);
            using (var stream = file.OpenWrite())
            {
                await stream.WriteAsync(contents, 0, contents.Length);
            }
        }

        public static byte[] ReadAllBytes(this IFileSystem fileSystem, string path)
        {
            Guard.NotEmpty(path, nameof(path));

            if (!fileSystem.FileExists(path))
            {
                return null;
            }

            var file = fileSystem.GetFile(path);
            using (var stream = file.OpenRead())
            {
                return stream.ToByteArray();
            }
        }

        public static async Task<byte[]> ReadAllBytesAsync(this IFileSystem fileSystem, string path)
        {
            Guard.NotEmpty(path, nameof(path));

            if (!fileSystem.FileExists(path))
            {
                return null;
            }

            var file = fileSystem.GetFile(path);
            using (var stream = file.OpenRead())
            {
                return await stream.ToByteArrayAsync();
            }
        }


        /// <summary>
        /// Tries to save a stream in the storage provider.
        /// </summary>
        /// <param name="path">The relative path to the file to be created.</param>
        /// <param name="inputStream">The stream to be saved.</param>
        /// <returns>True if success; False otherwise.</returns>
        public static bool TrySaveStream(this IFileSystem fileSystem, string path, Stream inputStream)
        {
            try
            {
                if (fileSystem.FileExists(path))
                {
                    return false;
                }

                fileSystem.SaveStream(path, inputStream);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Asynchronously tries to save a stream in the storage provider.
        /// </summary>
        /// <param name="path">The relative path to the file to be created.</param>
        /// <param name="inputStream">The stream to be saved.</param>
        /// <returns>True if success; False otherwise.</returns>
        public static async Task<bool> TrySaveStreamAsync(this IFileSystem fileSystem, string path, Stream inputStream)
        {
            try
            {
                if (fileSystem.FileExists(path))
                {
                    return false;
                }

                await fileSystem.SaveStreamAsync(path, inputStream);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves the count of files within a path.
        /// </summary>
        /// <param name="path">The relative path to the folder in which to retrieve file count.</param>
        /// <param name="pattern">The file pattern to match</param>
        /// <param name="deep">Whether to count files in all subfolders also</param>
        /// <returns>Total count of files.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CountFiles(this IFileSystem fileSystem, string path, string pattern, bool deep = true)
        {
            return fileSystem.CountFiles(path, pattern, null, deep);
        }

        /// <summary>
        /// Retrieves the count of files within a path.
        /// </summary>
        /// <param name="path">The relative path to the folder in which to retrieve file count.</param>
        /// <param name="deep">Whether to count files in all subfolders also</param>
        /// <returns>Total count of files.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CountFiles(this IFileSystem fileSystem, string path, bool deep = true)
        {
            return fileSystem.CountFiles(path, "*", null, deep);
        }
    }
}
