using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using SmartStore.Core.Data;
using EfState = System.Data.Entity.EntityState;

namespace SmartStore.Data
{
    public static class DbEntityEntryExtensions
    {
        public static void ReloadEntity(this DbEntityEntry entry)
        {
            try
            {
                entry.Reload();
            }
            catch
            {
                // Can occur when entity has been detached in the meantime (for whatever fucking reasons)
                if (entry.State == EfState.Detached)
                {
                    entry.State = EfState.Unchanged;
                    entry.Reload();
                }
            }
        }

        /// <summary>
        /// Gets a dictionary with modified properties for the specified entity
        /// </summary>
        /// <param name="entry">The entity entry instance for which to get modified properties for</param>
        /// <returns>
        /// A dictionary, where the key is the name of the modified property
        /// and the value is its ORIGINAL value (which was tracked when the entity
        /// was attached to the context the first time)
        /// Returns an empty dictionary if no modification could be detected.
        /// </returns>
        public static IDictionary<string, object> GetModifiedProperties(this DbEntityEntry entry, IDbContext ctx)
        {
            var props = GetModifiedPropertyEntries(entry, ctx).ToDictionary(k => k.Name, v => v.OriginalValue);

            //System.Diagnostics.Debug.WriteLine("GetModifiedProperties: " + String.Join(", ", props.Select(x => x.Key)));

            return props;
        }

        /// <summary>
        /// Checks whether an entity entry has any modified property. 
        /// Only entities in <see cref="System.Data.Entity.EntityState.Modified"/> state are scanned for changes.
        /// Merged values provided by the <see cref="IMergedData"/> are ignored.
        /// </summary>
        /// <param name="entry">The entry instance</param>
        /// <param name="ctx">The data context</param>
        /// <returns><c>true</c> if any property has changed, <c>false</c> otherwise</returns>
        public static bool HasChanges(this DbEntityEntry entry, IDbContext ctx)
        {
            var hasChanges = GetModifiedPropertyEntries(entry, ctx).Any();
            return hasChanges;
        }

        internal static IEnumerable<DbPropertyEntry> GetModifiedPropertyEntries(this DbEntityEntry entry, IDbContext ctx)
        {
            // Be aware of the entity state. you cannot get modified properties for detached entities.
            EnsureChangesDetected(entry, ctx);

            if (entry.State != EfState.Modified)
            {
                yield break;
            }

            foreach (var name in entry.CurrentValues.PropertyNames)
            {
                var prop = entry.Property(name);
                if (prop != null && PropIsModified(prop, ctx))
                {
                    // INFO: under certain conditions DbPropertyEntry.IsModified returns true, even when values are equal
                    yield return prop;
                }
            }
        }

        public static bool IsPropertyModified(this DbEntityEntry entry, IDbContext ctx, string propertyName)
        {
            object originalValue;
            return TryGetModifiedProperty(entry, ctx, propertyName, out originalValue);
        }

        public static bool TryGetModifiedProperty(this DbEntityEntry entry, IDbContext ctx, string propertyName, out object originalValue)
        {
            Guard.NotEmpty(propertyName, nameof(propertyName));

            EnsureChangesDetected(entry, ctx);

            originalValue = null;

            if (entry.State != EfState.Modified)
            {
                return false;
            }

            var prop = entry.Property(propertyName);
            if (prop != null && PropIsModified(prop, ctx))
            {
                // INFO: under certain conditions DbPropertyEntry.IsModified returns true, even when values are equal
                originalValue = prop.OriginalValue;
                return true;
            }

            return false;
        }

        private static void EnsureChangesDetected(DbEntityEntry entry, IDbContext ctx)
        {
            var state = entry.State;

            if (ctx.AutoDetectChangesEnabled && state == EfState.Modified)
                return;

            if ((ctx as ObjectContextBase)?.IsInSaveOperation == true)
                return;

            if (state == EfState.Unchanged || state == EfState.Modified)
            {
                // When AutoDetectChanges is off we cannot be sure whether the entity is really unchanged,
                // because no detection was performed to verify this.
                DetectChangesInProperties(entry, ctx);
            }
        }

        public static void DetectChangesInProperties(this DbEntityEntry entry, IDbContext ctx)
        {
            ctx.DetectChanges();

            #region Experimental

            //// ChangeDetection for single entity: calls DbEntityEntry.InternalEntry > ObjectStateEntry._stateEntry.DetectChangesInProperties(bool)
            //var invoked = false;

            //try
            //{
            //	var internalEntry = FastProperty.GetProperty(entry.GetType(), "InternalEntry").GetValue(entry);
            //	if (internalEntry != null)
            //	{
            //		var objectStateEntry = FastProperty.GetProperty(internalEntry.GetType(), "ObjectStateEntry").GetValue(internalEntry);
            //		if (objectStateEntry != null)
            //		{
            //			var innerStateEntry = objectStateEntry.GetType().GetField("_stateEntry", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(objectStateEntry);
            //			if (innerStateEntry != null)
            //			{
            //				var dcMethod = innerStateEntry.GetType().GetMethod("DetectChangesInProperties", BindingFlags.NonPublic | BindingFlags.Instance);
            //				if (dcMethod != null)
            //				{
            //					//var requiresScalarChangeTracking = (bool)FastProperty.GetProperty(objectStateEntry.GetType(), "RequiresScalarChangeTracking").GetValue(objectStateEntry);
            //					//var requiresComplexChangeTracking = (bool)FastProperty.GetProperty(objectStateEntry.GetType(), "RequiresComplexChangeTracking").GetValue(objectStateEntry);
            //					dcMethod.Invoke(innerStateEntry, new object[] { false });
            //					invoked = true;
            //				}
            //			}
            //		}
            //	}
            //}
            //finally
            //{
            //	if (!invoked) ctx.DetectChanges();
            //}

            #endregion
        }

        private static bool PropIsModified(DbPropertyEntry prop, IDbContext ctx)
        {
            // INFO: "CurrentValue" cannot be used for entities in the Deleted state.
            // INFO: "OriginalValues" cannot be used for entities in the Added state.
            //return !AreEqual(prop.CurrentValue, prop.OriginalValue);
            return ctx.AutoDetectChangesEnabled
                ? prop.IsModified
                : !AreEqual(prop.CurrentValue, prop.OriginalValue);
        }

        private static bool AreEqual(object cur, object orig)
        {
            if (cur == null && orig == null)
                return true;

            return orig != null
                ? orig.Equals(cur)
                : cur.Equals(orig);
        }
    }
}
