using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Search;

namespace SmartStore.Services.Search
{
	public interface IIndexDataSegmenter
	{
		/// <summary>
		/// Total number of documents
		/// </summary>
		int TotalDocuments { get; }

		/// <summary>
		/// Number of documents per segment
		/// </summary>
		int SegmentSize { get; }

		/// <summary>
		/// Gets current data segment
		/// </summary>
		IEnumerable<IndexOperation> CurrentSegment { get; }

		/// <summary>
		/// Reads the next segment
		/// </summary>
		/// <returns><c>true</c> if there are more segments, <c>false</c> if all segments have been processed.</returns>
		bool ReadNextSegment();
	}

	public enum IndexOperationType
	{
		Index,
		Delete
	}

	public class IndexOperation
	{
		public IndexOperation(IndexOperationType type, IIndexDocument document)
		{
			Guard.NotNull(document, nameof(document));

			OperationType = type;
			Document = document;
		}

		public IndexOperationType OperationType { get; private set; }
		public IIndexDocument Document { get; private set; }
	}
}
