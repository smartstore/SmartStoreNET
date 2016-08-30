using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Search
{
	public interface IIndexDocument : IEnumerable<IndexField>
	{
		int Id { get; }
		int Count { get; }
		void Add(IndexField field);
		int Remove(string name);
		bool Contains(string name);
		IEnumerable<IndexField> this[string name] { get; }
	}

	public static class IIndexDocumentExtensions
	{
		public static IIndexDocument SetId(this IIndexDocument doc, int id)
		{
			doc.Add(new IndexField("id", id).Store());
			return doc;
		}

		public static int? GetId(this IIndexDocument doc)
		{
			return (int?)doc["id"].FirstOrDefault()?.Value;
		}
	}
}
