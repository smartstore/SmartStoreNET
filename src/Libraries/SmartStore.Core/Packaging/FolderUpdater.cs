using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SmartStore.Core.Logging;

namespace SmartStore.Core.Packaging
{

	public interface IFolderUpdater
	{
		void Backup(DirectoryInfo existingFolder, DirectoryInfo backupfolder);
		void Restore(DirectoryInfo backupfolder, DirectoryInfo existingFolder);
	}

	public class FolderUpdater : IFolderUpdater
	{
		public class FolderContent
		{
			public DirectoryInfo Folder { get; set; }
			public IEnumerable<string> Files { get; set; }
		}

		private readonly ILogger _logger;

		public FolderUpdater(ILogger logger)
		{
			_logger = logger;
		}

		public void Backup(DirectoryInfo existingFolder, DirectoryInfo backupfolder)
		{
			CopyFolder(GetFolderContent(existingFolder), backupfolder);
		}

		public void Restore(DirectoryInfo backupfolder, DirectoryInfo existingFolder)
		{
			CopyFolder(GetFolderContent(backupfolder), existingFolder);
		}

		private void CopyFolder(FolderContent source, DirectoryInfo dest)
		{
			foreach (var file in source.Files)
			{
				CopyFile(source.Folder, file, dest);
			}
		}

		private void CopyFile(DirectoryInfo sourceFolder, string fileName, DirectoryInfo destinationFolder)
		{
			var sourceFile = new FileInfo(Path.Combine(sourceFolder.FullName, fileName));
			var destFile = new FileInfo(Path.Combine(destinationFolder.FullName, fileName));

			// If destination file exist, overwrite only if changed
			if (destFile.Exists)
			{
				if (sourceFile.Length == destFile.Length)
				{
					var source = File.ReadAllBytes(sourceFile.FullName);
					var dest = File.ReadAllBytes(destFile.FullName);
					if (source.SequenceEqual(dest))
					{
						return;
					}
				}
			}

			// Create destination directory
			if (!destFile.Directory.Exists)
			{
				destFile.Directory.Create();
			}

			File.Copy(sourceFile.FullName, destFile.FullName, true);
		}

		private FolderContent GetFolderContent(DirectoryInfo folder)
		{
			var files = new List<string>();
			GetFolderContent(folder, "", files);
			return new FolderContent { Folder = folder, Files = files };
		}

		private void GetFolderContent(DirectoryInfo folder, string prefix, List<string> files)
		{
			if (!folder.Exists)
				return;

			foreach (var file in folder.GetFiles())
			{
				files.Add(Path.Combine(prefix, file.Name));
			}

			foreach (var child in folder.GetDirectories())
			{
				GetFolderContent(child, Path.Combine(prefix, child.Name), files);
			}
		}

	}

}
