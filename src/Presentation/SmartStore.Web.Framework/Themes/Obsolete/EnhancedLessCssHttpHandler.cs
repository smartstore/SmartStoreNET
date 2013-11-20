//using System;
//using System.Web;
//using System.Web.SessionState;
//using dotless.Core;
//using dotless.Core.Abstractions;
//using dotless.Core.configuration;
//using dotless.Core.Input;
//using dotless.Core.Parameters;
//using dotless.Core.Response;
//using Microsoft.Practices.ServiceLocation;
//using SmartStore.Core;
//using SmartStore.Core.Data;
//using SmartStore.Core.Infrastructure;

//// codehint: sm-add (whole file)

//namespace SmartStore.Web.Framework.Themes
//{

//    public class EnhancedLessCssHttpHandler : LessCssHttpHandlerBase, IHttpHandler
//    {

//        private bool IsThemeableRequest
//        {
//            get
//            {
//                if (!DataSettingsHelper.DatabaseIsInstalled())
//                {
//                    return false;
//                }
//                else
//                {
//                    var webHelper = EngineContext.Current.Resolve<IWebHelper>();

//                    var requestUrl = webHelper.GetThisPageUrl(false);
//                    string themeUrl = string.Format("{0}themes", webHelper.GetStoreLocation());
//                    var isThemeableRequest = requestUrl.StartsWith(themeUrl + "/", StringComparison.InvariantCultureIgnoreCase);

//                    return isThemeableRequest;
//                }
//            }
//        }

//        public void ProcessRequest(HttpContext context)
//        {
//            try
//            {
//                IHttp http = Container.GetInstance<IHttp>();
//                IResponse response = Container.GetInstance<IResponse>();
//                ILessEngine engine = Container.GetInstance<ILessEngine>();
//                IFileReader fileReader = Container.GetInstance<IFileReader>();

//                var localPath = http.Context.Request.Url.LocalPath;
//                var source = fileReader.GetFileContents(localPath);

//                if (this.IsThemeableRequest)
//                {
//                    engine = new ParameterDecorator(engine, EngineContext.Current.Resolve<IParameterSource>());
//                }

//                var output = engine.TransformToCss(source, localPath);

//                response.WriteHeaders();
//                response.WriteCss(output);
//            }
//            catch (System.IO.FileNotFoundException ex)
//            {
//                context.Response.StatusCode = 404;
//                if (context.Request.IsLocal)
//                {
//                    context.Response.Write("/* File Not Found while parsing: " + ex.Message + " */");
//                }
//                else
//                {
//                    context.Response.Write("/* Error Occurred. Consult log or view on local machine. */");
//                }
//                context.Response.End();
//            }
//            catch (System.IO.IOException ex)
//            {
//                context.Response.StatusCode = 500;
//                if (context.Request.IsLocal)
//                {
//                    context.Response.Write("/* Error in less parsing: " + ex.Message + " */");
//                }
//                else
//                {
//                    context.Response.Write("/* Error Occurred. Consult log or view on local machine. */");
//                }
//                context.Response.End();
//            }
//            catch (Exception ex)
//            {
//                context.Response.StatusCode = 500;
//                context.Response.Write("/* Internal server error occurred: " + ex.Message + " */");
//                context.Response.End();
//            }
//        }

//        public bool IsReusable
//        {
//            get { return false; }
//        }
//    }
//}
