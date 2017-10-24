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

			var clonedNode = new TreeNode<TValue>(value);

			// Assign or clone Metadata
			if (_metadata != null && _metadata.Count > 0)
			{
				foreach (var kvp in _metadata)
				{
					var metadataValue = kvp.Value is ICloneable 
						? ((ICloneable)kvp.Value).Clone() 
						: kvp.Value;
					clonedNode.SetMetadata(kvp.Key, metadataValue);
				}
			}

			if (_id != null)
			{
				clonedNode._id = _id;
			}

			return clonedNode;
		}

		public TreeNode<TValue> Append(TValue value, object id = null)
		{
			var node = new TreeNode<TValue>(value);
			node._id = id;
			this.Append(node);
			return node;
		}

		public void AppendRange(IEnumerable<TValue> values)
		{
			values.Each(x => Append(x));
		}

		public void AppendRange(IEnumerable<TValue> values, Func<TValue, object> idSelector)
		{
			Guard.NotNull(idSelector, nameof(idSelector));

			values.Each(x => Append(x, idSelector(x)));
		}

		public TreeNode<TValue> Prepend(TValue value, object id = null)
		{
			var node = new TreeNode<TValue>(value);
			node._id = id;
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
			string id = null;
			Dictionary<string, object> metadata = null;

			reader.Read();
			while (reader.TokenType == JsonToken.PropertyName)
			{
				string a = reader.Value.ToString();
				if (string.Equals(a, "Value", StringComparison.OrdinalIgnoreCase))
				{
					reader.Read();
					objValue = serializer.Deserialize(reader, valueType);
				}
				else if (string.Equals(a, "Metadata", StringComparison.OrdinalIgnoreCase))
				{
					reader.Read();
					metadata = serializer.Deserialize<Dictionary<string, object>>(reader);
				}
				else if (string.Equals(a, "Children", StringComparison.OrdinalIgnoreCase))
				{
					reader.Read();
					objChildren = serializer.Deserialize(reader, sequenceType);
				}
				if (string.Equals(a, "Id", StringComparison.OrdinalIgnoreCase))
				{
					reader.Read();
					id = serializer.Deserialize<string>(reader);
				}
				else
				{
					reader.Skip();
				}

				reader.Read();
			}

			var treeNode = Activator.CreateInstance(objectType, new object[] { objValue, objChildren });

			// Set Metadata
			if (metadata != null && metadata.Count > 0)
			{
				var metadataProp = FastProperty.GetProperty(objectType, "Metadata", PropertyCachingStrategy.Cached);
				metadataProp.SetValue(treeNode, metadata);

				if (id.HasValue())
				{
					var idProp = FastProperty.GetProperty(objectType, "Id", PropertyCachingStrategy.Cached);
					idProp.SetValue(treeNode, id);
				}
			}
			
			return treeNode;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var idProp = FastProperty.GetProperty(value.GetType(), "Id", PropertyCachingStrategy.Cached);
			var valueProp = FastProperty.GetProperty(value.GetType(), "Value", PropertyCachingStrategy.Cached);
			var childrenProp = FastProperty.GetProperty(value.GetType(), "Children", PropertyCachingStrategy.Cached);
			var metadataProp = FastProperty.GetProperty(value.GetType(), "Metadata", PropertyCachingStrategy.Cached);

			writer.WriteStartObject();
			{
				writer.WritePropertyName("Id");
				serializer.Serialize(writer, idProp.GetValue(value));

				writer.WritePropertyName("Value");
				serializer.Serialize(writer, valueProp.GetValue(value));

				writer.WritePropertyName("Metadata");
				serializer.Serialize(writer, metadataProp.GetValue(value));

				writer.WritePropertyName("Children");
				serializer.Serialize(writer, childrenProp.GetValue(value));
			}
			writer.WriteEndObject();
		}
	}
}
