using System;

namespace SmartStore.Core.IO
{
	/// <summary>
	/// Abstraction for application-wide lock files creation.
	/// </summary>
	/// <remarks>
	/// All virtual paths passed in or returned are relative to "~/App_Data".
	/// </remarks>
	public interface ILockFileManager
	{
		/// <summary>
		/// Attempts to acquire an exclusive lock file.
		/// </summary>
		/// <param name="path">The filename of the lock file to create relative to ~/App_Data.</param>
		/// <param name="lockFile">A reference to the lock file object if the lock is granted.</param>
		/// <returns><c>true</c> if the lock is granted; otherwise, <c>false</c>.</returns>
		bool TryAcquireLock(string path, out ILockFile lockFile);

		/// <summary>
		/// Wether a lock file is already existing.
		/// </summary>
		/// <param name="path">The filename of the lock file to test.</param>
		/// <returns><c>true</c> if the lock file exists; otherwise, <c>false</c>.</returns>
		bool IsLocked(string path);
	}
}
