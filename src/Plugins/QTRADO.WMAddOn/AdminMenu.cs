using QTRADO.WMAddOn.Security;

using SmartStore.Collections;
using SmartStore.Web.Framework.UI;

namespace SmartStore.WMAddOn
{
    public class AdminMenu : AdminMenuProvider
    {
        protected override void BuildMenuCore(TreeNode<MenuItem> pluginsNode)
        {
            var menuItem = new MenuItem().ToBuilder()
                .Text("Werbemittel AddOn")
                .ResKey("Plugins.FriendlyName.QTRADO.WMAddOn")
                .Icon("far fa-images")
                .PermissionNames(WMAddOnPermissions.Read)
                .Action("ConfigurePlugin", "Plugin", new { systemName = "QTRADO.WMAddOn", area = "Admin" })
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
