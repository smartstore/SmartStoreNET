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

	}
}
