using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Data;

namespace SmartStore.Data
{
    /// <summary>
    /// Entity Framework repository
    /// </summary>
    public partial class EfRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly IDbContext _context;
        private IDbSet<T> _entities;

        public EfRepository(IDbContext context)
        {
            this._context = context;
            this.AutoCommitEnabled = true;
        }

        #region interface members

        public virtual IQueryable<T> Table
        {
            get
            {
				if (_context.ForceNoTracking)
				{
					return this.Entities.AsNoTracking();
				}
				return this.Entities;
            }
        }

        public virtual IQueryable<T> TableUntracked
        {
            get
            {
                return this.Entities.AsNoTracking();
            }
        }

		public virtual ICollection<T> Local
		{
			get
			{
				return this.Entities.Local;
			}
		}

        public T Create()
        {
            return this.Entities.Create();
        }

        public T GetById(object id)
        {
            return this.Entities.Find(id);
        }

        public void Insert(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            this.Entities.Add(entity);
			
            if (this.AutoCommitEnabled)
                _context.SaveChanges();
        }

        public void InsertRange(IEnumerable<T> entities, int batchSize = 100)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException("entities");

                if (entities.Any())
                {
                    if (batchSize <= 0)
                    {
                        // insert all in one step
                        entities.Each(x => this.Entities.Add(x));
                        if (this.AutoCommitEnabled)
                            _context.SaveChanges();
                    }
                    else
                    {
                        int i = 1;
                        bool saved = false;
                        foreach (var entity in entities)
                        {
                            this.Entities.Add(entity);
                            saved = false;
                            if (i % batchSize == 0)
                            {
                                if (this.AutoCommitEnabled)
                                    _context.SaveChanges();
                                i = 0;
                                saved = true;
                            }
                            i++;
                        }

                        if (!saved)
                        {
                            if (this.AutoCommitEnabled)
                                _context.SaveChanges();
                        }
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {
                throw ex;
            }
        }

        public void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (this.AutoCommitEnabled)
            {
				if (!InternalContext.Configuration.AutoDetectChangesEnabled)
				{
					InternalContext.Entry(entity).State = System.Data.Entity.EntityState.Modified;
				}
				_context.SaveChanges();
            }
            else
            {
                try
                {
                    this.Entities.Attach(entity);
                    InternalContext.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                }
                finally { }
            }
        }

        public void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (InternalContext.Entry(entity).State == System.Data.Entity.EntityState.Detached)
            {
                this.Entities.Attach(entity);
            }

            this.Entities.Remove(entity);

            if (this.AutoCommitEnabled)
                _context.SaveChanges();
        }

        public IQueryable<T> Expand(IQueryable<T> query, string path)
        {
            Guard.ArgumentNotNull(query, "query");
            Guard.ArgumentNotEmpty(path, "path");

            return query.Include(path);
        }

        public IQueryable<T> Expand<TProperty>(IQueryable<T> query, Expression<Func<T, TProperty>> path)
        {
            Guard.ArgumentNotNull(query, "query");
            Guard.ArgumentNotNull(path, "path");

            return query.Include(path);
        }

		public bool IsModified(T entity)
		{
			Guard.ArgumentNotNull(() => entity);
			var ctx = InternalContext;
			var entry = ctx.Entry(entity);

			if (entry != null)
			{
				var modified = entry.State == System.Data.Entity.EntityState.Modified;
				return modified;
			}

			return false;
		}

        public IDictionary<string, object> GetModifiedProperties(T entity)
        {
			return InternalContext.GetModifiedProperties(entity);
        }

        public IDbContext Context
        {
            get { return _context; }
        }

        public bool AutoCommitEnabled { get; set; }

        #endregion

        #region Helpers

        protected internal ObjectContextBase InternalContext
        {
            get { return _context as ObjectContextBase; }
        }

        private DbSet<T> Entities
        {
            get
            {
                if (_entities == null)
                {
                    _entities = _context.Set<T>();
                }
                return _entities as DbSet<T>;
            }
        }

        #endregion

    }
}