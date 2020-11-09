using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Rhino.Mocks;
using SmartStore.Core;
using SmartStore.Core.Data;

namespace SmartStore.Tests
{
    public class MemoryRepository<T> : IRepository<T> where T : BaseEntity, new()
    {
        private readonly TestDbSet<T> _dbSet = new TestDbSet<T>();
        private IDbContext _dbContext;

        public IQueryable<T> Table => _dbSet;

        public IQueryable<T> TableUntracked => Table;

        public ICollection<T> Local => _dbSet.Local;

        public T Create()
        {
            return _dbSet.Create<T>();
        }

        public T GetById(object id)
        {
            return _dbSet.Find(id);
        }

        public Task<T> GetByIdAsync(object id)
        {
            return _dbSet.FindAsync(id);
        }

        public T Attach(T entity)
        {
            return _dbSet.Attach(entity);
        }

        public void Insert(T entity)
        {
            if (entity.Id <= 0)
            {
                entity.Id = Local.Count == 0 ? 1 : Local.Max(x => x.Id) + 1;
            }
            _dbSet.Add(entity);
        }


        public Task InsertAsync(T entity)
        {
            Insert(entity);
            return Task.FromResult(0);
        }

        public void InsertRange(IEnumerable<T> entities, int batchSize = 100)
        {
            entities.Each(x => Insert(x));
        }

        public Task InsertRangeAsync(IEnumerable<T> entities, int batchSize = 100)
        {
            InsertRange(entities, batchSize);
            return Task.FromResult(0);
        }

        public void Update(T entity)
        {
            // Noop
        }

        public Task UpdateAsync(T entity)
        {
            return Task.FromResult(0);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            entities.Each(x => Update(x));
        }

        public Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            UpdateRange(entities);
            return Task.FromResult(0);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public Task DeleteAsync(T entity)
        {
            Delete(entity);
            return Task.FromResult(0);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            DeleteRange(entities);
            return Task.FromResult(0);
        }

        public IQueryable<T> Expand(IQueryable<T> query, string path)
        {
            return Table;
        }

        public IQueryable<T> Expand<TProperty>(IQueryable<T> query, Expression<Func<T, TProperty>> path)
        {
            return Table;
        }

        public IDbContext Context
        {
            get
            {
                if (_dbContext == null)
                {
                    var ctx = MockRepository.GenerateMock<IDbContext>();
                    ctx.Stub(x => x.Set<T>()).Return(_dbSet);
                    ctx.Stub(x => x.SaveChangesAsync()).Return(Task.FromResult<int>(0));
                    ctx.Stub(x => x.ExecuteStoredProcedureList<T>(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything)).Return(new List<T>());
                    ctx.Stub(x => x.GetModifiedProperties(Arg<BaseEntity>.Is.Anything)).Return(new Dictionary<string, object>());
                    ctx.Stub(x => x.BeginTransaction(Arg<IsolationLevel>.Is.Anything)).Return(MockRepository.GenerateMock<ITransaction>());
                    ctx.Stub(x => x.ChangeState(Arg<BaseEntity>.Is.TypeOf, Arg<System.Data.Entity.EntityState>.Is.Anything))
                        .WhenCalled(ChangeState);

                    _dbContext = ctx;
                }

                return _dbContext;
            }
        }

        private void ChangeState(MethodInvocation invocation)
        {
            var entity = (T)invocation.Arguments[0];
            var state = (System.Data.Entity.EntityState)invocation.Arguments[1];

            if (state == System.Data.Entity.EntityState.Deleted)
            {
                if (_dbSet.Contains(entity))
                {
                    _dbSet.Remove(entity);
                }
            }
            else if (state == System.Data.Entity.EntityState.Added)
            {
                if (!_dbSet.Contains(entity))
                {
                    _dbSet.Add(entity);
                }
            }
        }

        public bool? AutoCommitEnabled { get; set; }
    }
}
