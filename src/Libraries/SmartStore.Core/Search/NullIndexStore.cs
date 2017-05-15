//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using SmartStore.Utilities;

//namespace SmartStore.Core.Search
//{
//	public class NullIndexStore : IIndexStore
//	{
//		public void CreateIfNotExists() { }

//		public void Delete() { }

//		public string Scope
//		{
//			get { return ""; }
//		}

//		public bool Exists
//		{
//			get { return false; }
//		}

//		public DateTime? GetLastIndexedUtc() => null;

//		public void SetLastIndexedUtc(DateTime date) { }

//		public bool IsLocked
//		{
//			get { return false; }
//		}

//		public bool TryAcquireLock(out IDisposable lockObj)
//		{
//			lockObj = null;
//			return false;
//		}

//		public int DocumentCount
//		{
//			get { return 0; }
//		}

//		public IEnumerable<string> GetAllFields() => Enumerable.Empty<string>();

//		public void Clear() { }

//		public void SaveDocuments(IEnumerable<IIndexDocument> documents) { }

//		public void DeleteDocuments(IEnumerable<int> ids) { }
//	}
//}
