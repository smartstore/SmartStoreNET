using SmartStore.Web.Framework.Modelling;

namespace SmartStore.DevTools.Models
{
    [CustomModelPart]
    public class BackendExtensionModel : ModelBase
    {
        public string Welcome { get; set; }
        public int ProductId { get; set; }
    }
}