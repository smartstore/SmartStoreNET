using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace SmartStore.Services.DataExchange.Import
{
    public class ColumnMap
    {
        // maps source column to property
        private readonly Dictionary<string, ColumnMappingItem> _map = new Dictionary<string, ColumnMappingItem>(StringComparer.OrdinalIgnoreCase);

        private static bool IsIndexed(string name)
        {
            return (name.EmptyNull().EndsWith("]") && name.EmptyNull().Contains("["));
        }

        private static string CreateSourceName(string name, string index)
        {
            if (index.HasValue())
            {
                name += String.Concat("[", index, "]");
            }

            return name;
        }

        public IReadOnlyDictionary<string, ColumnMappingItem> Mappings => _map;

        public static bool ParseSourceName(string sourceName, out string nameWithoutIndex, out string index)
        {
            nameWithoutIndex = sourceName;
            index = null;

            var result = true;

            if (sourceName.HasValue() && IsIndexed(sourceName))
            {
                var x1 = sourceName.IndexOf('[');
                var x2 = sourceName.IndexOf(']', x1);

                if (x1 != -1 && x2 != -1 && x2 > x1)
                {
                    nameWithoutIndex = sourceName.Substring(0, x1);
                    index = sourceName.Substring(x1 + 1, x2 - x1 - 1);
                }
                else
                {
                    result = false;
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddMapping(string sourceName, string mappedName, string defaultValue = null)
        {
            AddMapping(sourceName, null, mappedName, defaultValue);
        }

        public void AddMapping(string sourceName, string index, string mappedName, string defaultValue = null)
        {
            Guard.NotEmpty(sourceName, nameof(sourceName));
            Guard.NotEmpty(mappedName, nameof(mappedName));

            var key = CreateSourceName(sourceName, index);

            _map[key] = new ColumnMappingItem
            {
                SoureName = key,
                MappedName = mappedName,
                Default = defaultValue
            };
        }

        /// <summary>
        /// Gets a mapped column value
        /// </summary>
        /// <param name="sourceName">The name of the column to get a mapped value for.</param>
        /// <param name="index">The column index, e.g. a language code (de, en etc.)</param>
        /// <returns>The mapped column value OR - if the name is unmapped - a value with the passed <paramref name="sourceName"/>[<paramref name="index"/>]</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ColumnMappingItem GetMapping(string sourceName, string index)
        {
            return GetMapping(CreateSourceName(sourceName, index));
        }

        /// <summary>
        /// Gets a mapped column value
        /// </summary>
        /// <param name="sourceName">The name of the column to get a mapped value for.</param>
        /// <returns>The mapped column value OR - if the name is unmapped - the value of the passed <paramref name="sourceName"/></returns>
        public ColumnMappingItem GetMapping(string sourceName)
        {
            ColumnMappingItem result;

            if (_map.TryGetValue(sourceName, out result))
            {
                return result;
            }

            return new ColumnMappingItem { SoureName = sourceName, MappedName = sourceName };
        }
    }


    [JsonObject(MemberSerialization.OptIn)]
    public class ColumnMappingItem
    {
        private bool? _ignored;

        /// <summary>
        /// The source name
        /// </summary>
        [JsonIgnore]
        public string SoureName { get; set; }

        /// <summary>
        /// The mapped name
        /// </summary>
        [JsonProperty]
        public string MappedName { get; set; }

        /// <summary>
        /// An optional default value
        /// </summary>
        [JsonProperty]
        public string Default { get; set; }

        /// <summary>
        /// Indicates whether to explicitly ignore this property
        /// </summary>
        public bool IgnoreProperty
        {
            get
            {
                if (_ignored == null)
                {
                    _ignored = Default != null && Default == "[IGNOREPROPERTY]";
                }

                return _ignored.Value;
            }
        }
    }
}
