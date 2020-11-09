using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Data.Setup
{
    internal class SettingEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool KeyIsGroup { get; set; }
    }

    public class SettingsBuilder : IHideObjectMembers
    {
        private readonly List<SettingEntry> _entries = new List<SettingEntry>();

        /// <summary>
        /// Deletes one or many setting records from the database
        /// </summary>
        /// <param name="keys">The key(s) of the settings to delete</param>
        public void Delete(params string[] keys)
        {
            keys.Each(x => _entries.Add(new SettingEntry { Key = x.TrimSafe() }));
        }

        /// <summary>
        /// Deletes all settings records prefixed with the specified group name from the database
        /// </summary>
        /// <param name="keys">The group/prefix (actually the settings class name)</param>
        public void DeleteGroup(string group)
        {
            Guard.NotEmpty(group, nameof(group));
            _entries.Add(new SettingEntry { Key = group.Trim(), KeyIsGroup = true });
        }

        /// <summary>
        /// Adds a setting if it doesn't exist yet.
        /// </summary>
        public void Add<TValue>(string key, TValue value)
        {
            Guard.NotEmpty(key, nameof(key));

            var valueStr = value.Convert<string>();
            _entries.Add(new SettingEntry { Key = key.Trim(), Value = valueStr });
        }

        internal void Reset()
        {
            _entries.Clear();
        }

        internal IEnumerable<SettingEntry> Build()
        {
            return _entries.Where(x => x.Key.HasValue());
        }
    }
}
