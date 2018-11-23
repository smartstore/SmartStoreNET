using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Newtonsoft.Json;
using SmartStore.Services;
using SmartStore.Services.Localization;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework.UI.Blocks
{
	public abstract class BlockHandlerBase<T> : IBlockHandler<T> where T : IBlock
	{
		public ICommonServices Services { get; set; }

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

			return block;
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
            RenderByChildAction(element, templates, htmlHelper, textWriter);
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

        protected abstract RouteInfo GetRoute(IBlockContainer element, string template);

		/// <summary>
		/// Add locales for localizable entities
		/// </summary>
		/// <typeparam name="TLocalizedModelLocal">Localizable model</typeparam>
		/// <param name="languageService">Language service</param>
		/// <param name="locales">Locales</param>
		/// <param name="configure">Configure action</param>
		protected virtual void AddLocales<TLocalizedModelLocal>(IList<TLocalizedModelLocal> locales, Action<TLocalizedModelLocal, int> configure) where TLocalizedModelLocal : ILocalizedModelLocal
		{
			var languageService = Services.Resolve<ILanguageService>();

			foreach (var language in languageService.GetAllLanguages(true))
			{
				var locale = Activator.CreateInstance<TLocalizedModelLocal>();
				locale.LanguageId = language.Id;
				if (configure != null)
				{
					configure.Invoke(locale, locale.LanguageId);
				}
				locales.Add(locale);
			}
		}
	}
}
