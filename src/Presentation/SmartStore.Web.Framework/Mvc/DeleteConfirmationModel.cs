namespace SmartStore.Web.Framework.Mvc
{
    public class DeleteConfirmationModel : EntityModelBase
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string ButtonSelector { get; set; }
        public string EntityType { get; set; }
    }
}