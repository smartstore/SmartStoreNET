using SmartStore.Core.Plugins;

namespace SmartStore.Services.Payments
{
	public abstract class PaymentPluginBase : PaymentMethodBase, IPlugin
	{
		/// <summary>
		/// Gets or sets the plugin descriptor
		/// </summary>
		public virtual PluginDescriptor PluginDescriptor { get; set; }

		/// <summary>
		/// Install plugin
		/// </summary>
		public virtual void Install()
		{
			PluginManager.MarkPluginAsInstalled(this.PluginDescriptor.SystemName);
		}

		/// <summary>
		/// Uninstall plugin
		/// </summary>
		public virtual void Uninstall()
		{
			PluginManager.MarkPluginAsUninstalled(this.PluginDescriptor.SystemName);
		}
	}
}
