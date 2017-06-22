using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SmartStore.Core.Data;

namespace SmartStore.Utilities
{
	public static class FileSystemHelper
	{
		/// <summary>
		/// Returns physical path to application temp directory
		/// </summary>
		/// <param name="subDirectory">Name of a sub directory to be created and returned (optional)</param>
		public static string TempDir(string subDirectory = null)
		{
			return TempDirInternal(CommonHelper.GetAppSetting("sm:TempDirectory", "~/App_Data/_temp"), subDirectory);
		}

		/// <summary>
		/// Returns physical path to current tenant temp directory
		/// </summary>
		/// <param name="subDirectory">Name of a sub directory to be created and returned (optional)</param>
		public static string TempDirTenant(string subDirectory = null)
		{
			return TempDirInternal(DataSettings.Current.TenantPath + "/_temp", subDirectory);
		}

		private static string TempDirInternal(string virtualPath, string subDirectory = null)
		{
			var path = CommonHelper.MapPath(virtualPath);

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
				
			if (subDirectory.HasValue())
			{
				path = Path.Combine(path, subDirectory);

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}	
			}

			return path;
		}

		/// <summary>
		/// Ensures that path is a valid root path
		/// </summary>
		/// <param name="path">Relative path</param>
		/// <returns>Valid root path</returns>
		public static string ValidateRootPath(string path)
		{
			if (path.HasValue())
			{
				path = path.Replace('\\', '/');

				if (!path.StartsWith("~/"))
				{
					if (path.StartsWith("~"))
						path = path.Substring(1);

					path = (path.StartsWith("/") ? "~" : "~/") + path;
				}
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
				var dirs = new string[] { TempDir(), TempDirTenant() };

				foreach (var dir in dirs)
				{
					if (Directory.Exists(dir))
					{
						var oldestDate = DateTime.Now.Subtract(new TimeSpan(0, 5, 0, 0));
						var files = Directory.EnumerateFiles(dir);

						foreach (string file in files)
						{
							var fi = new FileInfo(file);

							if (fi.LastWriteTime < oldestDate)
								Delete(file);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ex.Dump();
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
		public static bool CopyDirectory(DirectoryInfo source, DirectoryInfo target, bool overwrite = true)
		{
			if (target.FullName.Contains(source.FullName))
			{
				// Cannot copy a folder into itself.
				return false;
			}

			var result = true;

			foreach (FileInfo fi in source.GetFiles())
			{
				try
				{
					fi.CopyTo(Path.Combine(target.ToString(), fi.Name), overwrite);
				}
				catch (Exception exc)
				{
					result = false;
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
					result = false;
					exc.Dump();
				}
			}

			return result;
		}

		/// <summary>
		/// Safe way to delete all directory content
		/// </summary>
		/// <param name="directoryPath">A directory path</param>
		/// <param name="selfToo">Delete directoryPath too</param>
		/// <param name="exceptFileNames">Name of files not to be deleted</param>
		public static void ClearDirectory(string directoryPath, bool selfToo, List<string> exceptFileNames = null)
		{
			if (directoryPath.IsEmpty())
				return;

			try
			{
				var dir = new DirectoryInfo(directoryPath);

				foreach (var fi in dir.GetFiles())
				{
					if (exceptFileNames != null && exceptFileNames.Any(x => x.IsCaseInsensitiveEqual(fi.Name)))
						continue;

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
		/// Creates a non existing directory name
		/// </summary>
		/// <param name="directoryPath">Path of a directory</param>
		/// <param name="defaultName">Default name for directory. <c>null</c> to use a guid.</param>
		/// <returns>Non existing directory name</returns>
		public static string CreateNonExistingDirectoryName(string directoryPath, string defaultName)
		{
			if (defaultName.IsEmpty())
				defaultName = Guid.NewGuid().ToString();

			if (directoryPath.IsEmpty() || !Directory.Exists(directoryPath))
				return defaultName;

			var newName = defaultName;

			for (int i = 1; i < 999999 && Directory.Exists(Path.Combine(directoryPath, newName)); ++i)
			{
				newName = defaultName + i.ToString();
			}

			return newName;
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

		/// <summary>
		/// Checks whether the given path is a fully qualified absolute path (either UNC or rooted with drive letter)
		/// </summary>
		/// <param name="path">Path to check</param>
		/// <returns><c>true</c> if path is fully qualified</returns>
		public static bool IsFullPath(string path)
		{
			return !String.IsNullOrWhiteSpace(path)
				&& path.IndexOfAny(System.IO.Path.GetInvalidPathChars().ToArray()) == -1
				&& Path.IsPathRooted(path)
				&& !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
		}
	}
}
