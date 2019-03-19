using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Newtonsoft.Json;
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

            //routeInfo.RouteValues["model"] = element.Block;

            var originalWriter = htmlHelper.ViewContext.Writer;
            htmlHelper.ViewContext.Writer = textWriter;

            using (new ActionDisposable(() => htmlHelper.ViewContext.Writer = originalWriter))
            {
                htmlHelper.RenderAction(routeInfo.Action, routeInfo.Controller, routeInfo.RouteValues);
            }
        }

        protected virtual RouteInfo GetRoute(IBlockContainer element, string template) 
			=> throw new NotImplementedException();
	}
}
