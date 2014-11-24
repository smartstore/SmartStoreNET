using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Plugins
{
	/// <summary>
	/// Enables provider developers to specify one or many widgets, which
	/// should automatically get (de)activated when the provider gets (de)activated.
	/// Useful in scenarios where separate widgets are responsible for the displaying of provider data. 
	/// </summary>
	/// <remarks>
	/// A widget should definitely NOT depend on multiple providers as the activation
	/// only occurs on a single item base.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class DependentWidgetsAttribute : Attribute
	{
		public DependentWidgetsAttribute(params string[] widgetSystemNames)
		{
			WidgetSystemNames = widgetSystemNames;
		}

		public string[] WidgetSystemNames { get; private set; }
	}
}
