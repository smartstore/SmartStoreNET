using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Core.Data
{
    public class DbContextScope : IDisposable
    {
        private readonly IDbContext _ctx;
        private readonly bool _autoDetectChangesEnabled;
        private readonly bool _proxyCreationEnabled;
        private readonly bool _validateOnSaveEnabled;
        private readonly bool _forceNoTracking;
        private readonly bool _hooksEnabled;
        private readonly bool _autoCommit;
        private readonly bool _lazyLoading;


        public DbContextScope(IDbContext ctx = null,
            bool? autoDetectChanges = null,
            bool? proxyCreation = null,
            bool? validateOnSave = null,
            bool? forceNoTracking = null,
            bool? hooksEnabled = null,
            bool? autoCommit = null,
            bool? lazyLoading = null)
        {
            _ctx = ctx ?? EngineContext.Current.Resolve<IDbContext>();
            _autoDetectChangesEnabled = _ctx.AutoDetectChangesEnabled;
            _proxyCreationEnabled = _ctx.ProxyCreationEnabled;
            _validateOnSaveEnabled = _ctx.ValidateOnSaveEnabled;
            _forceNoTracking = _ctx.ForceNoTracking;
            _hooksEnabled = _ctx.HooksEnabled;
            _autoCommit = _ctx.AutoCommitEnabled;
            _lazyLoading = _ctx.LazyLoadingEnabled;

            if (autoDetectChanges.HasValue)
                _ctx.AutoDetectChangesEnabled = autoDetectChanges.Value;

            if (proxyCreation.HasValue)
                _ctx.ProxyCreationEnabled = proxyCreation.Value;

            if (validateOnSave.HasValue)
                _ctx.ValidateOnSaveEnabled = validateOnSave.Value;

            if (forceNoTracking.HasValue)
                _ctx.ForceNoTracking = forceNoTracking.Value;

            if (hooksEnabled.HasValue)
                _ctx.HooksEnabled = hooksEnabled.Value;

            if (autoCommit.HasValue)
                _ctx.AutoCommitEnabled = autoCommit.Value;

            if (lazyLoading.HasValue)
                _ctx.LazyLoadingEnabled = lazyLoading.Value;
        }

        public IDbContext DbContext => _ctx;

        public void LoadCollection<TEntity, TCollection>(
            TEntity entity,
            Expression<Func<TEntity, ICollection<TCollection>>> navigationProperty,
            bool force = false,
            Func<IQueryable<TCollection>, IQueryable<TCollection>> queryAction = null)
            where TEntity : BaseEntity
            where TCollection : BaseEntity
        {
            _ctx.LoadCollection(entity, navigationProperty, force, queryAction);
        }

        public void LoadReference<TEntity, TProperty>(
            TEntity entity,
            Expression<Func<TEntity, TProperty>> navigationProperty,
            bool force = false)
            where TEntity : BaseEntity
            where TProperty : BaseEntity
        {
            _ctx.LoadReference(entity, navigationProperty, force);
        }

        public int Commit()
        {
            return _ctx.SaveChanges();
        }

        public async Task<int> CommitAsync()
        {
            return await _ctx.SaveChangesAsync();
        }

        public void Dispose()
        {
            _ctx.AutoDetectChangesEnabled = _autoDetectChangesEnabled;
            _ctx.ProxyCreationEnabled = _proxyCreationEnabled;
            _ctx.ValidateOnSaveEnabled = _validateOnSaveEnabled;
            _ctx.ForceNoTracking = _forceNoTracking;
            _ctx.HooksEnabled = _hooksEnabled;
            _ctx.AutoCommitEnabled = _autoCommit;
            _ctx.LazyLoadingEnabled = _lazyLoading;
        }
    }
}
