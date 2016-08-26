using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Search
{
	public enum IndexingStatus
	{
		Rebuilding,
		Updating,
		Idle,
		Unavailable
	}

	public interface IIndexStore
	{
		/// <summary>
		/// Creates a new index if it doesn't exist already
		/// </summary>
		void CreateIfNotExists();

		/// <summary>
		/// Deletes the index
		/// </summary>
		void Delete();

		/// <summary>
		/// Returns the name of the index
		/// </summary>
		string Scope { get; }

		/// <summary>
		/// Checks whether the index is already existing or not
		/// </summary>
		bool Exists { get; }

		IndexingStatus Status { get; }

		/// <summary>
		/// Gets the date of the last index operation, or <c>null</c>
		/// if the index has never been processed before.
		/// </summary>
		/// <returns>A UTC datetime object</returns>
		DateTime? GetLastIndexedUtc();

		void SetLastIndexedUtc(DateTime date);

		/// <summary>
		/// Checks whether the index is locked.
		/// </summary>
		/// <remarks>
		/// An index is locked when it is currently being written to
		/// </remarks>
		bool IsLocked { get; }

		/// <summary>
		/// Tries to acquire a lock for atomic write operations 
		/// </summary>
		/// <param name="lockObj">A special disposable object which releases the lock implicitly on dispose, or <c>null</c> when the lock could not be acquired.</param>
		/// <returns><c>true</c> when a lock could be acquired successfully, or <c>false</c> when a lock is already is being held by another thread/process.</returns>
		bool TryAcquireLock(out IDisposable lockObj);

		/// <summary>
		/// Gets the total number of indexed documents
		/// </summary>
		int DocumentCount { get; }

		/// <summary>
		/// Returns every field's name available in the index
		/// </summary>
		IEnumerable<string> GetAllFields();

		/// <summary>
		/// Removes all documents from the index
		/// </summary>
		void Clear();

		/// <summary>
		/// Adds a set of new documents to the index
		/// </summary>
		/// <remarks>
		/// This method will delete already existing documents before saving them. Entity id is the match key.
		/// </remarks>
		void SaveDocuments(IEnumerable<IIndexDocument> documents);

		/// <summary>
		/// Removes a set of existing documents from the index
		/// </summary>
		void DeleteDocuments(IEnumerable<int> ids);
	}

	public static class IIndexStoreExtensions
	{
		/// <summary>
		/// Adds a new document to the index
		/// </summary>
		/// <remarks>
		/// This method will delete a document with the same entity id - if it exists - before saving it.
		/// </remarks>
		public static void SaveDocument(this IIndexStore store, IIndexDocument document)
		{
			store.SaveDocuments(new[] { document });
		}

		/// <summary>
		/// Removes an existing document from the index
		/// </summary>
		public static void DeleteDocument(this IIndexStore store, int id)
		{
			store.DeleteDocuments(new[] { id });
		}
	}
}
