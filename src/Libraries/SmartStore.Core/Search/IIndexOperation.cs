using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Search
{
	public enum IndexOperationType
	{
		Index,
		Delete
	}

	/// <summary>
	/// Represents an indexing operation
	/// </summary>
	public interface IIndexOperation
	{
		/// <summary>
		/// The type of the operation
		/// </summary>
		IndexOperationType OperationType { get; }

		/// <summary>
		/// The document being inserted to or deleted from the index storage
		/// </summary>
		IIndexDocument Document { get; }

		/// <summary>
		/// The database entity from which <see cref="Document"/> was created
		/// </summary>
		BaseEntity Entity { get; }
	}
}
