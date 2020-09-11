using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Data.Setup
{
    internal class LocaleResourcesMigrator
    {
        private readonly SmartObjectContext _ctx;
        private readonly DbSet<Language> _languages;
        private readonly DbSet<LocaleStringResource> _resources;

        public LocaleResourcesMigrator(SmartObjectContext ctx)
        {
            Guard.NotNull(ctx, nameof(ctx));

            _ctx = ctx;
            _languages = _ctx.Set<Language>();
            _resources = _ctx.Set<LocaleStringResource>();
        }

        public void Migrate(IEnumerable<LocaleResourceEntry> entries, bool updateTouchedResources = false)
        {
            Guard.NotNull(entries, nameof(entries));

            if (!entries.Any() || !_languages.Any())
                return;

            using (var scope = new DbContextScope(_ctx, autoDetectChanges: false, hooksEnabled: false))
            {
                var langMap = _languages.ToDictionarySafe(x => x.LanguageCulture.EmptyNull().ToLower());

                var toDelete = new List<LocaleStringResource>();
                var toUpdate = new List<LocaleStringResource>();
                var toAdd = new List<LocaleStringResource>();

                bool IsEntryValid(LocaleResourceEntry entry, Language targetLang)
                {
                    if (entry.Lang == null)
                        return true;

                    var sourceLangCode = entry.Lang.ToLower();

                    if (targetLang != null)
                    {
                        var culture = targetLang.LanguageCulture;
                        if (culture == sourceLangCode || culture.StartsWith(sourceLangCode + "-"))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (langMap.ContainsKey(sourceLangCode))
                            return true;

                        if (langMap.Keys.Any(k => k.StartsWith(sourceLangCode + "-", StringComparison.OrdinalIgnoreCase)))
                            return true;
                    }

                    return false;
                }

                // Remove all entries with invalid lang identifier
                var invalidEntries = entries.Where(x => !IsEntryValid(x, null));
                if (invalidEntries.Any())
                {
                    entries = entries.Except(invalidEntries).ToArray();
                }

                foreach (var lang in langMap)
                {
                    var validEntries = entries.Where(x => IsEntryValid(x, lang.Value)).ToArray();
                    foreach (var entry in validEntries)
                    {
                        var db = GetResource(entry.Key, lang.Value.Id, toAdd, out bool isLocal);

                        if (db == null && entry.Value.HasValue() && !entry.UpdateOnly)
                        {
                            // ADD action
                            toAdd.Add(new LocaleStringResource { LanguageId = lang.Value.Id, ResourceName = entry.Key, ResourceValue = entry.Value });
                        }

                        if (db == null)
                            continue;

                        if (entry.Value == null)
                        {
                            // DELETE action
                            if (isLocal)
                                toAdd.Remove(db);
                            else
                                toDelete.Add(db);
                        }
                        else
                        {
                            if (isLocal)
                            {
                                db.ResourceValue = entry.Value;
                                continue;
                            }

                            // UPDATE action
                            if (updateTouchedResources || !db.IsTouched.GetValueOrDefault())
                            {
                                db.ResourceValue = entry.Value;
                                toUpdate.Add(db);
                                if (toDelete.Contains(db))
                                    toDelete.Remove(db);
                            }
                        }
                    }
                }

                // add new resources to context
                _resources.AddRange(toAdd);

                // remove deleted resources
                _resources.RemoveRange(toDelete);

                // save now
                int affectedRows = _ctx.SaveChanges();

                _ctx.DetachEntities<Language>();
                _ctx.DetachEntities<LocaleStringResource>();
            }
        }

        private LocaleStringResource GetResource(string key, int langId, IList<LocaleStringResource> local, out bool isLocal)
        {
            var res = local.FirstOrDefault(x => x.ResourceName.Equals(key, StringComparison.InvariantCultureIgnoreCase) && x.LanguageId == langId);
            isLocal = res != null;

            if (res == null)
            {
                var query = _resources.Where(x => x.ResourceName.Equals(key, StringComparison.InvariantCultureIgnoreCase) && x.LanguageId == langId);
                res = query.FirstOrDefault();
            }

            return res;
        }

    }

}
