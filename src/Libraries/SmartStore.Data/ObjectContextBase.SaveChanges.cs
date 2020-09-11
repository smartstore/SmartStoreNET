using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Data.Hooks;
using SmartStore.Utilities;
using SmartStore.Utilities.ObjectPools;
using EfState = System.Data.Entity.EntityState;

namespace SmartStore.Data
{
    public abstract partial class ObjectContextBase
    {
        private SaveChangesOperation _currentSaveOperation;

        private readonly static ConcurrentDictionary<Type, bool> _hookableEntities = new ConcurrentDictionary<Type, bool>();

        private enum SaveStage
        {
            PreSave,
            PostSave
        }

        private IEnumerable<DbEntityEntry> GetChangedEntries()
        {
            return ChangeTracker.Entries().Where(x => x.State > EfState.Unchanged);
        }

        public IDbHookHandler DbHookHandler
        {
            get;
            set;
        }

        public override int SaveChanges()
        {
            var op = _currentSaveOperation;

            if (op != null)
            {
                if (op.Stage == SaveStage.PreSave)
                {
                    // This was called from within a PRE action hook. We must get out:... 
                    // 1.) to prevent cyclic calls
                    // 2.) we want new entities in the state tracker (added by pre hooks) to be committed atomically in the core SaveChanges() call later on.
                    return 0;
                }
                else if (op.Stage == SaveStage.PostSave)
                {
                    // This was called from within a POST action hook. Core SaveChanges() has already been called,
                    // but new entities could have been added to the state tracker by hooks.
                    // Therefore we need to commit them and get outta here, otherwise: cyclic nightmare!
                    // DetectChanges() here is important, 'cause we turned it off for the save process.
                    base.ChangeTracker.DetectChanges();
                    return SaveChangesCore();
                }
            }

            _currentSaveOperation = new SaveChangesOperation(this, this.DbHookHandler);

            Action endExecute = () =>
            {
                _currentSaveOperation?.Dispose();
                _currentSaveOperation = null;
            };

            using (new ActionDisposable(endExecute))
            {
                return _currentSaveOperation.Execute();
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            var op = _currentSaveOperation;

            if (op != null)
            {
                if (op.Stage == SaveStage.PreSave)
                {
                    // This was called from within a PRE action hook. We must get out:... 
                    // 1.) to prevent cyclic calls
                    // 2.) we want new entities in the state tracker (added by pre hooks) to be committed atomically in the core SaveChanges() call later on.
                    return Task.FromResult(0);
                }
                else if (op.Stage == SaveStage.PostSave)
                {
                    // This was called from within a POST action hook. Core SaveChanges() has already been called,
                    // but new entities could have been added to the state tracker by hooks.
                    // Therefore we need to commit them and get outta here, otherwise: cyclic nightmare!
                    // DetectChanges() here is important, 'cause we turned it off for the save process.
                    base.ChangeTracker.DetectChanges();
                    return SaveChangesCoreAsync(cancellationToken);
                }
            }

            _currentSaveOperation = new SaveChangesOperation(this, this.DbHookHandler);

            var result = _currentSaveOperation.ExecuteAsync(cancellationToken);

            result.ContinueWith(t =>
            {
                _currentSaveOperation?.Dispose();
                _currentSaveOperation = null;
            });

            return result;
        }

        /// <summary>
        /// Just calls <c>DbContext.SaveChanges()</c> without any sugar
        /// </summary>
        /// <returns>The number of affected records</returns>
        protected internal int SaveChangesCore()
        {
            return base.SaveChanges();
        }

        /// <summary>
        /// Just calls <c>DbContext.SaveChangesAsync()</c> without any sugar
        /// </summary>
        /// <returns>The number of affected records</returns>
        protected internal Task<int> SaveChangesCoreAsync(CancellationToken cancellationToken)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        private void IgnoreMergedData(IEnumerable<IMergedData> entries, bool ignore)
        {
            foreach (var entry in entries)
            {
                entry.MergedDataIgnore = ignore;
            }
        }

        internal bool IsInSaveOperation => _currentSaveOperation != null;

        private static bool IsHookableEntityType(Type entityType)
        {
            var isHookable = _hookableEntities.GetOrAdd(entityType, t =>
            {
                var attr = t.GetAttribute<HookableAttribute>(true);
                if (attr != null)
                {
                    return attr.IsHookable;
                }

                // Entities are hookable by default
                return true;
            });

            return isHookable;
        }

        class SaveChangesOperation : IDisposable
        {
            private SaveStage _stage;
            private IEnumerable<DbEntityEntry> _changedEntries;
            private ObjectContextBase _ctx;
            private IDbHookHandler _hookHandler;

            public SaveChangesOperation(ObjectContextBase ctx, IDbHookHandler hookHandler)
            {
                _ctx = ctx;
                _hookHandler = hookHandler;
            }

            public IEnumerable<DbEntityEntry> ChangedEntries => _changedEntries;

            public SaveStage Stage => _stage;

            private IDisposable ExecuteCore()
            {
                var autoDetectChanges = _ctx.Configuration.AutoDetectChangesEnabled;
                IEnumerable<IMergedData> mergeableEntities = null;

                // Suppress implicit DetectChanges() calls by EF,
                // e.g. called by SaveChanges(), ChangeTracker.Entries() etc.
                _ctx.Configuration.AutoDetectChangesEnabled = false;

                // Get all attached entries implementing IMergedData,
                // we need to ignore merge on them. Otherwise
                // EF's change detection may think that properties has changed
                // where they actually didn't.
                mergeableEntities = _ctx.GetMergeableEntitiesFromChangeTracker().ToArray();

                // Now ignore merged data, otherwise merged data will be saved to database
                _ctx.IgnoreMergedData(mergeableEntities, true);

                // We must detect changes earlier in the process
                // before hooks are executed. Therefore we suppressed the
                // implicit DetectChanges() call by EF and call it here explicitly.
                _ctx.ChangeTracker.DetectChanges();

                // Now get changed entries
                _changedEntries = _ctx.GetChangedEntries();

                // pre
                PreExecute(out IEnumerable<IHookedEntity> changedHookEntries);

                return new ActionDisposable(endExecute);

                void endExecute()
                {
                    try
                    {
                        // post
                        PostExecute(changedHookEntries);
                    }
                    finally
                    {
                        _ctx.Configuration.AutoDetectChangesEnabled = autoDetectChanges;
                        _ctx.IgnoreMergedData(mergeableEntities, false);
                    }
                }
            }

            public int Execute()
            {
                using (ExecuteCore())
                {
                    return _ctx.SaveChangesCore();
                }
            }

            public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
            {
                using (ExecuteCore())
                {
                    return await _ctx.SaveChangesCoreAsync(cancellationToken);
                }
            }

            private IEnumerable<IDbSaveHook> PreExecute(out IEnumerable<IHookedEntity> changedHookEntries)
            {
                bool enableHooks = false;
                bool importantHooksOnly = false;
                bool anyStateChanged = false;
                IEnumerable<IDbSaveHook> processedHooks = null;

                changedHookEntries = null;

                enableHooks = _changedEntries.Any(); // hooking is meaningless without hookable entries
                if (enableHooks)
                {
                    // despite the fact that hooking can be disabled, we MUST determine if any "important" pre hook exists.
                    // If yes, but hooking is disabled, we'll trigger only the important ones.
                    importantHooksOnly = !_ctx.HooksEnabled && _hookHandler.HasImportantSaveHooks();

                    // we'll enable hooking for this unit of work only when it's generally enabled,
                    // OR we have "important" hooks in the pipeline.
                    enableHooks = importantHooksOnly || _ctx.HooksEnabled;
                }

                if (enableHooks)
                {
                    var contextType = _ctx.GetType();

                    changedHookEntries = _changedEntries
                        .Select(x => new HookedEntity(contextType, x))
                        .Where(x => IsHookableEntityType(x.EntityType))
                        .ToArray();

                    // Regardless of validation (possible fixing validation errors too)
                    processedHooks = _hookHandler.TriggerPreSaveHooks(changedHookEntries, importantHooksOnly, out anyStateChanged);

                    if (processedHooks.Any() && changedHookEntries.Any(x => x.State == SmartStore.Core.Data.EntityState.Modified))
                    {
                        // Because at least one pre action hook has been processed,
                        // we must assume that entity properties has been changed.
                        // We need to call DetectChanges() again.
                        _ctx.ChangeTracker.DetectChanges();
                    }
                }

                if (anyStateChanged)
                {
                    // because the state of at least one entity has been changed during pre hooking
                    // we have to further reduce the set of hookable entities (for the POST hooks)
                    changedHookEntries = changedHookEntries.Where(x => x.InitialState > SmartStore.Core.Data.EntityState.Unchanged);
                }

                return processedHooks ?? Enumerable.Empty<IDbSaveHook>();
            }

            private IEnumerable<IDbSaveHook> PostExecute(IEnumerable<IHookedEntity> changedHookEntries)
            {
                if (changedHookEntries == null || !changedHookEntries.Any())
                    return Enumerable.Empty<IDbSaveHook>();

                // the existence of hook entries actually implies that hooking is enabled.

                _stage = SaveStage.PostSave;

                var importantHooksOnly = !_ctx.HooksEnabled && _hookHandler.HasImportantSaveHooks();

                return _hookHandler.TriggerPostSaveHooks(changedHookEntries, importantHooksOnly);
            }

            private string FormatValidationExceptionMessage(IEnumerable<DbEntityValidationResult> results)
            {
                var psb = PooledStringBuilder.Rent();
                var sb = (StringBuilder)psb;

                sb.Append("Entity validation failed" + Environment.NewLine);

                foreach (var res in results)
                {
                    var baseEntity = res.Entry.Entity as BaseEntity;
                    sb.AppendFormat("Entity Name: {0} - Id: {0} - State: {1}",
                        res.Entry.Entity.GetType().Name,
                        baseEntity != null ? baseEntity.Id.ToString() : "N/A",
                        res.Entry.State.ToString());
                    sb.AppendLine();

                    foreach (var validationError in res.ValidationErrors)
                    {
                        sb.AppendFormat("\tProperty: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        sb.AppendLine();
                    }
                }

                return psb.ToStringAndReturn();
            }

            public void Dispose()
            {
                _ctx = null;
                _hookHandler = null;
                _changedEntries = null;
            }
        }
    }
}
