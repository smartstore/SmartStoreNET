using System;
using System.Threading;
using SmartStore.Utilities.Threading;

namespace SmartStore.Core.IO
{
	public class LockFile : ILockFile
	{
		private readonly string _path;
		private readonly string _content;
		private readonly IVirtualPathProvider _vpp;
		private readonly ReaderWriterLockSlim _rwLock;
		private bool _released;

		public LockFile(IVirtualPathProvider vpp, string path, string content, ReaderWriterLockSlim rwLock)
		{
			_vpp = vpp;
			_content = content;
			_rwLock = rwLock;
			_path = path;

			// create the physical lock file
			_vpp.CreateFile(_path, content);
		}

		public void Dispose()
		{
			Release();
		}

		public void Release()
		{
			using (_rwLock.GetWriteLock())
			{
				if (_released || !_vpp.FileExists(_path))
				{
					// nothing to do, might happen if re-granted or already released
					return;
				}

				_released = true;

				// check it has not been granted in the meantime
				var current = _vpp.ReadFile(_path);
				if (current == _content)
				{
					_vpp.DeleteFile(_path);
				}
			}
		}
	}
}
