using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core
{
	/// <summary>
	/// Store context
	/// </summary>
	public interface IStoreContext
	{
		/// <summary>
		/// Gets or sets the current store
		/// </summary>
		Store CurrentStore { get; }

		/// <summary>
		/// IsSingleStoreMode ? 0 : CurrentStore.Id
		/// </summary>
		/// <remarks>codehint: sm-add</remarks>
		int CurrentStoreIdIfMultiStoreMode { get; }
	}
}
