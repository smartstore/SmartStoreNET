﻿using System;
using System.Linq;
using Newtonsoft.Json;

namespace SmartStore.Services.Filter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class FilterCriteria : IComparable
	{
		[JsonProperty]
		public string Name { get; set; }

		[JsonProperty]
		public string Value { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Entity { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool Or { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Type { get; set; }

		[JsonConverter(typeof(OperatorConverter))]
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FilterOperator Operator { get; set; }

		/// <summary>left, right or none parenthesis</summary>
		//[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool? Open { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? ID { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? PId { get; set; }

		// Metadata
		public int MatchCount { get; set; }
		public bool IsInactive { get; set; }
		public string NameLocalized { get; set; }
		public string ValueLocalized { get; set; }

		public string SqlName
		{
			get
			{
				if (Entity.IsCaseInsensitiveEqual("Manufacturer") && !Name.Contains('.'))
					return "{0}.{1}".FormatInvariant(Entity, Name);

				return Name;
			}
		}
		
		public bool IsRange
		{
			get
			{
				return (Value.HasValue() && Value.Contains('~') && (Operator == FilterOperator.RangeGreaterEqualLessEqual || Operator == FilterOperator.RangeGreaterEqualLess));
			}
		}

		int IComparable.CompareTo(object obj)
		{
			var filter = (FilterCriteria)obj;

			var compare = string.Compare(this.Entity, filter.Entity, true);

			if (compare == 0)
			{
				compare = string.Compare(this.Name, filter.Name, true);

				if (compare == 0)
					compare = string.Compare(this.Value, filter.Value, true);
			}

			return compare;
		}

		public override string ToString()
		{
			try
			{
				return JsonConvert.SerializeObject(this);
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
			return "";
		}
	}
}
