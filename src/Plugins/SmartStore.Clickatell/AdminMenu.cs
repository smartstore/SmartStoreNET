using SmartStore.Collections;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Clickatell
{
    public class AdminMenu : AdminMenuProvider
    {
        protected override void BuildMenuCore(TreeNode<MenuItem> pluginsNode)
        {
            var menuItem = new MenuItem().ToBuilder()
                .Text("Clickatell SMS Provider")
                .ResKey("Plugins.FriendlyName.SmartStore.Clickatell")
                .Icon("far fa-paper-plane")
                .Action("ConfigurePlugin", "Plugin", new { systemName = "SmartStore.Clickatell", area = "Admin" })
                .ToItem();

            pluginsNode.Prepend(menuItem);
        }

        public override int Ordinal => 100;

    }
}
