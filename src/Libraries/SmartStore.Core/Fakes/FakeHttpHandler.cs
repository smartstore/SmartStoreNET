using System;
using System.Web;

namespace SmartStore.Core.Fakes
{
    public class FakeHttpHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
