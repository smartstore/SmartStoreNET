using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Configuration;

namespace SmartStore.Data.Setup
{

    internal class SettingsMigrator
    {
        private readonly SmartObjectContext _ctx;
        private readonly DbSet<Setting> _settings;

        public SettingsMigrator(SmartObjectContext ctx)
        {
            Guard.NotNull(ctx, nameof(ctx));

            _ctx = ctx;
            _settings = _ctx.Set<Setting>();
        }

        public void Migrate(IEnumerable<SettingEntry> entries)
        {
            Guard.NotNull(entries, nameof(entries));

            if (!entries.Any())
                return;

            using (var scope = new DbContextScope(_ctx, autoDetectChanges: false))
            {
                var toDelete = new List<Setting>();
                var toAdd = new List<Setting>();

                // First perform DELETE actions
                foreach (var entry in entries.Where(x => x.Value == null))
                {
                    bool isPattern = entry.KeyIsGroup;
                    if (!HasSettings(entry.Key, isPattern))
                        continue; // nothing to delete

                    var db = GetSettings(entry.Key, isPattern);
                    _settings.RemoveRange(db);
                }

                _ctx.SaveChanges();

                // Then perform ADD actions
                foreach (var entry in entries.Where(x => x.Value.HasValue()))
                {
                    var existing = toAdd.FirstOrDefault(x => x.Name.Equals(entry.Key, StringComparison.InvariantCultureIgnoreCase));
                    if (existing != null)
                    {
                        existing.Value = entry.Value;
                        continue;
                    }

                    if (HasSettings(entry.Key, false))
                        continue; // skip existing (we don't perform updates)

                    _settings.Add(new Setting
                    {
                        Name = entry.Key,
                        Value = entry.Value,
                        StoreId = 0
                    });
                }

                _ctx.SaveChanges();
            }
        }

        private bool HasSettings(string key, bool isPattern = false)
        {
            var query = BuildQuery(key, isPattern);
            return query.Any();
        }

        private IList<Setting> GetSettings(string key, bool isPattern = false)
        {
            var query = BuildQuery(key, isPattern);
            return query.ToList();
        }

        private IQueryable<Setting> BuildQuery(string key, bool isPattern = false)
        {
            var query = _settings.AsQueryable();
            if (isPattern)
            {
                query = query.Where(x => x.Name.StartsWith(key));
            }
            else
            {
                query = query.Where(x => x.Name.Equals(key));
            }

            return query;
        }
    }

}
