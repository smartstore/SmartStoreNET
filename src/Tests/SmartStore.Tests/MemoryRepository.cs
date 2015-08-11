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
		private readonly IDictionary<int, T> _data = new Dictionary<int, T>();
		private IDbContext _dbContext;

		public IQueryable<T> Table
		{
			get { return _data.Values.AsQueryable(); }
		}

		public IQueryable<T> TableUntracked
		{
			get { return Table; }
		}

		public ICollection<T> Local
		{
			get { return _data.Values; }
		}

		public T Create()
		{
			return Activator.CreateInstance<T>();
		}

		public T GetById(object id)
		{
			int id2 = (int)id;
			T entity;
			if (_data.TryGetValue(id2, out entity))
			{
				return entity;
			}

			return null;
		}

		public void Insert(T entity)
		{
			_data[entity.Id] = entity;
		}

		public void InsertRange(IEnumerable<T> entities, int batchSize = 100)
		{
			entities.Each(x => _data[x.Id] = x);
		}

		public void Update(T entity)
		{
			_data[entity.Id] = entity;
		}

		public void UpdateRange(IEnumerable<T> entities)
		{
			entities.Each(x => _data[x.Id] = x);
		}

		public void Delete(T entity)
		{
			_data.Remove(entity.Id);
		}

		public void DeleteRange(IEnumerable<T> entities)
		{
			entities.Each(x => Delete(x));
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
					var dbSet = MockRepository.GenerateMock<DbSet<T>>();
					dbSet.Expect(x => ((IQueryable<T>)x).Provider).Return(_data.Values.AsQueryable().Provider);
					dbSet.Expect(x => ((IQueryable<T>)x).Expression).Return(_data.Values.AsQueryable().Expression);
					dbSet.Expect(x => ((IQueryable<T>)x).ElementType).Return(_data.Values.AsQueryable().ElementType);
					dbSet.Expect(x => ((IQueryable<T>)x).GetEnumerator()).Return(_data.Values.AsQueryable().GetEnumerator());
					
					_dbContext = MockRepository.GenerateMock<IDbContext>();
					_dbContext.Expect(x => x.Set<T>()).Return(dbSet);
					_dbContext.Expect(x => x.SaveChangesAsync()).Return(Task.FromResult<int>(0));
					_dbContext.Expect(x => x.ExecuteStoredProcedureList<T>(Arg<string>.Is.Anything)).Return(new List<T>());
					_dbContext.Expect(x => x.GetModifiedProperties(Arg<BaseEntity>.Is.Anything)).Return(new Dictionary<string, object>());
					_dbContext.Expect(x => x.BeginTransaction(Arg<IsolationLevel>.Is.Anything)).Return(MockRepository.GenerateMock<ITransaction>());
				}

				return _dbContext;
			}
		}

		public bool? AutoCommitEnabled { get; set; }
	}
}
