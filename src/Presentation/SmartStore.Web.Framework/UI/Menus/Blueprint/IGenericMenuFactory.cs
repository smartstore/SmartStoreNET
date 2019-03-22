using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Web.Framework.UI.Menus
{
	/// <summary>
	/// Responsible for creating instances of <see cref="GenericMenu"/>.
	/// </summary>
	public interface IGenericMenuFactory
	{
		/// <summary>
		/// Creates a transient <see cref="GenericMenu"/> instance.
		/// </summary>
		/// <param name="menuName">The system name of the menu in the underlying storage.</param>
		/// <returns>The menu</returns>
		IMenu Create(string menuName);
	}
}
