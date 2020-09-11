using System;
using System.Collections.Generic;
using System.Threading;

namespace SmartStore.Core.Async
{
    /// <summary>
    /// Stores status information about long-running processes.
    /// </summary>
    public interface IAsyncState
    {
        /// <summary>
        /// Checks whether a status object exists.
        /// </summary>
        /// <typeparam name="T">The type of status to check for.</typeparam>
        /// <param name="name">The optional identifier.</param>
        /// <returns><c>true</c> when the status object exists.</returns>
        bool Exists<T>(string name = null);

        /// <summary>
        /// Gets the status object.
        /// </summary>
        /// <typeparam name="T">The type of status to retrieve.</typeparam>
        /// <param name="name">The optional identifier.</param>
        /// <returns>The status object instance or <c>null</c> if it didn't exist.</returns>
		T Get<T>(string name = null);

        /// <summary>
        /// Enumerates all currently available status objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Status type to enumerate.</typeparam>
		IEnumerable<T> GetAll<T>();

        /// <summary>
        /// Sets status information about any long-running process. The key is <typeparamref name="T"/> + <paramref name="name"/>.
        /// When "Redis" is active the item will be saved in the Redis store so that all nodes in a web farm have access to the object.
        /// If an object with the same key already exists it will be removed.
        /// </summary>
        /// <typeparam name="T">The type of status object.</typeparam>
        /// <param name="state">The status object instance. Can be anything but should be serializable.</param>
        /// <param name="name">The optional identifier. Without identifier, any item of type <typeparamref name="T"/> will be overwritten.</param>
        /// <param name="neverExpires">The default sliding expiration time is 15 minutes. Pass <c>true</c> to prevent automatic expiration but be sure to remove the item.</param>
        void Set<T>(T state, string name = null, bool neverExpires = false);

        /// <summary>
        /// Updates an existing status object. Call this if your unit of work made any progress.
        /// </summary>
        /// <typeparam name="T">The type of status object.</typeparam>
        /// <param name="update">The update action delegate</param>
        /// <param name="name">The optional identifier.</param>
		void Update<T>(Action<T> update, string name = null);

        /// <summary>
        /// Removes a status object. Any cancellation token source with the same key will also be removed automatically.
        /// </summary>
        /// <typeparam name="T">The type of status object to remove.</typeparam>
        /// <param name="name">The optional identifier of the object to remove.</param>
        /// <returns><c>true</c> when the object existed and has been removed, <c>false</c> otherwise.</returns>
		bool Remove<T>(string name = null);

        /// <summary>
        /// Sets a cancellation token that can cancel long-running processes.
        /// The token does not expire automatically, so be sure to remove it once finished.
        /// </summary>
        /// <typeparam name="T">The type of the corresponding status object. However, it is not necessary to save a status object.</typeparam>
        /// <param name="cancelTokenSource">The token.</param>
        /// <param name="name">The optional identifier.</param>
        void SetCancelTokenSource<T>(CancellationTokenSource cancelTokenSource, string name = null);

        CancellationTokenSource GetCancelTokenSource<T>(string name = null);

        bool RemoveCancelTokenSource<T>(string name = null);

        bool Cancel<T>(string name = null);

    }
}
