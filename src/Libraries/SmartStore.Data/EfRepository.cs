using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Data.Caching;
using EfState = System.Data.Entity.EntityState;

namespace SmartStore.Data
{
    public partial class EfRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly IDbContext _context;
        private IDbSet<T> _entities;

        public EfRepository(IDbContext context)
        {
            this._context = context;
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

        public virtual T Create()
        {
            return this.Entities.Create();
        }

		public virtual T GetById(object id)
        {
            return this.Entities.Find(id);
        }

		public virtual T Attach(T entity)
		{
			return this.Entities.Attach(entity);
		}

		public virtual void Insert(T entity)
        {
			Guard.NotNull(entity, nameof(entity));

			this.Entities.Add(entity);

			if (this.AutoCommitEnabledInternal)
			{
				_context.SaveChanges();
			}
        }

		public virtual void InsertRange(IEnumerable<T> entities, int batchSize = 100)
        {
            try
            {
				Guard.NotNull(entities, nameof(entities));

				if (entities.Any())
                {
                    if (batchSize <= 0)
                    {
						// insert all in one step
						this.Entities.AddRange(entities);
						if (this.AutoCommitEnabledInternal)
						{
							_context.SaveChanges();
						}   
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
								if (this.AutoCommitEnabledInternal)
								{
									_context.SaveChanges();
								}  
                                i = 0;
                                saved = true;
                            }
                            i++;
                        }

                        if (!saved)
                        {
							if (this.AutoCommitEnabledInternal)
							{
								_context.SaveChanges();
							} 
                        }
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {
                throw ex;
            }
        }

		public virtual void Update(T entity)
        {
			Guard.NotNull(entity, nameof(entity));

			ChangeStateToModifiedIfApplicable(entity);

			if (this.AutoCommitEnabledInternal)
			{
				_context.SaveChanges();
			}
        }

		public virtual void UpdateRange(IEnumerable<T> entities)
		{
			Guard.NotNull(entities, nameof(entities));

			foreach (var entity in entities)
			{
				ChangeStateToModifiedIfApplicable(entity);
			}

			if (this.AutoCommitEnabledInternal)
			{
				_context.SaveChanges();
			}
		}

		private void ChangeStateToModifiedIfApplicable(T entity)
		{
			if (entity.IsTransientRecord())
				return;

			var entry = InternalContext.Entry(entity);

			if (entry.State == EfState.Detached)
			{
				// Entity was detached before or was explicitly constructed.
				// This unfortunately sets all properties to modified.
				entry.State = EfState.Modified;
			}
			else if (entry.State == EfState.Unchanged)
			{
				// We simply do nothing here, because it is ensured now that DetectChanges()
				// gets implicitly called prior SaveChanges().

				//if (this.AutoCommitEnabledInternal && !ctx.Configuration.AutoDetectChangesEnabled)
				//{
				//	_context.DetectChanges();
				//}
			}
		}

		public virtual void Delete(T entity)
        {
			Guard.NotNull(entity, nameof(entity));

			InternalContext.Entry(entity).State = EfState.Deleted;

			if (this.AutoCommitEnabledInternal)
			{
				_context.SaveChanges();
			}   
        }

		public virtual void DeleteRange(IEnumerable<T> entities)
		{
			Guard.NotNull(entities, nameof(entities));

			foreach (var entity in entities)
			{
				InternalContext.Entry(entity).State = EfState.Deleted;
			}

			if (this.AutoCommitEnabledInternal)
			{
				_context.SaveChanges();
			}	
		}

		[Obsolete("Use the extension method from 'SmartStore.Core, SmartStore.Core.Data' instead")]
        public IQueryable<T> Expand(IQueryable<T> query, string path)
        {
            Guard.NotNull(query, "query");
            Guard.NotEmpty(path, "path");

            return query.Include(path);
        }

		[Obsolete("Use the extension method from 'SmartStore.Core, SmartStore.Core.Data' instead")]
        public IQueryable<T> Expand<TProperty>(IQueryable<T> query, Expression<Func<T, TProperty>> path)
        {
            Guard.NotNull(query, "query");
            Guard.NotNull(path, "path");

            return query.Include(path);
        }

		public virtual IDbContext Context
        {
            get { return _context; }
        }

        public bool? AutoCommitEnabled { get; set; }

		private bool AutoCommitEnabledInternal
		{
			get
			{
				return this.AutoCommitEnabled ?? _context.AutoCommitEnabled;
			}
		}

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