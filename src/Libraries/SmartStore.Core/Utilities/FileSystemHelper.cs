using System;
using System.IO;
using System.Threading;

namespace SmartStore.Utilities
{
	public static class FileSystemHelper
	{
		/// <summary>
		/// Returns physical path to temp directory
		/// </summary>
		/// <param name="subDirectory">Name of a sub directory to be created and returned (optional)</param>
		public static string TempDir(string subDirectory = null)
		{
			string path = CommonHelper.GetAppSetting<string>("sm:TempDirectory", "~/App_Data/_temp");
			path = CommonHelper.MapPath(path);

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			if (subDirectory.HasValue())
			{
				path = Path.Combine(path, subDirectory);

				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
			}

			return path;
		}

		/// <summary>
		/// Safe way to cleanup the temp directory. Should be called via scheduled task.
		/// </summary>
		public static void TempCleanup()
		{
			try
			{
				string dir = FileSystemHelper.TempDir();

				if (Directory.Exists(dir))
				{
					FileInfo fi;
					var oldestDate = DateTime.Now.Subtract(new TimeSpan(0, 5, 0, 0));
					var files = Directory.EnumerateFiles(dir);

					foreach (string file in files)
					{
						fi = new FileInfo(file);

						if (fi != null && fi.LastWriteTime < oldestDate)
							FileSystemHelper.Delete(file);
					}
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
		}

		/// <summary>
		/// Safe way to delete a file.
		/// </summary>
		public static bool Delete(string path)
		{
			if (path.IsEmpty())
				return true;

			bool result = true;
			try
			{
				if (Directory.Exists(path))
				{
					throw new MemberAccessException("Deleting folders due to security reasons not possible: {0}".FormatWith(path));
				}

				File.Delete(path);	// no exception, if file doesn't exists
			}
			catch (Exception exc)
			{
				result = false;
				exc.Dump();
			}
			return result;
		}

		/// <summary>
		/// Safe way to copy a file.
		/// </summary>
		public static bool Copy(string sourcePath, string destinationPath, bool overwrite = true, bool deleteSource = false)
		{
			bool result = true;
			try
			{
				File.Copy(sourcePath, destinationPath, overwrite);

				if (deleteSource)
					Delete(sourcePath);
			}
			catch (Exception exc)
			{
				result = false;
				exc.Dump();
			}
			return result;
		}

		/// <summary>
		/// Safe way to copy a directory and all content.
		/// </summary>
		/// <param name="source">Source directory</param>
		/// <param name="target">Target directory</param>
		/// <param name="overwrite">Whether to override existing files</param>
		public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target, bool overwrite = true)
		{
			foreach (FileInfo fi in source.GetFiles())
			{
				try
				{
					fi.CopyTo(Path.Combine(target.ToString(), fi.Name), overwrite);
				}
				catch (Exception exc)
				{
					exc.Dump();
				}
			}

			foreach (DirectoryInfo sourceSubDir in source.GetDirectories())
			{
				try
				{
					DirectoryInfo targetSubDir = target.CreateSubdirectory(sourceSubDir.Name);
					CopyDirectory(sourceSubDir, targetSubDir, overwrite);
				}
				catch (Exception exc)
				{
					exc.Dump();
				}
			}
		}

		/// <summary>
		/// Safe way to delete all directory content
		/// </summary>
		/// <param name="directoryPath">A directory path</param>
		public static void ClearDirectory(string directoryPath, bool selfToo)
		{
			if (directoryPath.IsEmpty())
				return;

			try
			{
				var dir = new DirectoryInfo(directoryPath);

				foreach (var fi in dir.GetFiles())
				{
					try
					{
						fi.IsReadOnly = false;
						fi.Delete();
					}
					catch (Exception)
					{
						try
						{
							Thread.Sleep(0);
							fi.Delete();
						}
						catch (Exception) { }
					}
				}

				foreach (var di in dir.GetDirectories())
				{
					ClearDirectory(di.FullName, false);

					try
					{
						di.Delete();
					}
					catch (Exception)
					{
						try
						{
							Thread.Sleep(0);
							di.Delete();
						}
						catch (Exception) { }
					}
				}
			}
			catch (Exception) { }

			if (selfToo)
			{
				try
				{
					Directory.Delete(directoryPath, true);	// just deletes the (now empty) directory
				}
				catch (Exception) { }
			}
		}

		/// <summary>
		/// Safe way to count files in a directory
		/// </summary>
		/// <param name="directoryPath">A directory path</param>
		/// <returns>File count</returns>
		public static int CountFiles(string directoryPath)
		{
			try
			{
				return Directory.GetFiles(directoryPath).Length;
			}
			catch (Exception) { }

			return 0;
		}

		/// <summary>
		/// Safe way to empty a file
		/// </summary>
		/// <param name="path">File path</param>
		public static void ClearFile(string path)
		{
			try
			{
				if (path.HasValue())
					File.WriteAllText(path, "");
			}
			catch (Exception) { }
		}
	}
}
