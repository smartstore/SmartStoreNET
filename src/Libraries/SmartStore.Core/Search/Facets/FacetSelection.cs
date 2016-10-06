using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartStore.Core.Search.Facets
{
	/// <summary>
	/// A selection or filter to be applied, e.g. Color=Red.
	/// </summary>
	[Serializable]
	public class FacetSelection
	{
		public enum ValueOperator
		{
			And,
			Or
		}

		private readonly List<string> _values;

		public FacetSelection(string fieldName)
		{
			Guard.NotEmpty(fieldName, nameof(fieldName));

			_values = new List<string>();
			FieldName = fieldName;
		}

		/// <summary>
		/// Gets or sets the field name.
		/// </summary>
		public string FieldName
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the selected values for this facet.
		/// </summary>
		public ICollection<string> Values
		{
			get
			{
				return _values;
			}
		}

		/// <summary>
		/// Adds a selection value.
		/// </summary>
		/// <param name="value">Value to select</param>
		public FacetSelection AddValue(params string[] values)
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
		/// Gets or sets the maximum number of choices to return. Default = 0 which means all.
		/// </summary>
		public int MaxChoicesCount
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

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.Append("FieldName: ").Append(FieldName).Append(" ");
			sb.Append("Values: " + string.Join(",", _values.ToArray())).Append(" ");
			sb.Append("op: " + Operator.ToString()).Append(" ");

			return sb.ToString();
		}
	}
}
