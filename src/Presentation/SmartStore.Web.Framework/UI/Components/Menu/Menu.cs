namespace SmartStore.Web.Framework.UI
{
    public class Menu : Component
    {
        public override bool NameIsRequired => true;

        public string Template { get; set; }

        public RouteInfo Route { get; set; }
    }
}
