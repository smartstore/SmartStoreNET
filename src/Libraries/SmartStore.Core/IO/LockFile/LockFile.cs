using System;
using System.Threading;
using System.IO;
using SmartStore.Utilities.Threading;

namespace SmartStore.Core.IO
{
	public class LockFile : ILockFile
	{
		private readonly string _path;
		private readonly string _content;
		private readonly IVirtualFolder _folder;
		private readonly ReaderWriterLockSlim _rwLock;
		private bool _released;

		public LockFile(IVirtualFolder folder, string path, string content, ReaderWriterLockSlim rwLock)
		{
			_folder = folder;
			_content = content;
			_rwLock = rwLock;
			_path = path;

			// create the physical lock file
			_folder.CreateTextFile(_path, content);
		}

		public void Dispose()
		{
			Release();
		}

		public void Release()
		{
			using (_rwLock.GetWriteLock())
			{
				if (_released || !File.Exists(_folder.MapPath(_path)))
				{
					// nothing to do, might happen if re-granted or already released
					// INFO: VirtualPathProvider caches file existence info, so not very reliable here.
					return;
				}

				_released = true;

				// check it has not been granted in the meantime
				var current = _folder.ReadFile(_path);
				if (current == _content)
				{
					_folder.DeleteFile(_path);
				}
			}
		}
	}
}
