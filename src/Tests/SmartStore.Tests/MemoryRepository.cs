using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Data;
using Rhino.Mocks;
using System.Data;
using System.Data.Entity;

namespace SmartStore.Tests
{
	public class MemoryRepository<T> : IRepository<T> where T : BaseEntity, new()
	{
		private readonly TestDbSet<T> _dbSet = new TestDbSet<T>();
		private IDbContext _dbContext;

		public IQueryable<T> Table
		{
			get { return _dbSet; }
		}

		public IQueryable<T> TableUntracked
		{
			get { return Table; }
		}

		public ICollection<T> Local
		{
			get { return _dbSet.Local; }
		}

		public T Create()
		{
			return _dbSet.Create<T>();
		}

		public T GetById(object id)
		{
			return _dbSet.Find(id);
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

		public void InsertRange(IEnumerable<T> entities, int batchSize = 100)
		{
			entities.Each(x => Insert(x));
		}

		public void Update(T entity)
		{
			// Noop
		}

		public void UpdateRange(IEnumerable<T> entities)
		{
			entities.Each(x => Update(x));
		}

		public void Delete(T entity)
		{
			_dbSet.Remove(entity);
		}

		public void DeleteRange(IEnumerable<T> entities)
		{
			_dbSet.RemoveRange(entities);
		}

		public IQueryable<T> Expand(IQueryable<T> query, string path)
		{
			return Table;
		}

		public IQueryable<T> Expand<TProperty>(IQueryable<T> query, Expression<Func<T, TProperty>> path)
		{
			return Table;
		}

		public bool IsModified(T entity)
		{
			return false;
		}

		public IDictionary<string, object> GetModifiedProperties(T entity)
		{
			return new Dictionary<string, object>();
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

					_dbContext = ctx;
				}

				return _dbContext;
			}
		}

		public bool? AutoCommitEnabled { get; set; }
	}
}
