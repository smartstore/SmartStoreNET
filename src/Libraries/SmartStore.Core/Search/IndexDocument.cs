using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;

namespace SmartStore.Core.Search
{
	public class IndexDocument : IIndexDocument
	{
		private readonly Multimap<string, IndexField> _fields;

		public IndexDocument(int id)
		{
			_fields = new Multimap<string, IndexField>(StringComparer.OrdinalIgnoreCase);
			_fields.Add("id", new IndexField("id", id).Store());
		}

		public int Id
		{
			get
			{
				return (int)_fields["id"].FirstOrDefault().Value;
			}
		}

		public int Count => _fields.TotalValueCount;

		public void Add(IndexField field)
		{
			if (field.Name.IsCaseInsensitiveEqual("id") && _fields.ContainsKey("id"))
			{
				// special treatment for id field: allow only one!
				_fields.RemoveAll("id");
			}
			_fields.Add(field.Name, field);
		}

		public int Remove(string name)
		{
			if (_fields.ContainsKey(name))
			{
				var num = _fields[name].Count;
				_fields.RemoveAll(name);
				return num;
			}

			return 0;
		}

		public bool Contains(string name)
		{
			return _fields.ContainsKey(name);
		}

		public IEnumerable<IndexField> this[string name]
		{
			get
			{
				if (_fields.ContainsKey(name))
				{
					return _fields[name];
				}

				return Enumerable.Empty<IndexField>();
			}
		}


		public IEnumerator<IndexField> GetEnumerator()
		{
			return _fields.SelectMany(x => x.Value).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
