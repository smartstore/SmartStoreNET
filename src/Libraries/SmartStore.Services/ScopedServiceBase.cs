using System;
using SmartStore.Utilities;

namespace SmartStore.Services
{
	public abstract class ScopedServiceBase : IScopedService
	{
		private bool _isInScope;

		/// <summary>
		/// Creates a long running unit of work in which cache eviction is suppressed
		/// </summary>
		/// <param name="clearCache">Specifies whether the cache should be evicted completely on batch disposal</param>
		/// <returns>A disposable unit of work</returns>
		public IDisposable BeginScope(bool clearCache = true)
		{
			if (_isInScope)
			{
				// nested batches are not supported
				return ActionDisposable.Empty;
			}

			OnBeginScope();
			_isInScope = true;	

			return new ActionDisposable(() =>
			{
				_isInScope = false;
				OnEndScope();
				if (clearCache && HasChanges)
				{
					ClearCache();
				}
			});
		}

		/// <summary>
		/// Gets a value indicating whether data has changed during a request, making cache eviction necessary.
		/// </summary>
		/// <remarks>
		/// Cache eviction sets this member to <c>false</c>
		/// </remarks>
		public bool HasChanges
		{
			get;
			protected set;
		}

		protected bool IsInScope
		{
			get { return _isInScope; }
		}

		public void ClearCache()
		{
			if (!_isInScope)
			{
				OnClearCache();
				HasChanges = false;
			}
		}

		protected abstract void OnClearCache();

		protected virtual void OnBeginScope() { }
		protected virtual void OnEndScope() { }
	}
}
