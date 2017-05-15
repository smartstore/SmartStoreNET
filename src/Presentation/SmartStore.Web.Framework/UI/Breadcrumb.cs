using System;
using System.Collections.Generic;

namespace SmartStore.Web.Framework.UI
{
	public interface IBreadcrumb
	{
		void Track(MenuItem item);
		IReadOnlyList<MenuItem> Trail { get; }
	}

	public class DefaultBreadcrumb : IBreadcrumb
	{
		private List<MenuItem> _trail;

		public void Track(MenuItem item)
		{
			Guard.NotNull(item, nameof(item));

			if (_trail == null)
			{
				_trail = new List<MenuItem>();
			}

			_trail.Add(item);
		}

		public IReadOnlyList<MenuItem> Trail
		{
			get
			{
				return _trail;
			}
		}


	}
}
