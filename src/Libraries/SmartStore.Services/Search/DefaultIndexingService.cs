using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Search;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Search
{
	public class DefaultIndexingService : IIndexingService
	{
		private readonly IIndexManager _indexManager;
		private readonly IEnumerable<IIndexCollector> _collectors;

		public DefaultIndexingService(
			IIndexManager indexManager,
			IEnumerable<IIndexCollector> collectors)
		{
			_indexManager = indexManager;
			_collectors = collectors;
		}

		public IEnumerable<string> EnumerateScopes()
		{
			return _collectors.Select(x => x.Scope);
		}

		public void RebuildIndex(string scope, TaskExecutionContext context)
		{
			BuildIndexInternal(scope, true, context);
		}

		public void UpdateIndex(string scope, TaskExecutionContext context)
		{
			BuildIndexInternal(scope, false, context);
		}

		public void DeleteIndex(string scope)
		{
			throw new NotImplementedException();
		}

		public IndexInfo GetIndexInfo(string scope)
		{
			throw new NotImplementedException();
		}

		private void BuildIndexInternal(string scope, bool rebuild, TaskExecutionContext context)
		{
			if (!_indexManager.HasAnyProvider())
				return;

			var collector = GetCollectorFor(scope);

			if (collector == null)
			{
				// TODO: throw Exception or get out?
			}

			var provider = _indexManager.GetProvider();
			var store = provider.GetIndexStore(scope);

			IDisposable lockObj;

			if (!store.TryAcquireLock(out lockObj))
			{
				// TODO: throw Exception or get out?
			}

			using (lockObj)
			{
				// TODO: progress, cancellation

				if (store.Exists && rebuild)
				{
					store.Delete();
				}

				store.CreateIfNotExists();

				DateTime? lastIndexedUtc = rebuild 
					? null 
					: store.GetLastIndexedUtc();

				var segmenter = collector.Collect(lastIndexedUtc, (i) => provider.CreateDocument(i));

				while (segmenter.ReadNextSegment())
				{
					var segment = segmenter.CurrentSegment;

					if (!rebuild)
					{
						var toDelete = segment.Where(x => x.OperationType == IndexOperationType.Delete).Select(x => x.Document.Id);
						store.DeleteDocuments(toDelete);
					}

					var toIndex = segment.Where(x => x.OperationType == IndexOperationType.Index).Select(x => x.Document);
					store.SaveDocuments(toIndex);
				}
			}
		}

		private IIndexCollector GetCollectorFor(string scope)
		{
			Guard.NotEmpty(scope, nameof(scope));

			return _collectors.FirstOrDefault(x => x.Scope.IsCaseInsensitiveEqual(scope));
		}
	}
}
