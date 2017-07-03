using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartStore.Core.Search.Facets
{
	public enum FacetSorting
	{
		HitsDesc,
		ValueAsc,
		LabelAsc,
		DisplayOrder
	}

	/// <summary>
	/// A filter and its selection to be applied, e.g. Color=Red.
	/// </summary>
	[Serializable]
	public class FacetDescriptor
	{
		private readonly List<FacetValue> _values;

		public FacetDescriptor(string key)
		{
			Guard.NotEmpty(key, nameof(key));

			_values = new List<FacetValue>();
			Key = key;
		}

		/// <summary>
		/// Gets the string resource key for a facet group kind
		/// </summary>
		/// <param name="kind">Facet group kind</param>
		/// <returns>Resource key</returns>
		public static string GetLabelResourceKey(FacetGroupKind kind)
		{
			switch (kind)
			{
				case FacetGroupKind.Category:
					return "Search.Facet.Category";
				case FacetGroupKind.Brand:
					return "Search.Facet.Manufacturer";
				case FacetGroupKind.Price:
					return "Search.Facet.Price";
				case FacetGroupKind.Rating:
					return "Search.Facet.Rating";
				case FacetGroupKind.DeliveryTime:
					return "Search.Facet.DeliveryTime";
				case FacetGroupKind.Availability:
					return "Search.Facet.Availability";
				case FacetGroupKind.NewArrivals:
					return "Search.Facet.NewArrivals";
				default:
					return null;
			}
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
		/// Gets or sets the label.
		/// </summary>
		public string Label
		{
			get;
			set;
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
		//public ValueOperator Operator
		//{
		//	get;
		//	set;
		//}

		/// <summary>
		/// Gets or sets whether selection of multiple values is allowed.
		/// </summary>
		public bool IsMultiSelect
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
		public FacetSorting OrderBy
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the display order.
		/// </summary>
		public int DisplayOrder
		{
			get;
			set;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.Append("FieldName: ").Append(Key).Append(" ");
			sb.Append("Values: " + string.Join(",", _values.Select(x => x.Value.ToString()))).Append(" ");

			return sb.ToString();
		}
	}
}
