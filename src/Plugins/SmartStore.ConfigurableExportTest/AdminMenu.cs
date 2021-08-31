using SmartStore.Collections;
using SmartStore.Web.Framework.UI;


namespace SmartStore.ConfigurableExport
{
    public class AdminMenu : AdminMenuProvider
    {
        protected override void BuildMenuCore(TreeNode<MenuItem> pluginsNode)
        {
            var menuItem = new MenuItem().ToBuilder()
                .Text("My Plugin")
                .ResKey("Plugins.FriendlyName.SmartStore.ConfigurableExport")
                .Icon("far fa-images")

                .Action("ConfigurePlugin", "Plugin", new { systemName = "SmartStore.ConfigurableExportTest", area = "Admin" })
                .ToItem();

            pluginsNode.Prepend(menuItem);
        }

        public override int Ordinal
        {
            get
            {
                return -200;
            }
        }
    }
}
