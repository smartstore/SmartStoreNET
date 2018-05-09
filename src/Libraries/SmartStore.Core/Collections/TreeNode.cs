using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Newtonsoft.Json;
using SmartStore.ComponentModel;

namespace SmartStore.Collections
{
	[JsonConverter(typeof(TreeNodeConverter))]
	public class TreeNode<TValue> : TreeNodeBase<TreeNode<TValue>>
	{
		public TreeNode(TValue value)
		{
			Guard.NotNull(value, nameof(value));

			Value = value;
		}

		public TreeNode(TValue value, IEnumerable<TValue> children)
		{
			Value = value;
			AppendRange(children);
		}

		public TreeNode(TValue value, IEnumerable<TreeNode<TValue>> children)
		{
			// for serialization
			Value = value;
			AppendRange(children);
		}

		public TValue Value
		{
			get;
			private set;
		}

		protected override TreeNode<TValue> CreateInstance()
		{
			TValue value = this.Value;

			if (value is ICloneable<TValue>)
			{
				value = ((ICloneable<TValue>)value).Clone();
			}

			return new TreeNode<TValue>(value);
		}

		public TreeNode<TValue> Append(TValue value)
		{
			var node = new TreeNode<TValue>(value);
			this.Append(node);
			return node;
		}

		public void AppendRange(IEnumerable<TValue> values)
		{
			values.Each(x => Append(x));
		}

		public TreeNode<TValue> Prepend(TValue value)
		{
			var node = new TreeNode<TValue>(value);
			this.Prepend(node);
			return node;
		}

		public IEnumerable<TValue> Flatten(bool includeSelf = true)
		{
			return this.Flatten(null, includeSelf);
		}

		public IEnumerable<TValue> Flatten(Expression<Func<TValue, bool>> expression, bool includeSelf = true)
		{
			IEnumerable<TValue> list;
			if (includeSelf)
			{
				list = new[] { this.Value };
			}
			else
			{
				list = Enumerable.Empty<TValue>();
			}

			if (!HasChildren)
				return list;

			var result = list.Union(Children.SelectMany(x => x.Flatten()));
			if (expression != null)
			{
				result = result.Where(expression.Compile());
			}

			return result;
		}
	}

	public class TreeNodeConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			var canConvert = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(TreeNode<>);
			return canConvert;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var valueType = objectType.GetGenericArguments()[0];
			var sequenceType = typeof(List<>).MakeGenericType(objectType);

			object objValue = null;
			object objChildren = null;

			reader.Read();
			while (reader.TokenType == JsonToken.PropertyName)
			{
				string a = reader.Value.ToString();
				if (string.Equals(a, "Value", StringComparison.OrdinalIgnoreCase))
				{
					reader.Read();
					objValue = serializer.Deserialize(reader, valueType);
				}
				else if (string.Equals(a, "Children", StringComparison.OrdinalIgnoreCase))
				{
					reader.Read();
					objChildren = serializer.Deserialize(reader, sequenceType);
				}
				else
				{
					reader.Skip();
				}

				reader.Read();
			}

			var treeNode = Activator.CreateInstance(objectType, new object[] { objValue, objChildren });
			
			return treeNode;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var valueProp = FastProperty.GetProperty(value.GetType(), "Value", PropertyCachingStrategy.Cached);
			var childrenProp = FastProperty.GetProperty(value.GetType(), "Children", PropertyCachingStrategy.Cached);

			writer.WriteStartObject();
			{
				writer.WritePropertyName("Value");
				serializer.Serialize(writer, valueProp.GetValue(value));

				writer.WritePropertyName("Children");
				serializer.Serialize(writer, childrenProp.GetValue(value));
			}
			writer.WriteEndObject();
		}
	}
}
