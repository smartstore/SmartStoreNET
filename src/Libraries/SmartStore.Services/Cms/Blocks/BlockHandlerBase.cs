using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Newtonsoft.Json;
using SmartStore.ComponentModel;
using SmartStore.Core.Logging;
using SmartStore.Services.Localization;
using SmartStore.Utilities;

namespace SmartStore.Services.Cms.Blocks
{
    public abstract class BlockHandlerBase<T> : IBlockHandler<T> where T : IBlock
    {
        public ICommonServices Services { get; set; }

        public ILogger Logger { get; set; }

        public ILocalizedEntityService LocalizedEntityService { get; set; }

        public virtual T Create(IBlockEntity entity)
        {
            return Activator.CreateInstance<T>();
        }

        public virtual T Load(IBlockEntity entity, StoryViewMode viewMode)
        {
            Guard.NotNull(entity, nameof(entity));

            var block = Create(entity);
            var json = entity.Model;

            if (json.IsEmpty())
            {
                return block;
            }

            JsonConvert.PopulateObject(json, block);

            if (block is IBindableBlock bindableBlock)
            {
                bindableBlock.BindEntityName = entity.BindEntityName;
                bindableBlock.BindEntityId = entity.BindEntityId;
            }

            return block;
        }

        public virtual bool IsValid(T block)
        {
            return true;
        }

        public virtual void Save(T block, IBlockEntity entity)
        {
            Guard.NotNull(entity, nameof(entity));

            if (block == null)
            {
                return;
            }

            var settings = new JsonSerializerSettings
            {
                ContractResolver = SmartContractResolver.Instance,
                TypeNameHandling = TypeNameHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                NullValueHandling = NullValueHandling.Ignore
            };

            entity.Model = JsonConvert.SerializeObject(block, Formatting.None, settings);

            // save BindEntintyName & BindEntintyId
            if (block is IBindableBlock bindableBlock)
            {
                entity.BindEntityId = bindableBlock.BindEntityId;
                entity.BindEntityName = bindableBlock.BindEntityName;
            }
        }

        public virtual void AfterSave(IBlockContainer container, IBlockEntity entity)
        {
            // Default impl does nothing.
        }

        public virtual void BeforeRender(IBlockContainer container, StoryViewMode viewMode, IBlockHtmlParts htmlParts)
        {
            // Default impl does nothing.
        }

        public virtual string Clone(IBlockEntity sourceEntity, IBlockEntity clonedEntity)
        {
            return sourceEntity.Model;
        }

        public void Render(IBlockContainer element, IEnumerable<string> templates, HtmlHelper htmlHelper)
        {
            RenderCore(element, templates, htmlHelper, htmlHelper.ViewContext.Writer);
        }

        public IHtmlString ToHtmlString(IBlockContainer element, IEnumerable<string> templates, HtmlHelper htmlHelper)
        {
            using (var writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                RenderCore(element, templates, htmlHelper, writer);
                return MvcHtmlString.Create(writer.ToString());
            }
        }

        protected virtual void RenderCore(IBlockContainer element, IEnumerable<string> templates, HtmlHelper htmlHelper, TextWriter textWriter)
        {
            RenderByView(element, templates, htmlHelper, textWriter);
        }

        protected void RenderByView(IBlockContainer element, IEnumerable<string> templates, HtmlHelper htmlHelper, TextWriter textWriter)
        {
            Guard.NotNull(element, nameof(element));
            Guard.NotNull(templates, nameof(templates));
            Guard.NotNull(htmlHelper, nameof(htmlHelper));
            Guard.NotNull(textWriter, nameof(textWriter));

            var viewContext = htmlHelper.ViewContext;

            if (!element.Metadata.IsInbuilt)
            {
                // Change "area" token in RouteData in order to begin search in the plugin's view folder.
                var originalRouteData = htmlHelper.ViewContext.RouteData;
                var routeData = new RouteData(originalRouteData.Route, originalRouteData.RouteHandler);
                routeData.Values.Merge(originalRouteData.Values);
                routeData.DataTokens["area"] = element.Metadata.AreaName;

                viewContext = new ViewContext
                {
                    RouteData = routeData,
                    RequestContext = htmlHelper.ViewContext.RequestContext
                };
            }

            var viewResult = FindFirstView(element.Metadata, templates, viewContext, out var searchedLocations);

            if (viewResult == null)
            {
                var msg = string.Format("No template found for '{0}'. Searched locations:\n{1}.", string.Join(", ", templates), string.Join("\n", searchedLocations));
                Logger.Debug(msg);
                throw new FileNotFoundException(msg);
            }

            var viewData = new ViewDataDictionary(element.Block);
            viewData.TemplateInfo.HtmlFieldPrefix = "Block";

            viewContext = new ViewContext(
                htmlHelper.ViewContext,
                viewResult.View,
                viewData,
                htmlHelper.ViewContext.TempData,
                textWriter);

            viewResult.View.Render(viewContext, textWriter);
        }

        private ViewEngineResult FindFirstView(IBlockMetadata blockMetadata, IEnumerable<string> templates, ViewContext viewContext, out ICollection<string> searchedLocations)
        {
            searchedLocations = new List<string>();

            foreach (var template in templates)
            {
                var viewName = string.Concat("BlockTemplates/", blockMetadata.SystemName, "/", template);
                var viewResult = ViewEngines.Engines.FindPartialView(viewContext, viewName);
                searchedLocations.AddRange(viewResult.SearchedLocations);
                if (viewResult.View != null)
                {
                    return viewResult;
                }
            }

            return null;
        }

        protected void RenderByChildAction(IBlockContainer element, IEnumerable<string> templates, HtmlHelper htmlHelper, TextWriter textWriter)
        {
            Guard.NotNull(element, nameof(element));
            Guard.NotNull(templates, nameof(templates));
            Guard.NotNull(htmlHelper, nameof(htmlHelper));
            Guard.NotNull(textWriter, nameof(textWriter));

            var routeInfo = templates.Select(x => GetRoute(element, x)).FirstOrDefault();
            if (routeInfo == null)
            {
                throw new InvalidOperationException("The return value of the 'GetRoute()' method cannot be NULL.");
            }

            var routeValues = routeInfo.RouteValues;

            routeValues["action"] = routeInfo.Action;
            routeValues["controller"] = routeInfo.Controller;

            VirtualPathData vpd = GetVirtualPathForArea(htmlHelper.RouteCollection, htmlHelper.ViewContext.RequestContext, null /* name */, routeValues, out var usingAreas);
            if (vpd == null)
            {
                throw new InvalidOperationException("Could not find any matching route.");
            }

            if (usingAreas)
            {
                routeValues.Remove("area");
            }

            var routeData = CreateRouteData(vpd.Route, routeValues, vpd.DataTokens, htmlHelper.ViewContext);
            var httpContext = htmlHelper.ViewContext.HttpContext;
            var requestContext = new RequestContext(httpContext, routeData);

            // Create the controller instance
            var controller = ControllerBuilder.Current.GetControllerFactory().CreateController(requestContext, routeInfo.Controller) as Controller;
            if (controller == null)
            {
                throw new InvalidOperationException($"Could not activate controller '{routeInfo.Controller}'. Please ensure that the controller class exists and inherits from '{typeof(Controller).FullName}'.");
            }

            var originalWriter = htmlHelper.ViewContext.Writer;
            htmlHelper.ViewContext.Writer = textWriter;

            var originalOutput = httpContext.Response.Output;
            httpContext.Response.Output = textWriter;

            var originalActionInvoker = controller.ActionInvoker;
            controller.ActionInvoker = new ActionInvokerWithResultValidator();

            void endRender()
            {
                htmlHelper.ViewContext.Writer = originalWriter;
                httpContext.Response.Output = originalOutput;
                controller.ActionInvoker = originalActionInvoker;
            }

            using (new ActionDisposable((Action)endRender))
            {
                ((IController)controller).Execute(requestContext);
            }
        }

        #region Legacy 'RenderByChildAction'

        //protected void RenderByChildAction(IBlockContainer element, IEnumerable<string> templates, HtmlHelper htmlHelper, TextWriter textWriter)
        //      {
        //          Guard.NotNull(element, nameof(element));
        //          Guard.NotNull(templates, nameof(templates));
        //          Guard.NotNull(htmlHelper, nameof(htmlHelper));
        //          Guard.NotNull(textWriter, nameof(textWriter));

        //          var routeInfo = templates.Select(x => GetRoute(element, x)).FirstOrDefault();
        //          if (routeInfo == null)
        //          {
        //              throw new InvalidOperationException("The return value of the 'GetRoute()' method cannot be NULL.");
        //          }

        //          //routeInfo.RouteValues["model"] = element.Block;

        //          var originalWriter = htmlHelper.ViewContext.Writer;
        //          htmlHelper.ViewContext.Writer = textWriter;

        //          using (new ActionDisposable(() => htmlHelper.ViewContext.Writer = originalWriter))
        //          {
        //              htmlHelper.RenderAction(routeInfo.Action, routeInfo.Controller, routeInfo.RouteValues);
        //          }
        //      }

        #endregion

        protected virtual RouteInfo GetRoute(IBlockContainer element, string template)
        {
            throw new NotImplementedException();
        }

        class ActionInvokerWithResultValidator : AsyncControllerActionInvoker
        {
            protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
            {
                switch (actionResult)
                {
                    case PartialViewResult _:
                    case ContentResult _:
                    case EmptyResult _:
                        base.InvokeActionResult(controllerContext, actionResult);
                        break;
                    case HttpNotFoundResult nfr:
                        throw new InvalidOperationException(nfr.StatusDescription.NullEmpty() ?? $"The resource was not found ({nfr.StatusCode}).");
                    default:
                        throw new InvalidOperationException($"The action result type of an MVC route block must either be '{nameof(PartialViewResult)}', '{nameof(ContentResult)}' or '{nameof(EmptyResult)}'");
                }
            }
        }

        #region Copied from ASP.NET MVC

        private VirtualPathData GetVirtualPathForArea(RouteCollection routes, RequestContext requestContext, string name, RouteValueDictionary values, out bool usingAreas)
        {
            // Copied over from https://github.com/aspnet/AspNetWebStack/blob/master/src/System.Web.Mvc/RouteCollectionExtensions.cs#L53

            Guard.NotNull(routes, nameof(routes));

            if (!String.IsNullOrEmpty(name))
            {
                // the route name is a stronger qualifier than the area name, so just pipe it through
                usingAreas = false;
                return routes.GetVirtualPath(requestContext, name, values);
            }

            string targetArea = null;
            if (values != null)
            {
                if (values.TryGetValue("area", out var targetAreaRawValue))
                {
                    targetArea = targetAreaRawValue as string;
                }
                else
                {
                    // set target area to current area
                    if (requestContext != null)
                    {
                        targetArea = requestContext.RouteData.GetAreaName();
                    }
                }
            }

            // need to apply a correction to the RVD if areas are in use
            RouteValueDictionary correctedValues = values;
            RouteCollection filteredRoutes = FilterRouteCollectionByArea(routes, targetArea, out usingAreas);
            if (usingAreas)
            {
                correctedValues = new RouteValueDictionary(values);
                correctedValues.Remove("area");
            }

            VirtualPathData vpd = filteredRoutes.GetVirtualPath(requestContext, correctedValues);
            return vpd;
        }

        // This method returns a new RouteCollection containing only routes that matched a particular area.
        // The Boolean out parameter is just a flag specifying whether any registered routes were area-aware.
        private RouteCollection FilterRouteCollectionByArea(RouteCollection routes, string areaName, out bool usingAreas)
        {
            // Copied over from https://github.com/aspnet/AspNetWebStack/blob/master/src/System.Web.Mvc/RouteCollectionExtensions.cs#L18

            if (areaName == null)
            {
                areaName = String.Empty;
            }

            usingAreas = false;

            // Ensure that we continue using the same settings as the previous route collection
            // if we are using areas and the route collection is exchanged
            RouteCollection filteredRoutes = new RouteCollection
            {
                AppendTrailingSlash = routes.AppendTrailingSlash,
                LowercaseUrls = routes.LowercaseUrls,
                RouteExistingFiles = routes.RouteExistingFiles
            };

            using (routes.GetReadLock())
            {
                foreach (RouteBase route in routes)
                {
                    string thisAreaName = route.GetAreaName() ?? String.Empty;
                    usingAreas |= (thisAreaName.Length > 0);
                    if (String.Equals(thisAreaName, areaName, StringComparison.OrdinalIgnoreCase))
                    {
                        filteredRoutes.Add(route);
                    }
                }
            }

            // if areas are not in use, the filtered route collection might be incorrect
            return (usingAreas) ? filteredRoutes : routes;
        }

        private RouteData CreateRouteData(RouteBase route, RouteValueDictionary routeValues, RouteValueDictionary dataTokens, ViewContext parentViewContext)
        {
            var routeData = new RouteData();

            foreach (var kvp in routeValues)
            {
                routeData.Values.Add(kvp.Key, kvp.Value);
            }

            foreach (var kvp in dataTokens)
            {
                routeData.DataTokens.Add(kvp.Key, kvp.Value);
            }

            routeData.Route = route;
            routeData.DataTokens["ParentActionViewContext"] = parentViewContext;

            return routeData;
        }

        class ChildActionMvcHandler : MvcHandler
        {
            public ChildActionMvcHandler(RequestContext context)
                : base(context)
            {
            }

            protected override void AddVersionHeader(HttpContextBase httpContext)
            {
                // No version header for child actions
            }
        }

        #endregion
    }
}
