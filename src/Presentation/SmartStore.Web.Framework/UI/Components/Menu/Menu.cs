namespace SmartStore.Web.Framework.UI
{
    public class Menu : Component
    {
        public override bool NameIsRequired => true;

        public string ViewName { get; set; }

        public bool ResolveCounts { get; set; } = true;
    }
}
