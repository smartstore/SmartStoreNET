using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Web.WebPages;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Localization;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Framework
{
	public enum InputEditorType
    {   TextBox,
        Password,
        Hidden,
        Checkbox/*,
        RadioButton*/
    }
    
    public static class HtmlExtensions
    {
        public static MvcHtmlString Hint(this HtmlHelper helper, string value)
        {
            // create a
            var a = new TagBuilder("a");
            a.MergeAttribute("href", "#");
            a.MergeAttribute("onclick", "return false;");
            //a.MergeAttribute("rel", "tooltip");
            a.MergeAttribute("title", value);
            a.MergeAttribute("tabindex", "-1");
            a.AddCssClass("hint");

			// Create img
			var img = new TagBuilder("i");
			img.AddCssClass("fa fa-question-circle");

            a.InnerHtml = img.ToString();

            // Render tag
            return MvcHtmlString.Create(a.ToString());
        }

        public static HelperResult LocalizedEditor<T, TLocalizedModelLocal>(this HtmlHelper<T> helper, string name, Func<int, HelperResult> localizedTemplate, Func<T, HelperResult> standardTemplate)
            where T : ILocalizedModel<TLocalizedModelLocal>
            where TLocalizedModelLocal : ILocalizedModelLocal
        {
			return new HelperResult(writer =>
            {
                if (helper.ViewData.Model.Locales.Count > 1)
                {
					var languageService = EngineContext.Current.Resolve<ILanguageService>();

					writer.Write("<div class='locale-editor'>");
                    var tabStrip = helper.SmartStore().TabStrip().Name(name).SmartTabSelection(false).Style(TabsStyle.Tabs).AddCssClass("nav-locales").Items(x =>
                    {
						if (standardTemplate != null)
						{
							var masterLanguage = languageService.GetLanguageById(languageService.GetDefaultLanguageId());
							x.Add().Text(EngineContext.Current.Resolve<ILocalizationService>().GetResource("Admin.Common.Standard"))
								.ContentHtmlAttributes(new { @class = "locale-editor-content", data_lang = masterLanguage.LanguageCulture, data_rtl = masterLanguage.Rtl.ToString().ToLower() })
								.Content(standardTemplate(helper.ViewData.Model).ToHtmlString())
								.Selected(true);
						}

                        for (int i = 0; i < helper.ViewData.Model.Locales.Count; i++)
                        {
                            var locale = helper.ViewData.Model.Locales[i];
                            var language = languageService.GetLanguageById(locale.LanguageId);

 							x.Add().Text(language.Name)
								.ContentHtmlAttributes(new { @class = "locale-editor-content", data_lang = language.LanguageCulture, data_rtl = language.Rtl.ToString().ToLower() })
								.Content(localizedTemplate(i))
								.ImageUrl("~/Content/images/flags/" + language.FlagImageFileName)
								.Selected(i == 0 && standardTemplate == null);
                        }
                    }).ToHtmlString();
                    writer.Write(tabStrip);
                    writer.Write("</div>");
                }
                else if (standardTemplate != null)
                {
                    standardTemplate(helper.ViewData.Model).WriteTo(writer);
                }
            });
        }

        public static MvcHtmlString DeleteConfirmation<T>(this HtmlHelper<T> helper, string buttonsSelector = null) where T : EntityModelBase
        {
            return DeleteConfirmation<T>(helper, "", buttonsSelector);
        }

		/// <summary>
		/// Adds an action name parameter for using other delete action names		
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="helper"></param>
		/// <param name="actionName"></param>
		/// <param name="buttonsSelector"></param>
		/// <returns></returns>
		public static MvcHtmlString DeleteConfirmation<T>(this HtmlHelper<T> helper, string actionName, string buttonsSelector = null) where T : EntityModelBase
        {
            if (String.IsNullOrEmpty(actionName))
                actionName = "Delete";

            var modalId = MvcHtmlString.Create(helper.ViewData.ModelMetadata.ModelType.Name.ToLower() + "-delete-confirmation").ToHtmlString();

            var deleteConfirmationModel = new DeleteConfirmationModel
            {
                Id = helper.ViewData.Model.Id,
                ControllerName = helper.ViewContext.RouteData.GetRequiredString("controller"),
                ActionName = actionName,
                ButtonSelector = buttonsSelector,
                // TODO: (MC) this is really awkward, but sufficient for the moment
                EntityType = buttonsSelector.Replace("-delete", "")
            };

			var script = string.Empty;
			if (buttonsSelector.HasValue())
			{
				script = "<script>$(function() { $('#" + modalId + "').modal(); $('#" + buttonsSelector + "').on('click', function(e){e.preventDefault();openModalWindow('" + modalId + "');} );  });</script>\n";
			}

			helper.SmartStore().Window().Name(modalId)
				.Title(EngineContext.Current.Resolve<ILocalizationService>().GetResource("Admin.Common.AreYouSure"))
				.Content(helper.Partial("Delete", deleteConfirmationModel).ToHtmlString())
				.Show(false)
				.Render();

            return new MvcHtmlString(script);
        }

		public static MvcHtmlString SmartLabel(this HtmlHelper helper, string expression, string labelText, string hint = null, object htmlAttributes = null)
		{
			var result = new StringBuilder();

			var labelAttrs = new RouteValueDictionary(htmlAttributes);
			//labelAttrs.AppendCssClass("col-form-label");

			var label = helper.Label(expression, labelText, labelAttrs);

			result.Append("<div class='ctl-label'>");
			{
				result.Append(label);
				if (hint.HasValue())
				{
					result.Append(helper.Hint(hint).ToHtmlString());
				}
			}
			result.Append("</div>");

			return MvcHtmlString.Create(result.ToString());
		}

        public static MvcHtmlString SmartLabelFor<TModel, TValue>(
			this HtmlHelper<TModel> helper, 
			Expression<Func<TModel, TValue>> expression, 
			bool displayHint = true, 
			object htmlAttributes = null)
        {
			var metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
			metadata.AdditionalValues.TryGetValue("SmartResourceDisplayName", out object resourceDisplayName);

			return SmartLabelFor(helper, expression, resourceDisplayName as SmartResourceDisplayName, metadata, displayHint, htmlAttributes);
        }

		public static MvcHtmlString SmartLabelFor<TModel, TValue>(
			this HtmlHelper<TModel> helper,
			Expression<Func<TModel, TValue>> expression,
			string resourceKey,
			bool displayHint = true,
			object htmlAttributes = null)
		{
			Guard.NotEmpty(resourceKey, nameof(resourceKey));
			
			var metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
			var resourceDisplayName = new SmartResourceDisplayName(resourceKey, metadata.PropertyName);

			return SmartLabelFor(helper, expression, resourceDisplayName, metadata, displayHint, htmlAttributes);
		}

		private static MvcHtmlString SmartLabelFor<TModel, TValue>(
			this HtmlHelper<TModel> helper, 
			Expression<Func<TModel, TValue>> expression,
			SmartResourceDisplayName resourceDisplayName, 
			ModelMetadata metadata,
			bool displayHint = true, 
			object htmlAttributes = null)
		{
			var result = new StringBuilder();
			string labelText = null;
			string hint = null;

			if (resourceDisplayName != null)
			{
				// resolve label display name
				labelText = resourceDisplayName.DisplayName.NullEmpty();
				if (labelText == null)
				{
					// take reskey as absolute fallback
					labelText = resourceDisplayName.ResourceKey;
				}

				// resolve hint
				if (displayHint)
				{
					var langId = EngineContext.Current.Resolve<IWorkContext>().WorkingLanguage.Id;
					hint = EngineContext.Current.Resolve<ILocalizationService>().GetResource(resourceDisplayName.ResourceKey + ".Hint", langId, false, "", true);
				}
			}

			if (labelText == null)
			{
				labelText = metadata.PropertyName.SplitPascalCase();
			}

			var labelAttrs = new RouteValueDictionary(htmlAttributes);
			//labelAttrs.AppendCssClass("col-form-label");

			var label = helper.LabelFor(expression, labelText, labelAttrs);

			if (displayHint)
			{
				result.Append("<div class='ctl-label'>");
				{
					result.Append(label);
					if (hint.HasValue())
					{
						result.Append(helper.Hint(hint).ToHtmlString());
					}
				}
				result.Append("</div>");
			}
			else
			{
				result.Append(label);
			}

			return MvcHtmlString.Create(result.ToString());
		}

        public static string FieldNameFor<T, TResult>(this HtmlHelper<T> html, Expression<Func<T, TResult>> expression)
        {
            return html.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));
        }

        public static string FieldIdFor<T, TResult>(this HtmlHelper<T> html, Expression<Func<T, TResult>> expression)
        {
            var id = html.ViewData.TemplateInfo.GetFullHtmlFieldId(ExpressionHelper.GetExpressionText(expression));
            // because "[" and "]" aren't replaced with "_" in GetFullHtmlFieldId
            return id.Replace('[', '_').Replace(']', '_');
        }

        /// <summary>
        /// Creates a days, months, years drop down list using an HTML select control. 
        /// The parameters represent the value of the "name" attribute on the select control.
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="dayName">"Name" attribute of the day drop down list.</param>
        /// <param name="monthName">"Name" attribute of the month drop down list.</param>
        /// <param name="yearName">"Name" attribute of the year drop down list.</param>
        /// <param name="beginYear">Begin year</param>
        /// <param name="endYear">End year</param>
        /// <param name="selectedDay">Selected day</param>
        /// <param name="selectedMonth">Selected month</param>
        /// <param name="selectedYear">Selected year</param>
        /// <param name="localizeLabels">Localize labels</param>
        /// <returns></returns>
        public static MvcHtmlString DatePickerDropDowns(this HtmlHelper html,
            string dayName, string monthName, string yearName,
            int? beginYear = null, int? endYear = null,
            int? selectedDay = null, int? selectedMonth = null, int? selectedYear = null, bool localizeLabels = true, bool disabled = false)
        {
			var row = new TagBuilder("div");
			row.AddCssClass("row xs-gutters");

			var daysCol = new TagBuilder("div");
			daysCol.AddCssClass("col");

			var monthsCol = new TagBuilder("div");
			monthsCol.AddCssClass("col");

			var yearsCol = new TagBuilder("div");
			yearsCol.AddCssClass("col");

			var daysList = new TagBuilder("select");
            var monthsList = new TagBuilder("select");
            var yearsList = new TagBuilder("select");

            daysList.Attributes.Add("data-native-menu", "false");
            monthsList.Attributes.Add("data-native-menu", "false");
            yearsList.Attributes.Add("data-native-menu", "false");

            daysList.Attributes.Add("name", dayName);
            monthsList.Attributes.Add("name", monthName);
            yearsList.Attributes.Add("name", yearName);
            
            daysList.Attributes.Add("class", "date-part form-control noskin");
            monthsList.Attributes.Add("class", "date-part form-control noskin");
            yearsList.Attributes.Add("class", "date-part form-control noskin");

			daysList.Attributes.Add("data-minimum-results-for-search", "100");
			monthsList.Attributes.Add("data-minimum-results-for-search", "100");
			//yearsList.Attributes.Add("data-minimum-results-for-search", "100");

			if (disabled)
			{
				daysList.Attributes.Add("disabled", "disabled");
				monthsList.Attributes.Add("disabled", "disabled");
				yearsList.Attributes.Add("disabled", "disabled");
			}

            var days = new StringBuilder();
            var months = new StringBuilder();
            var years = new StringBuilder();

            string dayLocale, monthLocale, yearLocale;
            if (localizeLabels)
            {
                var locService = EngineContext.Current.Resolve<ILocalizationService>();
                dayLocale = locService.GetResource("Common.Day");
                monthLocale = locService.GetResource("Common.Month");
                yearLocale = locService.GetResource("Common.Year");
            }
            else
            {
                dayLocale = "Day";
                monthLocale = "Month";
                yearLocale = "Year";
            }

            days.AppendFormat("<option value=''>{0}</option>", dayLocale);
            for (int i = 1; i <= 31; i++)
                days.AppendFormat("<option value='{0}'{1}>{0}</option>", i,
                    (selectedDay.HasValue && selectedDay.Value == i) ? " selected=\"selected\"" : null);


            months.AppendFormat("<option value=''>{0}</option>", monthLocale);
            for (int i = 1; i <= 12; i++)
            {
                months.AppendFormat("<option value='{0}'{1}>{2}</option>",
                                    i, 
                                    (selectedMonth.HasValue && selectedMonth.Value == i) ? " selected=\"selected\"" : null,
                                    CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(i));
            }


            years.AppendFormat("<option value=''>{0}</option>", yearLocale);

            if (beginYear == null)
                beginYear = DateTime.UtcNow.Year - 90;
            if (endYear == null)
                endYear = DateTime.UtcNow.Year + 10;

            for (int i = beginYear.Value; i <= endYear.Value; i++)
                years.AppendFormat("<option value='{0}'{1}>{0}</option>", i,
                    (selectedYear.HasValue && selectedYear.Value == i) ? " selected=\"selected\"" : null);

            daysList.InnerHtml = days.ToString();
            monthsList.InnerHtml = months.ToString();
            yearsList.InnerHtml = years.ToString();

			daysCol.InnerHtml = daysList.ToString();
			monthsCol.InnerHtml = monthsList.ToString();
			yearsCol.InnerHtml = yearsList.ToString();

			row.InnerHtml = string.Concat(daysCol, monthsCol, yearsCol);

			return MvcHtmlString.Create(row.ToString());
        }

        #region DropDownList Extensions

        private static readonly SelectListItem[] _singleEmptyItem = new[] { new SelectListItem { Text = "", Value = "" } };

        public static MvcHtmlString DropDownListForEnum<TModel, TEnum>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum>> expression,
            string optionLabel = null) where TEnum : struct
        {

            return htmlHelper.DropDownListForEnum(expression, null, optionLabel);
        }

        public static MvcHtmlString DropDownListForEnum<TModel, TEnum>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum>> expression,
            object htmlAttributes,
            string optionLabel = null) where TEnum : struct
        {
            IDictionary<string, object> attrs = null;
            if (htmlAttributes != null)
            {
                attrs = CommonHelper.ObjectToDictionary(htmlAttributes);
            }

            return htmlHelper.DropDownListForEnum(expression, attrs, optionLabel);
        }

        public static MvcHtmlString DropDownListForEnum<TModel, TEnum>(
            this HtmlHelper<TModel> htmlHelper, 
            Expression<Func<TModel, TEnum>> expression,
            IDictionary<string, object> htmlAttributes,
            string optionLabel = null) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("An Enumeration type is required.", "expression");

            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            var workContext = EngineContext.Current.Resolve<IWorkContext>();

            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            Type enumType = metadata.ModelType.GetNonNullableType();
            IEnumerable<TEnum> values = Enum.GetValues(enumType).Cast<TEnum>();
            
            IEnumerable<SelectListItem> items =
                values.Select(value => new SelectListItem
                {
                    Text = value.GetLocalizedEnum(localizationService, workContext),
                    Value = Enum.GetName(enumType, value),
                    Selected = value.Equals(metadata.Model.Convert(enumType))
                });

            if (metadata.IsNullableValueType)
            {
                items = _singleEmptyItem.Concat(items);
            }

            return htmlHelper.DropDownListFor(expression, items, optionLabel, htmlAttributes);
        }

        #endregion

        public static MvcHtmlString Widget(this HtmlHelper helper, string widgetZone)
        {
			return helper.Widget(widgetZone, null);
        }

		public static MvcHtmlString Widget(this HtmlHelper helper, string widgetZone, object model)
		{ 
			var routeValues = GetWidgetsByZoneRouteValues(helper, widgetZone, model);
			if (routeValues != null)
			{
				return helper.Action("WidgetsByZone", "Widget", routeValues);
			}

			return MvcHtmlString.Empty;
		}

		public static void RenderWidget(this HtmlHelper helper, string widgetZone)
		{
			helper.RenderWidget(widgetZone, null);
		}

		public static void RenderWidget(this HtmlHelper helper, string widgetZone, object model)
		{
			var routeValues = GetWidgetsByZoneRouteValues(helper, widgetZone, model);
			if (routeValues != null)
			{
				helper.RenderAction("WidgetsByZone", "Widget", routeValues);
			}
		}

		private static object GetWidgetsByZoneRouteValues(HtmlHelper helper, string widgetZone, object model)
		{
			if (widgetZone.HasValue())
			{
				model = model ?? helper.ViewData.Model;
				var widgetSelector = EngineContext.Current.Resolve<IWidgetSelector>();
				var widgets = widgetSelector.GetWidgets(widgetZone, model).ToList();
				if (widgets.Any())
				{
					var zoneModel = new WidgetZoneModel { Widgets = widgets, WidgetZone = widgetZone, Model = model };
					return new { zoneModel = zoneModel, model = model, area = "" };
				}
			}

			return null;
		}

		public static IHtmlString MetaAcceptLanguage(this HtmlHelper html)
        {
            var acceptLang = HttpUtility.HtmlAttributeEncode(Thread.CurrentThread.CurrentUICulture.ToString());
            return new MvcHtmlString(string.Format("<meta name=\"accept-language\" content=\"{0}\"/>", acceptLang));
        }

		public static IHtmlString LanguageAttributes(this HtmlHelper html, bool omitLTR = false)
		{
			return LanguageAttributes(html, EngineContext.Current.Resolve<IWorkContext>().WorkingLanguage, omitLTR);
		}

		public static IHtmlString LanguageAttributes<T>(this HtmlHelper html, LocalizedValue<T> localizedValue)
		{
			Guard.NotNull(localizedValue, nameof(localizedValue));

			if (!localizedValue.BidiOverride)
			{
				return MvcHtmlString.Empty;
			}

			return LanguageAttributes(html, localizedValue.CurrentLanguage, false);
		}

		public static IHtmlString LanguageAttributes(this HtmlHelper html, Language currentLanguage, bool omitLTR = false)
		{
			Guard.NotNull(currentLanguage, nameof(currentLanguage));

			var code = currentLanguage.GetTwoLetterISOLanguageName();
			var rtl = currentLanguage.Rtl;

			var result = "lang=\"" + code + "\"";
			if (rtl || !omitLTR)
			{
				result += " dir=\"" + (rtl ? "rtl" : "ltr") + "\"";
			}

			return new MvcHtmlString(result);
		}

		public static IHtmlString LanguageAttributes(this HtmlHelper html, bool currentRtl, Language pageLanguage)
		{
			Guard.NotNull(pageLanguage, nameof(pageLanguage));

			if (currentRtl == pageLanguage.Rtl)
			{
				return MvcHtmlString.Empty;
			}

			var result = "dir=\"" + (currentRtl ? "rtl" : "ltr") + "\"";
			return new MvcHtmlString(result);
		}

		public static MvcHtmlString ControlGroupFor<TModel, TValue>(
            this HtmlHelper<TModel> html, 
            Expression<Func<TModel, TValue>> expression, 
            InputEditorType editorType = InputEditorType.TextBox,
            bool required = false,
            string helpHint = null,
			string breakpoint = "md")
        {
            if (editorType == InputEditorType.Hidden)
            {
                return html.HiddenFor(expression);
            }

			string inputHtml = "";
			var htmlAttributes = new RouteValueDictionary();
			var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
			var dataTypeName = metadata.DataTypeName.EmptyNull();
            var groupClass = "form-group row";
            var labelClass = "col-{0}-3 col-form-label".FormatInvariant(breakpoint.NullEmpty() ?? "md");
            var controlsClass = "col-{0}-9".FormatInvariant(breakpoint.NullEmpty() ?? "md");
            var sb = new StringBuilder("<div class='{0}'>".FormatWith(groupClass));

            if (editorType != InputEditorType.Checkbox)
            {
                var className = labelClass + (required ? " required" : "");
                var fieldId = html.IdFor(expression).ToString();
                sb.AppendLine(html.LabelFor(expression, new { @class = className, @for = fieldId }).ToString());
            }

            sb.AppendLine("<div class='{0}'>".FormatWith(controlsClass));

            if (!required && (editorType == InputEditorType.TextBox || editorType == InputEditorType.Password))
            {
				htmlAttributes.Add("placeholder", EngineContext.Current.Resolve<ILocalizationService>().GetResource("Common.Optional"));
            }

			switch (dataTypeName)
			{
				case "EmailAddress":
					htmlAttributes.Add("type", "email");
					break;
				case "PhoneNumber":
					htmlAttributes.Add("type", "tel");
					break;
			}
            
            htmlAttributes.Add("class", "form-control");
            
            switch (editorType)
            {
                case InputEditorType.Checkbox:
					CommonHelper.TryConvert<bool>(metadata.Model, out var isChecked);
                    inputHtml = string.Format("<div class='form-check'>{0}<label class='form-check-label' for='{1}'>{2}</label></div>",
                        html.CheckBox(ExpressionHelper.GetExpressionText(expression), isChecked, new { @class = "form-check-input" }).ToString(),
						html.IdFor(expression),
						metadata.DisplayName);
                    break;
                case InputEditorType.Password:
                    inputHtml = html.PasswordFor(expression, htmlAttributes).ToString();
                    break;
                default:
                    inputHtml = html.TextBoxFor(expression, htmlAttributes).ToString();
                    break;
            }
			
            sb.AppendLine(inputHtml);
            sb.AppendLine(html.ValidationMessageFor(expression).ToString());
            if (helpHint.HasValue())
            {
                sb.AppendLine(string.Format("<div class='form-text text-muted'>{0}</div>", helpHint));
            }
            sb.AppendLine("</div>"); // div.controls

            sb.AppendLine("</div>"); // div.control-group

            return MvcHtmlString.Create(sb.ToString());
        }

		public static MvcHtmlString ColorBox(this HtmlHelper html, string name, string color)
		{
			return ColorBox(html, name, color, null);
		}

        public static MvcHtmlString ColorBox(this HtmlHelper html, string name, string color, string defaultColor)
        {
			var sb = new StringBuilder();

			defaultColor = defaultColor.EmptyNull();
			var isDefault = color.IsCaseInsensitiveEqual(defaultColor);

            sb.Append("<div class='input-group colorpicker-component sm-colorbox' data-fallback-color='{0}'>".FormatInvariant(defaultColor));

            sb.AppendFormat(html.TextBox(name, isDefault ? "" : color, new { @class = "form-control colorval", placeholder = defaultColor }).ToHtmlString());
            sb.AppendFormat("<div class='input-group-append input-group-addon'><div class='input-group-text'><i class='thecolor' style='{0}'>&nbsp;</i></div></div>", defaultColor.HasValue() ? "background-color: " + defaultColor : "");

            sb.Append("</div>");

            return MvcHtmlString.Create(sb.ToString());
        }

		public static MvcHtmlString TableFormattedVariantAttributes(this HtmlHelper helper, string formattedVariantAttributes, string separatorLines = "<br />", string separatorValues = ": ") {
			var sb = new StringBuilder();
			string name, value;
			string[] lines = formattedVariantAttributes.SplitSafe(separatorLines);

			if (lines.Length <= 0)
				return MvcHtmlString.Empty;

			sb.Append("<table class=\"product-attribute-table\">");

			foreach (string line in lines) {
				sb.Append("<tr>");
				if (line.SplitToPair(out name, out value, separatorValues)) {
					sb.AppendFormat("<td class=\"column-name\">{0}:</td>", name);
					sb.AppendFormat("<td class=\"column-value\">{0}</td>", value);
				}
				else {
					sb.AppendFormat("<td colspan=\"2\">{0}</td>", line);
				}
				sb.Append("</tr>");
			}

			sb.Append("</table>");
			return MvcHtmlString.Create(sb.ToString());
		}

		//public static MvcHtmlString SettingEditorFor<TModel, TValue>(
		//	this HtmlHelper<TModel> helper,
		//	Expression<Func<TModel, TValue>> expression,
		//	string parentSelector = null,
		//	object additionalViewData = null)
		//{
		//	var editor = helper.EditorFor(expression, additionalViewData);

		//	var data = helper.ViewData[StoreDependingSettingHelper.ViewDataKey] as StoreDependingSettingData;
		//	if (data == null || data.ActiveStoreScopeConfiguration <= 0)
		//		return editor; // CONTROL

		//	var sb = new StringBuilder("<div class='form-row flex-nowrap multi-store-setting-group'>");
		//	sb.Append("<div class='col-auto'><div class='form-control-plaintext'>");
		//	sb.Append(helper.SettingOverrideCheckboxInternal(expression, data, parentSelector)); // CHECK
		//	sb.Append("</div></div>");
		//	sb.Append("<div class='col multi-store-setting-control'>");
		//	sb.Append(editor); // CONTROL
		//	sb.Append("</div></div>");

		//	return MvcHtmlString.Create(sb.ToString());
		//}

		public static MvcHtmlString SettingEditorFor<TModel, TValue>(
			this HtmlHelper<TModel> helper,
			Expression<Func<TModel, TValue>> expression,
			string parentSelector = null,
			object additionalViewData = null)
		{
			return SettingEditorFor(
				helper, 
				expression, 
				helper.EditorFor(expression, additionalViewData), 
				parentSelector);
		}

		public static MvcHtmlString SettingEditorFor<TModel, TValue>(
			this HtmlHelper<TModel> helper,
			Expression<Func<TModel, TValue>> expression,
			Func<TModel, HelperResult> editor,
			string parentSelector = null)
		{
			return SettingEditorFor(
				helper,
				expression,
				new MvcHtmlString(editor(helper.ViewData.Model).ToHtmlString()),
				parentSelector);
		}

		public static MvcHtmlString EnumSettingEditorFor<TModel, TValue>(
			this HtmlHelper<TModel> helper,
			Expression<Func<TModel, TValue>> expression,
			string parentSelector = null,
			object htmlAttributes = null,
			string optionLabel = null) where TValue : struct
		{
			return SettingEditorFor(
				helper,
				expression,
				helper.DropDownListForEnum(expression, htmlAttributes, optionLabel),
				parentSelector);
		}

		public static MvcHtmlString SettingEditorFor<TModel, TValue>(
			this HtmlHelper<TModel> helper,
			Expression<Func<TModel, TValue>> expression,
			MvcHtmlString editor,
			string parentSelector = null)
		{
			Guard.NotNull(expression, nameof(expression));
			Guard.NotNull(editor, nameof(editor));

			var data = helper.ViewData[StoreDependingSettingHelper.ViewDataKey] as StoreDependingSettingData;
			if (data == null || data.ActiveStoreScopeConfiguration <= 0)
				return editor; // CONTROL

			var sb = new StringBuilder("<div class='form-row flex-nowrap multi-store-setting-group'>");
			sb.Append("<div class='col-auto'><div class='form-control-plaintext'>");
			sb.Append(helper.SettingOverrideCheckboxInternal(expression, data, parentSelector)); // CHECK
			sb.Append("</div></div>");
			sb.Append("<div class='col multi-store-setting-control'>");
			sb.Append(editor.ToHtmlString()); // CONTROL
			sb.Append("</div></div>");

			return MvcHtmlString.Create(sb.ToString());
		}

		private static MvcHtmlString SettingOverrideCheckboxInternal<TModel, TValue>(
			this HtmlHelper<TModel> helper,
			Expression<Func<TModel, TValue>> expression,
			StoreDependingSettingData data,
			string parentSelector = null)
		{
			var fieldPrefix = helper.ViewData.TemplateInfo.HtmlFieldPrefix;
			var settingKey = ExpressionHelper.GetExpressionText(expression);
			var localizeService = EngineContext.Current.Resolve<ILocalizationService>();

			if (fieldPrefix.HasValue())
				settingKey = string.Concat(fieldPrefix, ".", settingKey);
			else if (!settingKey.Contains("."))
				settingKey = string.Concat(data.RootSettingClass, ".", settingKey);

			var overrideForStore = (data.OverrideSettingKeys.FirstOrDefault(x => x.IsCaseInsensitiveEqual(settingKey)) != null);
			var fieldId = settingKey + (settingKey.EndsWith("_OverrideForStore") ? "" : "_OverrideForStore");

			var sb = new StringBuilder();
			sb.Append("<label class='switch switch-blue multi-store-override-switch'>");

			sb.AppendFormat("<input type='checkbox' id='{0}' name='{0}' class='multi-store-override-option'", fieldId);
			sb.AppendFormat(" onclick='SmartStore.Admin.checkOverriddenStoreValue(this)' data-parent-selector='{0}'{1} />",
				parentSelector.EmptyNull(), overrideForStore ? " checked" : "");

			sb.AppendFormat("<span class='switch-toggle' data-on='{0}' data-off='{1}'></span>",
				localizeService.GetResource("Common.On").Truncate(3),
				localizeService.GetResource("Common.Off").Truncate(3));
			//sb.Append("</label>");
			// Controls are not floating, so line-break prevents different distances between them.
			sb.Append("</label>\r\n");

			return MvcHtmlString.Create(sb.ToString());
		}

		public static MvcHtmlString CollapsedText(this HtmlHelper helper, string text)
		{
			if (text.IsEmpty())
				return MvcHtmlString.Empty;

			var catalogSettings = EngineContext.Current.Resolve<CatalogSettings>();

			if (!catalogSettings.EnableHtmlTextCollapser)
				return MvcHtmlString.Create(text);

			string result = "<div class='more-less' data-max-height='{0}'><div class='more-block'>{1}</div></div>".FormatWith(
				catalogSettings.HtmlTextCollapsedHeight, text
			);

			return MvcHtmlString.Create(result);
		}

		public static MvcHtmlString IconForFileExtension(this HtmlHelper helper, string fileExtension, bool renderLabel = false)
		{
			return IconForFileExtension(helper, fileExtension, null, renderLabel);
		}

		public static MvcHtmlString IconForFileExtension(this HtmlHelper helper, string fileExtension, string extraCssClasses = null, bool renderLabel = false)
		{
			Guard.NotNull(helper, nameof(helper));
			Guard.NotEmpty(fileExtension, nameof(fileExtension));

			var icon = "file-o";
			var ext = fileExtension;

			if (ext != null && ext.StartsWith("."))
			{
				ext = ext.Substring(1);
			}

			if (ext.HasValue())
			{
				switch (ext.ToLowerInvariant())
				{
					case "pdf":
						icon = "file-pdf-o";
						break;
					case "doc":
					case "docx":
					case "docm":
					case "odt":
					case "dot":
					case "dotx":
					case "dotm":
						icon = "file-word-o";
						break;
					case "xls":
					case "xlsx":
					case "xlsm":
					case "xlsb":
					case "ods":
						icon = "file-excel-o";
						break;
					case "csv":
					case "tab":
						icon = "table";
						break;
					case "ppt":
					case "pptx":
					case "pptm":
					case "ppsx":
					case "odp":
					case "potx":
					case "pot":
					case "potm":
					case "pps":
					case "ppsm":
						icon = "file-powerpoint-o";
						break;
					case "zip":
					case "rar":
					case "7z":
						icon = "file-archive-o";
						break;
					case "png":
					case "jpg":
					case "jpeg":
					case "bmp":
					case "psd":
						icon = "file-image-o";
						break;
					case "mp3":
					case "wav":
					case "ogg":
					case "wma":
						icon = "file-audio-o";
						break;
					case "mp4":
					case "mkv":
					case "wmv":
					case "avi":
					case "asf":
					case "mpg":
					case "mpeg":
						icon = "file-video-o";
						break;
					case "txt":
						icon = "file-text-o";
						break;
					case "exe":
						icon = "gear";
						break;
					case "xml":
					case "html":
					case "htm":
						icon = "file-code-o";
						break;
				}
			}

			var label = ext.NaIfEmpty().ToUpper();

			var result = "<i class='fa fa-fw fa-{0}{1}' title='{2}'></i>".FormatInvariant(
				icon, 
				extraCssClasses.HasValue() ? " " + extraCssClasses : "",
				label);

			if (renderLabel)
			{
				if (ext.IsEmpty())
				{
					result = "<span class='text-muted'>{0}</span>".FormatInvariant("".NaIfEmpty());
				}
				else
				{
					result = result + "<span class='ml-1'>{0}</span>".FormatInvariant(label);
				}	
			}

			return MvcHtmlString.Create(result);
		}
	}
}

