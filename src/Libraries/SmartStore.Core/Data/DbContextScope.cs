using System;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Core.Data
{
    public class DbContextScope : IDisposable
    {
        private readonly bool _autoDetectChangesEnabled;
        private readonly bool _proxyCreationEnabled;
        private readonly bool _validateOnSaveEnabled;
		private readonly IDbContext _ctx;

        public DbContextScope(IDbContext ctx = null, bool? autoDetectChanges = null, bool? proxyCreation = null, bool? validateOnSave = null)
        {
			_ctx = ctx ?? EngineContext.Current.Resolve<IDbContext>();
            _autoDetectChangesEnabled = ctx.AutoDetectChangesEnabled;
            _proxyCreationEnabled = ctx.ProxyCreationEnabled;
            _validateOnSaveEnabled = ctx.ValidateOnSaveEnabled;
            
            if (autoDetectChanges.HasValue)
				_ctx.AutoDetectChangesEnabled = autoDetectChanges.Value;

            if (proxyCreation.HasValue)
				_ctx.ProxyCreationEnabled = proxyCreation.Value;

            if (validateOnSave.HasValue)
				_ctx.ValidateOnSaveEnabled = validateOnSave.Value;
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
        }

    }
}
