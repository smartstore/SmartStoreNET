using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SmartStore.Core;
using SmartStore.Core.IO;
using SmartStore.Core.Search;
using SmartStore.Services.Tasks;
using SmartStore.Utilities;

namespace SmartStore.Services.Search
{
	public class DefaultIndexingService : IIndexingService
	{
		private readonly IIndexManager _indexManager;
		private readonly IEnumerable<IIndexCollector> _collectors;
		private readonly ILockFileManager _lockFileManager;
		private readonly IVirtualPathProvider _vpp;
		private readonly IApplicationEnvironment _env;

		public DefaultIndexingService(
			IIndexManager indexManager,
			IEnumerable<IIndexCollector> collectors,
			ILockFileManager lockFileManager,
			IVirtualPathProvider vpp,
			IApplicationEnvironment env)
		{
			_indexManager = indexManager;
			_collectors = collectors;
			_lockFileManager = lockFileManager;
			_vpp = vpp;
			_env = env;
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
			if (!_indexManager.HasAnyProvider())
				return;

			string path = GetStatusFilePath(scope);

			ILockFile lockFile;
			if (!_lockFileManager.TryAcquireLock(path, out lockFile))
			{
				// TODO: throw Exception or get out?
			}

			using (lockFile)
			{
				var provider = _indexManager.GetIndexProvider();
				var store = provider.GetIndexStore(scope);

				if (store.Exists)
				{
					store.Delete();
				}

				// TODO: delete info file
			}
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

			string path = GetStatusFilePath(scope);

			ILockFile lockFile;
			if (!_lockFileManager.TryAcquireLock(path, out lockFile))
			{
				// TODO: throw Exception or get out?
			}

			using (lockFile)
			{
				// TODO: progress, cancellation, set status

				var provider = _indexManager.GetIndexProvider();
				var store = provider.GetIndexStore(scope);
				var info = GetIndexInfo(scope);

				if (store.Exists && rebuild)
				{
					store.Delete();
				}

				store.CreateIfNotExists();

				DateTime? lastIndexedUtc = rebuild 
					? null 
					: info.LastIndexedUtc;

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

		public IndexInfo GetIndexInfo(string scope)
		{
			Guard.NotEmpty(scope, nameof(scope));

			var provider = _indexManager.GetIndexProvider();
			if (provider == null)
				return null;

			var store = provider.GetIndexStore(scope);
			var info = ReadStatusFile(store);

			info.Scope = scope;
			info.DocumentCount = store.DocumentCount;
			info.Fields = store.GetAllFields();

			return info;
		}

		private IndexInfo ReadStatusFile(IIndexStore store)
		{
			var info = new IndexInfo { Status = store.Exists ? IndexingStatus.Unavailable : IndexingStatus.Idle };

			string path = _vpp.Combine("~/App_Data", GetStatusFilePath(store.Scope));

			if (_vpp.FileExists(path))
			{
				info.Status = IndexingStatus.Idle;

				var xml = _vpp.ReadFile(path);

				try
				{
					var doc = XDocument.Parse(xml);

					var elLastIndexed = doc.Descendants("last-indexed-utc").FirstOrDefault()?.Value;
					if (elLastIndexed.HasValue())
					{
						info.LastIndexedUtc = elLastIndexed.Convert<DateTime?>()?.ToUniversalTime();
					}

					var elStatus = doc.Descendants("status").FirstOrDefault()?.Value;
					if (elStatus.HasValue())
					{
						info.Status = elStatus.Convert<IndexingStatus>();
					}
				}
				catch { }
			}

			return info;
		}

		private void SaveStatusFile(IndexInfo info)
		{
			// ...
		}

		private string GetStatusFilePath(string scope)
		{
			var fileName = SeoHelper.GetSeName("{0}-{1}.xml".FormatInvariant(scope, _env.EnvironmentIdentifier), false, false);
			return _vpp.Combine("Indexing", fileName);
		}

		private IIndexCollector GetCollectorFor(string scope)
		{
			Guard.NotEmpty(scope, nameof(scope));

			return _collectors.FirstOrDefault(x => x.Scope.IsCaseInsensitiveEqual(scope));
		}
	}
}
