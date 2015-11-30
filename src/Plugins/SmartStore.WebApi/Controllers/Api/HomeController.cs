using SmartStore.Web.Framework.WebApi.Security;
using System.Web.Http;

namespace SmartStore.WebApi.Controllers.Api
{
	[WebApiAuthenticate]
	public class HomeController : ApiController
	{
	}
}
