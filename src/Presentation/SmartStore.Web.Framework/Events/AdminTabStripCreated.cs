//using Telerik.Web.Mvc.UI;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Framework.Events
{
    /// <summary>
    /// Admin tabstrip created event
    /// </summary>
    public class AdminTabStripCreated
    {
        public AdminTabStripCreated(TabFactory itemFactory, string tabStripName)
        {
            this.ItemFactory = itemFactory;
            this.TabStripName = tabStripName;
        }

        public TabFactory ItemFactory { get; private set; }
        public string TabStripName { get; private set; }
    }
}