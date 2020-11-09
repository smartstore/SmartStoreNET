using System.Web;

namespace SmartStore.Core.Events
{
    public class AppStartedEvent
    {
        public HttpContextBase HttpContext { get; set; }
    }
}
