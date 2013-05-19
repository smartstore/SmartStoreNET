//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Routing;
//using System.Web.Mvc;
//using dotless.Core.Input;

//// codehint: sm-add (whole file)

//namespace SmartStore.Web.Framework.Themes
//{
//    public class HttpFileReader : IFileReader
//    {
//        private readonly FileReader inner;

//        public HttpFileReader(FileReader inner)
//        {
//            this.inner = inner;
//        }

//        public bool DoesFileExist(string fileName)
//        {
//            if (!fileName.StartsWith("~/"))
//                return inner.DoesFileExist(fileName);

//            // always return true for perf reasons
//            return true;

//            //using (var client = new CustomWebClient())
//            //{
//            //    client.HeadOnly = true;
//            //    try
//            //    {
//            //        client.DownloadString(ConvertToAbsoluteUrl(fileName));
//            //        return true;
//            //    }
//            //    catch
//            //    {
//            //        return false;
//            //    }
//            //}
//        }

//        public byte[] GetBinaryFileContents(string fileName)
//        {
//            throw new NotImplementedException();
//        }

//        public string GetFileContents(string fileName)
//        {
//            if (!fileName.StartsWith("~/"))
//                return inner.GetFileContents(fileName);

//            using (var client = new CustomWebClient())
//            {
//                try
//                {
//                    var content = client.DownloadString(ConvertToAbsoluteUrl(fileName));
//                    return content;
//                }
//                catch
//                {
//                    return null;
//                }
//            }
//        }

//        private static string ConvertToAbsoluteUrl(string virtualPath)
//        {
//            return new Uri(HttpContext.Current.Request.Url, VirtualPathUtility.ToAbsolute(virtualPath)).AbsoluteUri;
//        }

//        public bool UseCacheDependencies
//        {
//            get { return false; }
//        }

//        private class CustomWebClient : WebClient
//        {
//            public bool HeadOnly { get; set; }
//            protected override WebRequest GetWebRequest(Uri address)
//            {
//                var request = base.GetWebRequest(address);
//                if (HeadOnly && request.Method == "GET")
//                    request.Method = "HEAD";

//                return request;
//            }
//        }

//    }
//}
