using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            : this(value)
        {
            if (children != null && children.Any())
            {
                AppendRange(children);
            }
        }

        public TreeNode(TValue value, IEnumerable<TreeNode<TValue>> children)
            : this(value)
        {
            // for serialization
            if (children != null && children.Any())
            {
                AppendRange(children);
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendRange(IEnumerable<TValue> values)
        {
            values.Each(x => Append(x));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TValue> Flatten(bool includeSelf = true)
        {
            return this.Flatten(null, includeSelf);
        }

        public IEnumerable<TValue> Flatten(Func<TValue, bool> predicate, bool includeSelf = true)
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
            if (predicate != null)
            {
                result = result.Where(predicate);
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
                else if (string.Equals(a, "Id", StringComparison.OrdinalIgnoreCase))
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

            var ctorParams = objChildren != null
                ? new object[] { objValue, objChildren }
                : new object[] { objValue };

            var treeNode = Activator.CreateInstance(objectType, ctorParams);

            // Set Metadata
            if (metadata != null && metadata.Count > 0)
            {
                var metadataProp = FastProperty.GetProperty(objectType, "Metadata", PropertyCachingStrategy.Cached);
                metadataProp.SetValue(treeNode, metadata);
            }

            // Set Id
            if (id.HasValue())
            {
                var idProp = FastProperty.GetProperty(objectType, "Id", PropertyCachingStrategy.Cached);
                idProp.SetValue(treeNode, id);
            }

            return treeNode;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            {
                // Id
                if (GetPropValue("Id", value) is object o)
                {
                    writer.WritePropertyName("Id");
                    serializer.Serialize(writer, o);
                }

                // Value
                writer.WritePropertyName("Value");
                serializer.Serialize(writer, GetPropValue("Value", value));

                // Metadata
                if (GetPropValue("Metadata", value) is IDictionary<string, object> dict && dict.Count > 0)
                {
                    writer.WritePropertyName("Metadata");

                    var typeNameHandling = serializer.TypeNameHandling;
                    try
                    {
                        serializer.TypeNameHandling = TypeNameHandling.All;
                        serializer.Serialize(writer, dict);
                    }
                    finally
                    {
                        serializer.TypeNameHandling = typeNameHandling;
                    }
                }

                // Children
                if (GetPropValue("HasChildren", value) is bool b && b == true)
                {
                    writer.WritePropertyName("Children");
                    serializer.Serialize(writer, GetPropValue("Children", value));
                }
            }
            writer.WriteEndObject();
        }

        private object GetPropValue(string name, object instance)
        {
            return FastProperty.GetProperty(instance.GetType(), name, PropertyCachingStrategy.Cached).GetValue(instance);
        }
    }
}
