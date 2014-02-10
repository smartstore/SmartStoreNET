using System;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Core.Data
{
    public class DbContextScope : IDisposable
    {
        private readonly string _alias = null;
        private readonly bool _autoDetectChangesEnabled;
        private readonly bool _proxyCreationEnabled;
        private readonly bool _validateOnSaveEnabled;

        public DbContextScope(string alias = null, bool? autoDetectChanges = null, bool? proxyCreation = null, bool? validateOnSave = null)
        {
            var ctx = EngineContext.Current.Resolve<IDbContext>(alias);
            _alias = alias;
            _autoDetectChangesEnabled = ctx.AutoDetectChangesEnabled;
            _proxyCreationEnabled = ctx.ProxyCreationEnabled;
            _validateOnSaveEnabled = ctx.ValidateOnSaveEnabled;
            
            if (autoDetectChanges.HasValue)
                ctx.AutoDetectChangesEnabled = autoDetectChanges.Value;

            if (proxyCreation.HasValue)
                ctx.ProxyCreationEnabled = proxyCreation.Value;

            if (validateOnSave.HasValue)
                ctx.ValidateOnSaveEnabled = validateOnSave.Value;
        }

		public int Commit()
		{
			var ctx = EngineContext.Current.Resolve<IDbContext>(_alias);
			return ctx.SaveChanges();
		}

        public void Dispose()
        {
            var ctx = EngineContext.Current.Resolve<IDbContext>(_alias);
            ctx.AutoDetectChangesEnabled = _autoDetectChangesEnabled;
            ctx.ProxyCreationEnabled = _proxyCreationEnabled;
            ctx.ValidateOnSaveEnabled = _validateOnSaveEnabled;
        }

    }
}
