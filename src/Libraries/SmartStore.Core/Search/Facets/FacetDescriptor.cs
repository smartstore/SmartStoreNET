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

		private readonly List<FacetValue> _selectedValues;

		public FacetDescriptor(string key)
		{
			Guard.NotEmpty(key, nameof(key));

			_selectedValues = new List<FacetValue>();
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
		/// Gets the initially selected values for this facet.
		/// </summary>
		public ICollection<FacetValue> SelectedValues
		{
			get
			{
				return _selectedValues;
			}
		}

		/// <summary>
		/// Adds a selection value.
		/// </summary>
		/// <param name="value">Value to select</param>
		public FacetDescriptor AddSelectedValue(params FacetValue[] values)
		{
			_selectedValues.AddRange(values);
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

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.Append("FieldName: ").Append(Key).Append(" ");
			sb.Append("Values: " + string.Join(",", _selectedValues.Select(x => x.Value.ToString()))).Append(" ");
			sb.Append("op: " + Operator.ToString()).Append(" ");

			return sb.ToString();
		}
	}
}
