using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.IO
{
	public static class IFileSystemExtensions
	{
		public static void WriteAllText(this IFileSystem fileSystem, string path, string contents)
		{
			Guard.NotEmpty(path, nameof(path));
			Guard.NotEmpty(contents, nameof(contents));

			if (fileSystem.FileExists(path))
			{
				fileSystem.DeleteFile(path);
			}

			var file = fileSystem.CreateFile(path);
			using (var stream = file.OpenWrite())
			using (var streamWriter = new StreamWriter(stream))
			{
				streamWriter.Write(contents);
			}
		}

		public static async Task WriteAllTextAsync(this IFileSystem fileSystem, string path, string contents)
		{
			Guard.NotEmpty(path, nameof(path));
			Guard.NotEmpty(contents, nameof(contents));

			if (fileSystem.FileExists(path))
			{
				fileSystem.DeleteFile(path);
			}

			var file = fileSystem.CreateFile(path);
			using (var stream = file.OpenWrite())
			using (var streamWriter = new StreamWriter(stream))
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
			using (var streamReader = new StreamReader(stream))
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
			using (var streamReader = new StreamReader(stream))
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
		/// Tries to create a folder in the storage provider.
		/// </summary>
		/// <param name="path">The relative path to the folder to be created.</param>
		/// <returns>True if success; False otherwise.</returns>
		public static bool TryCreateFolder(this IFileSystem fileSystem, string path)
		{
			try
			{
				// prevent unnecessary exception
				if (fileSystem.FolderExists(path))
				{
					return false;
				}

				fileSystem.CreateFolder(path);
			}
			catch
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks whether the name of the file is unique within its directory.
		/// When given file exists, this method appends [1...n] to the file title until
		/// the check returns false.
		/// </summary>
		/// <param name="path">The path of file to check</param>
		/// <param name="uniqueFile">An <see cref="IFile"/> object containing the unique file's info, or <c>null</c> if method returns <c>false</c></param>
		/// <returns>
		/// <c>false</c> when <paramref name="path"/> does not exist yet. <c>true</c> otherwise.
		/// </returns>
		public static bool CheckFileUniqueness(this IFileSystem fileSystem, string path, out IFile uniqueFile)
		{
			Guard.NotEmpty(path, nameof(path));

			uniqueFile = null;

			var file = fileSystem.GetFile(path);
			if (!file.Exists)
			{
				return false;
			}

			var pattern = string.Concat(file.Title, "-*", file.Extension);
			var dir = file.Directory;
			var files = new HashSet<string>(fileSystem.SearchFiles(dir, pattern, false).Select(x => Path.GetFileName(x)));

			int i = 1;
			while (true)
			{
				var newFileName = string.Concat(file.Title, "-", i, file.Extension);
				if (!files.Contains(newFileName))
				{
					// Found our gap
					uniqueFile = fileSystem.GetFile(string.Concat(dir, newFileName));
					return true;
				}

				i++;
			}
		}

		/// <summary>
		/// Retrieves the count of files within a path.
		/// </summary>
		/// <param name="path">The relative path to the folder in which to retrieve file count.</param>
		/// <param name="pattern">The file pattern to match</param>
		/// <param name="deep">Whether to count files in all subfolders also</param>
		/// <returns>Total count of files.</returns>
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
		public static long CountFiles(this IFileSystem fileSystem, string path, bool deep = true)
		{
			return fileSystem.CountFiles(path, "*", null, deep);
		}
	}
}
