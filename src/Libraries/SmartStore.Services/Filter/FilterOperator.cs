using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmartStore.Services.Filter
{
	public sealed class FilterOperator
	{
		private const string _equal = "=";
		private const string _unequal = "!=";
		private const string _greater = ">";
		private const string _greaterEqual = ">=";
		private const string _less = "<";
		private const string _lessEqual = "<=";
		private const string _contains = "contain";
		private const string _startWith = "start";
		private const string _endsWith = "end";
		private const string _rangeGreaterEqualLessEqual = ">=<=";
		private const string _rangeGreaterEqualLess = ">=<";

		private readonly string _name;

		private FilterOperator(string name) {
			this._name = name;
		}

		public static readonly FilterOperator Equal = new FilterOperator(_equal);
		public static readonly FilterOperator Unequal = new FilterOperator(_unequal);
		public static readonly FilterOperator Greater = new FilterOperator(_greater);
		public static readonly FilterOperator GreaterEqual = new FilterOperator(_greaterEqual);
		public static readonly FilterOperator Less = new FilterOperator(_less);
		public static readonly FilterOperator LessEqual = new FilterOperator(_lessEqual);
		public static readonly FilterOperator Contains = new FilterOperator(_contains);
		public static readonly FilterOperator StartsWith = new FilterOperator(_startWith);
		public static readonly FilterOperator EndsWith = new FilterOperator(_endsWith);
		public static readonly FilterOperator RangeGreaterEqualLessEqual = new FilterOperator(_rangeGreaterEqualLessEqual);
		public static readonly FilterOperator RangeGreaterEqualLess = new FilterOperator(_rangeGreaterEqualLess);

		public override string ToString() {
			return _name ?? "=";
		}
		public static FilterOperator Parse(string op) {
			switch (op) {
				case _equal:
					return FilterOperator.Equal;
				case _unequal:
					return FilterOperator.Unequal;
				case _greater:
					return FilterOperator.Greater;
				case _greaterEqual:
					return FilterOperator.GreaterEqual;
				case _less:
					return FilterOperator.Less;
				case _lessEqual:
					return FilterOperator.LessEqual;
				case _contains:
					return FilterOperator.Contains;
				case _startWith:
					return FilterOperator.StartsWith;
				case _endsWith:
					return FilterOperator.EndsWith;
				case _rangeGreaterEqualLessEqual:
					return FilterOperator.RangeGreaterEqualLessEqual;
				case _rangeGreaterEqualLess:
					return FilterOperator.RangeGreaterEqualLess;
				default:
					return null;
			}
		}
	}	// class


	public class OperatorConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) {
			return objectType == typeof(FilterOperator);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			writer.WriteValue(value.ToString());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if (reader != null && reader.TokenType == JsonToken.String) {
				return FilterOperator.Parse(reader.Value as string);
			}
			return null;
		}
	}	// class
}
