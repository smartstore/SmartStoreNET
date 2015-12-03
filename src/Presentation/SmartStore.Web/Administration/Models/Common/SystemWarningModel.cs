using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Common
{
    public class SystemWarningModel : ModelBase
    {
        public SystemWarningLevel Level { get; set; }

        public string Text { get; set; }
    }

    public enum SystemWarningLevel
    {
        Pass,
        Warning,
        Fail
    }
}