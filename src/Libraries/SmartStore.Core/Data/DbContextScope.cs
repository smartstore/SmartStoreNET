using System;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Core.Data
{
    public class DbContextScope : IDisposable
    {
        private readonly bool _autoDetectChangesEnabled;
        private readonly bool _proxyCreationEnabled;
        private readonly bool _validateOnSaveEnabled;
		private readonly bool _forceNoTracking;
		private readonly IDbContext _ctx;

		public DbContextScope(IDbContext ctx = null, bool? autoDetectChanges = null, bool? proxyCreation = null, bool? validateOnSave = null, bool? forceNoTracking = null)
        {
			_ctx = ctx ?? EngineContext.Current.Resolve<IDbContext>();
			_autoDetectChangesEnabled = _ctx.AutoDetectChangesEnabled;
			_proxyCreationEnabled = _ctx.ProxyCreationEnabled;
			_validateOnSaveEnabled = _ctx.ValidateOnSaveEnabled;
			_forceNoTracking = _ctx.ForceNoTracking;
            
            if (autoDetectChanges.HasValue)
				_ctx.AutoDetectChangesEnabled = autoDetectChanges.Value;

            if (proxyCreation.HasValue)
				_ctx.ProxyCreationEnabled = proxyCreation.Value;

            if (validateOnSave.HasValue)
				_ctx.ValidateOnSaveEnabled = validateOnSave.Value;

			if (forceNoTracking.HasValue)
				_ctx.ForceNoTracking = forceNoTracking.Value;
        }

		public int Commit()
		{
			return _ctx.SaveChanges();
		}

        public void Dispose()
        {
			_ctx.AutoDetectChangesEnabled = _autoDetectChangesEnabled;
			_ctx.ProxyCreationEnabled = _proxyCreationEnabled;
			_ctx.ValidateOnSaveEnabled = _validateOnSaveEnabled;
			_ctx.ForceNoTracking = _forceNoTracking;
        }

    }
}
