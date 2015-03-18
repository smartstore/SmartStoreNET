﻿using System;
using System.Configuration;
using System.Web.Hosting;
using System.IO;
using SmartStore.Utilities;

namespace SmartStore
{
	public static class AppPath
	{
		/// <summary>Returns physical path to temp directory</summary>
		/// <param name="subDirectory">Name of a sub directory to be created and returned (optional)</param>
		public static string TempDir(string subDirectory = null)
		{
			string path = CommonHelper.GetAppSetting<string>("sm:TempDirectory", "~/App_Data/_temp");
			path = HostingEnvironment.MapPath(path);

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

		/// <summary>Safe way to cleanup the temp directory. Should be called via scheduled task.</summary>
		public static void TempCleanup()
		{
			try
			{
				string dir = AppPath.TempDir();

				if (!Directory.Exists(dir))
					return;

				FileInfo fi;
				DateTime dtOld = DateTime.Now.Subtract(new TimeSpan(0, 5, 0, 0));
				var files = Directory.EnumerateFiles(dir);

				foreach (string file in files)
				{
					fi = new FileInfo(file);

					if (fi != null && fi.LastWriteTime < dtOld)
						AppPath.Delete(file);
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
		}

		/// <summary>Safe way to delete a file.</summary>
		public static bool Delete(string path)
		{
			if (path.IsEmpty())
				return true;

			bool result = true;
			try
			{
				if (Directory.Exists(path))
				{
					throw new MemberAccessException("Deleting folders cause of security reasons not possible: {0}".FormatWith(path));
				}

				System.IO.File.Delete(path);	// no exception, if file doesn't exists
			}
			catch (Exception exc)
			{
				result = false;
				exc.Dump();
			}
			return result;
		}

		/// <summary>Safe way to copy a file.</summary>
		public static bool Copy(string sourcePath, string destinationPath, bool overwrite = true, bool deleteSource = false)
		{
			bool result = true;
			try
			{
				System.IO.File.Copy(sourcePath, destinationPath, overwrite);

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
	}
}
