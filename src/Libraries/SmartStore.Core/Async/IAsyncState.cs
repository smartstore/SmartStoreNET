using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartStore.Core.Async
{
	public interface IAsyncState
	{
		bool Exists<T>(string name = null);

		T Get<T>(string name = null);
		IEnumerable<T> GetAll<T>();
		
		void Set<T>(T state, string name = null, bool neverExpires = false);
		void Update<T>(Action<T> update, string name = null);
		bool Remove<T>(string name = null);

		CancellationTokenSource GetCancelTokenSource<T>(string name = null);
		void SetCancelTokenSource<T>(CancellationTokenSource cancelTokenSource, string name = null);
		bool Cancel<T>(string name = null);
	}
}
