using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartStore.Core.Search.Facets
{
	/// <summary>
	/// A filter and its selection to be applied, e.g. Color=Red.
	/// </summary>
	[Serializable]
	public class FacetDescriptor
	{
		public enum ValueOperator
		{
			And,
			Or
		}

		public enum Sorting
		{
			HitsDesc,
			ValueAsc
		}

		private readonly List<FacetValue> _values;

		public FacetDescriptor(string key)
		{
			Guard.NotEmpty(key, nameof(key));

			_values = new List<FacetValue>();
			Key = key;
		}

		/// <summary>
		/// Gets the key / field name.
		/// </summary>
		public string Key
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the values for this facet.
		/// </summary>
		public ICollection<FacetValue> Values
		{
			get
			{
				return _values;
			}
		}

		/// <summary>
		/// Adds a facet value.
		/// </summary>
		/// <param name="value">Facet value</param>
		public FacetDescriptor AddValue(params FacetValue[] values)
		{
			_values.AddRange(values);
			return this;
		}

		/// <summary>
		/// Gets or sets the boolean value operator.
		/// </summary>
		public ValueOperator Operator
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the minimum number of hits a choice would need to have to be returned.
		/// </summary>
		public int MinHitCount
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the maximum number of choices to return. Default = 0 which means all.
		/// </summary>
		public int MaxChoicesCount
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the result choices sort order.
		/// </summary>
		public Sorting OrderBy
		{
			get;
			set;
		}

		public bool IsMultiSelect
		{
			get;
			set;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.Append("FieldName: ").Append(Key).Append(" ");
			sb.Append("Values: " + string.Join(",", _values.Select(x => x.Value.ToString()))).Append(" ");
			sb.Append("op: " + Operator.ToString()).Append(" ");

			return sb.ToString();
		}
	}


	public static class FacetDescriptorExtensions
	{
		public static IOrderedEnumerable<Facet> OrderBy(this IEnumerable<Facet> source, FacetDescriptor.Sorting sorting)
		{
			Guard.NotNull(source, nameof(source));

			if (sorting == FacetDescriptor.Sorting.HitsDesc)
			{
				return source.OrderByDescending(x => x.HitCount);
			}

			return source.OrderBy(x => x.Label);
		}
	}
}
