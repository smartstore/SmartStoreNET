using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using SmartStore.Core;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities;

namespace SmartStore.Data
{
	public abstract partial class ObjectContextBase
	{
		private IDbHookHandler _dbHookHandler;
		private SaveChangesOperation _currentSaveOperation;

		enum SaveStage
		{
			PreSave,
			PostSave
		}

		private IEnumerable<DbEntityEntry> GetChangedEntries()
		{
			return ChangeTracker.Entries().Where(x => x.State > System.Data.Entity.EntityState.Unchanged);
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
					//	// This was called from within a PRE action hook. We must get out:... 
					//	// 1.) to prevent cyclic calls
					//	// 2.) we want new entities in the state tracker (added by pre hooks) to be committed atomically in the core SaveChanges() call later on.
					return 0;
				}
				else if (op.Stage == SaveStage.PostSave)
				{
					//	// This was called from within a POST action hook. Core SaveChanges() has already been called,
					//	// but new entities could have been added to the state tracker by hooks.
					//	// Therefore we need to commit them and get outta here, otherwise: cyclic nightmare!
					return SaveChangesCore();
				}
			}

			_currentSaveOperation = new SaveChangesOperation(this, this.DbHookHandler);

			using (new ActionDisposable(() => _currentSaveOperation = null))
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
					//	// This was called from within a PRE action hook. We must get out:... 
					//	// 1.) to prevent cyclic calls
					//	// 2.) we want new entities in the state tracker (added by pre hooks) to be committed atomically in the core SaveChanges() call later on.
					return Task.FromResult(0);
				}
				else if (op.Stage == SaveStage.PostSave)
				{
					//	// This was called from within a POST action hook. Core SaveChanges() has already been called,
					//	// but new entities could have been added to the state tracker by hooks.
					//	// Therefore we need to commit them and get outta here, otherwise: cyclic nightmare!
					return SaveChangesCoreAsync(cancellationToken);
				}
			}

			_currentSaveOperation = new SaveChangesOperation(this, this.DbHookHandler);

			using (new ActionDisposable(() => _currentSaveOperation = null))
			{
				return _currentSaveOperation.ExecuteAsync(cancellationToken);
			}
		}

		/// <summary>
		/// Just calls <c>DbContext.SaveChanges()</c> without any sugar
		/// </summary>
		/// <returns>The number of affected records</returns>
		protected internal int SaveChangesCore()
		{
			var changedEntries = _currentSaveOperation?.ChangedEntries ?? GetChangedEntries();

			using (new ActionDisposable(() => IgnoreMergedData(changedEntries, false)))
			{
				IgnoreMergedData(changedEntries, true);
				return base.SaveChanges();
			}		
		}

		/// <summary>
		/// Just calls <c>DbContext.SaveChangesAsync()</c> without any sugar
		/// </summary>
		/// <returns>The number of affected records</returns>
		protected internal Task<int> SaveChangesCoreAsync(CancellationToken cancellationToken)
		{
			var changedEntries = _currentSaveOperation?.ChangedEntries ?? GetChangedEntries();

			using (new ActionDisposable(() => IgnoreMergedData(changedEntries, false)))
			{
				IgnoreMergedData(changedEntries, true);
				return base.SaveChangesAsync(cancellationToken);
			}
		}

		private void IgnoreMergedData(IEnumerable<DbEntityEntry> entries, bool ignore)
		{
			foreach (var entry in entries.Select(x => x.Entity).OfType<IMergedData>())
			{
				entry.MergedDataIgnore = ignore;
			}
		}

		class SaveChangesOperation : IDisposable
		{
			private SaveStage _stage;
			private IList<DbEntityEntry> _changedEntries;
			private ObjectContextBase _ctx;
			private IDbHookHandler _hookHandler;

			public SaveChangesOperation(ObjectContextBase ctx, IDbHookHandler hookHandler)
			{
				_ctx = ctx;
				_hookHandler = hookHandler;
				_changedEntries = ctx.GetChangedEntries().ToList();
			}

			public IEnumerable<DbEntityEntry> ChangedEntries
			{
				get { return _changedEntries; }
			}

			public SaveStage Stage
			{
				get { return _stage; }
			}

			public int Execute()
			{
				// pre
				HookedEntity[] changedHookEntries;
				PreExecute(out changedHookEntries);

				// save
				var result = _ctx.SaveChangesCore();

				// post
				PostExecute(changedHookEntries);

				return result;
			}

			public Task<int> ExecuteAsync(CancellationToken cancellationToken)
			{
				// pre
				HookedEntity[] changedHookEntries;
				PreExecute(out changedHookEntries);

				// save
				var result = _ctx.SaveChangesCoreAsync(cancellationToken);

				// post
				result.ContinueWith((t) =>
				{
					if (!t.IsFaulted)
					{
						PostExecute(changedHookEntries);
					}
				});
				
				return result;
			}

			private void PreExecute(out HookedEntity[] changedHookEntries)
			{
				bool enableHooks = false;
				bool importantHooksOnly = false;
				bool anyStateChanged = false;

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
					changedHookEntries = _changedEntries
						.Select(x => new HookedEntity(x))
						.ToArray();

					// Regardless of validation (possible fixing validation errors too)
					anyStateChanged = _hookHandler.TriggerPreSaveHooks(changedHookEntries, importantHooksOnly);
				}

				if (anyStateChanged)
				{
					// because the state of at least one entity has been changed during pre hooking
					// we have to further reduce the set of hookable entities (for the POST hooks)
					changedHookEntries = changedHookEntries
						.Where(x => x.InitialState > SmartStore.Core.Data.EntityState.Unchanged)
						.ToArray();
				}
			}

			private void PostExecute(HookedEntity[] changedHookEntries)
			{
				if (changedHookEntries == null || changedHookEntries.Length == 0)
					return;

				// the existence of hook entries actually implies that hooking is enabled.

				_stage = SaveStage.PostSave;

				var importantHooksOnly = !_ctx.HooksEnabled && _hookHandler.HasImportantSaveHooks();

				_hookHandler.TriggerPostSaveHooks(changedHookEntries, importantHooksOnly);
			}

			private string FormatValidationExceptionMessage(IEnumerable<DbEntityValidationResult> results)
			{
				var sb = new StringBuilder();
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

				return sb.ToString();
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
