using System;

namespace SmartStore.Core.Search
{
	public class IndexField
	{
		public IndexField(string name, bool value)
			: this(name, value, IndexTypeCode.Boolean)
		{
		}

		public IndexField(string name, int value)
			: this(name, value, IndexTypeCode.Int32)
		{
		}

		public IndexField(string name, double value)
			: this(name, value, IndexTypeCode.Double)
		{
		}

		public IndexField(string name, DateTime value)
			: this(name, value, IndexTypeCode.DateTime)
		{
		}

		public IndexField(string name, string value)
			: this(name, value, IndexTypeCode.String)
		{
		}

		private IndexField(string name, object value, IndexTypeCode typeCode)
		{
			Guard.NotEmpty(name, nameof(name));

			Name = name;
			Value = value;
			TypeCode = typeCode;
		}

		public IndexField Store(bool store = true)
		{
			Stored = store;
			return this;
		}

		public IndexField Analyze(bool analyze = true)
		{
			Analyzed = analyze;
			return this;
		}

		public IndexField RemoveTags(bool removeTags = true)
		{
			ShouldRemoveTags = removeTags;
			return this;
		}

		public IndexField Boost(float factor)
		{
			Boosted = factor;
			return this;
		}

		public string Name
		{
			get;
			private set;
		}

		public object Value
		{
			get;
			private set;
		}

		public IndexTypeCode TypeCode
		{
			get;
			private set;
		}

		public bool Stored
		{
			get;
			private set;
		}

		public bool Analyzed
		{
			get;
			private set;
		}

		public bool ShouldRemoveTags
		{
			get;
			private set;
		}

		public float Boosted
		{
			get;
			private set;
		}

		public override string ToString()
		{
			return Name + ": " + Value.ToString().NaIfEmpty();
		}
	}

	public enum IndexTypeCode
	{
		Empty = 0,
		Boolean = 3,
		Int32 = 9,
		Double = 14,
		DateTime = 16,
		String = 18
	}
}
