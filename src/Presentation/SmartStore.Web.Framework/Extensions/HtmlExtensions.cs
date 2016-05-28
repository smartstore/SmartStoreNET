﻿using System;
using System.Web;
using System.Threading;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Html;
using System.Web.WebPages;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Framework.Settings;
using SmartStore.Utilities;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Web.Framework.Modelling;

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

        public static HelperResult LocalizedEditor<T, TLocalizedModelLocal>(this HtmlHelper<T> helper, string name,
             Func<int, HelperResult> localizedTemplate,
             Func<T, HelperResult> standardTemplate)
            where T : ILocalizedModel<TLocalizedModelLocal>
            where TLocalizedModelLocal : ILocalizedModelLocal
        {
            return new HelperResult(writer =>
            {
                if (helper.ViewData.Model.Locales.Count > 1)
                {
                    writer.Write("<div class='well well-small'>");
                    var tabStrip = helper.SmartStore().TabStrip().Name(name).SmartTabSelection(false).Style(TabsStyle.Pills).Items(x =>
                    {
                        x.Add().Text("Standard").Content(standardTemplate(helper.ViewData.Model).ToHtmlString()).Selected(true);
                        for (int i = 0; i < helper.ViewData.Model.Locales.Count; i++)
                        {
                            var locale = helper.ViewData.Model.Locales[i];
                            var language = EngineContext.Current.Resolve<ILanguageService>().GetLanguageById(locale.LanguageId);
                            x.Add().Text(language.Name)
                                .Content(localizedTemplate(i).ToHtmlString())
                                .ImageUrl("~/Content/images/flags/" + language.FlagImageFileName);
                        }
                    }).ToHtmlString();
                    writer.Write(tabStrip);
                    writer.Write("</div>");
                }
                else
                {
                    standardTemplate(helper.ViewData.Model).WriteTo(writer);
                }
            });
        }

        public static MvcHtmlString DeleteConfirmation<T>(this HtmlHelper<T> helper, string buttonsSelector = null) where T : EntityModelBase
        {
            return DeleteConfirmation<T>(helper, "", buttonsSelector);
        }

        // Adds an action name parameter for using other delete action names
        public static MvcHtmlString DeleteConfirmation<T>(this HtmlHelper<T> helper, string actionName, string buttonsSelector = null) where T : EntityModelBase
        {
            if (String.IsNullOrEmpty(actionName))
                actionName = "Delete";

            var modalId = MvcHtmlString.Create(helper.ViewData.ModelMetadata.ModelType.Name.ToLower() + "-delete-confirmation").ToHtmlString();

            string script = "";
            if (!string.IsNullOrEmpty(buttonsSelector))
            {
                script = "<script>$(function() { $('#" + modalId + "').modal({show:false}); $('#" + buttonsSelector + "').click( function(e){e.preventDefault();openModalWindow('" + modalId + "');} );  });</script>\n";
            }

            var deleteConfirmationModel = new DeleteConfirmationModel
            {
                Id = helper.ViewData.Model.Id,
                ControllerName = helper.ViewContext.RouteData.GetRequiredString("controller"),
                ActionName = actionName,
                ButtonSelector = buttonsSelector,
                // TODO: (MC) this is really awkward, but sufficient for the moment
                EntityType = buttonsSelector.Replace("-delete", "")
            };

            var window = helper.SmartStore().Window().Name(modalId)
                .Title(EngineContext.Current.Resolve<ILocalizationService>().GetResource("Admin.Common.AreYouSure"))
                .Modal(true)
                .Visible(false)
                .Content(helper.Partial("Delete", deleteConfirmationModel).ToHtmlString())
                .ToHtmlString();

            return MvcHtmlString.Create(script + window);
        }

		public static MvcHtmlString SmartLabel(this HtmlHelper helper, string expression, string labelText, string hint = null, object htmlAttributes = null)
		{
			var result = new StringBuilder();

			var label = helper.Label(expression, labelText, htmlAttributes);

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
			object resourceDisplayName = null;
			metadata.AdditionalValues.TryGetValue("SmartResourceDisplayName", out resourceDisplayName);

			return SmartLabelFor(helper, expression, resourceDisplayName as SmartResourceDisplayName, metadata, displayHint, htmlAttributes);
        }

		public static MvcHtmlString SmartLabelFor<TModel, TValue>(
			this HtmlHelper<TModel> helper,
			Expression<Func<TModel, TValue>> expression,
			string resourceKey,
			bool displayHint = true,
			object htmlAttributes = null)
		{
			Guard.ArgumentNotEmpty(() => resourceKey);
			
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

			var label = helper.LabelFor(expression, labelText, htmlAttributes);

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
            var daysList = new TagBuilder("select");
            daysList.MergeAttribute("style", "width: 70px");
            var monthsList = new TagBuilder("select");
			monthsList.MergeAttribute("style", "width: 130px");
            var yearsList = new TagBuilder("select");
			yearsList.MergeAttribute("style", "width: 90px");

            daysList.Attributes.Add("data-native-menu", "false");
            monthsList.Attributes.Add("data-native-menu", "false");
            yearsList.Attributes.Add("data-native-menu", "false");

            daysList.Attributes.Add("name", dayName);
            monthsList.Attributes.Add("name", monthName);
            yearsList.Attributes.Add("name", yearName);
            
            daysList.Attributes.Add("class", "date-part");
            monthsList.Attributes.Add("class", "date-part");
            yearsList.Attributes.Add("class", "date-part");

			daysList.Attributes.Add("data-select-min-results-for-search", "100");
			monthsList.Attributes.Add("data-select-min-results-for-search", "100");
			//yearsList.Attributes.Add("data-select-min-results-for-search", "100");

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

            days.AppendFormat("<option>{0}</option>", dayLocale);
            for (int i = 1; i <= 31; i++)
                days.AppendFormat("<option value='{0}'{1}>{0}</option>", i,
                    (selectedDay.HasValue && selectedDay.Value == i) ? " selected=\"selected\"" : null);


            months.AppendFormat("<option>{0}</option>", monthLocale);
            for (int i = 1; i <= 12; i++)
            {
                months.AppendFormat("<option value='{0}'{1}>{2}</option>",
                                    i, 
                                    (selectedMonth.HasValue && selectedMonth.Value == i) ? " selected=\"selected\"" : null,
                                    CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(i));
            }


            years.AppendFormat("<option>{0}</option>", yearLocale);

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

            return MvcHtmlString.Create(string.Concat(daysList, monthsList, yearsList));
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
			if (widgetZone.HasValue())
			{
				model = model ?? helper.ViewData.Model;
				var widgetSelector = EngineContext.Current.Resolve<IWidgetSelector>();
				var widgets = widgetSelector.GetWidgets(widgetZone, model).ToArray();
				if (widgets.Any())
				{
					var zoneModel = new WidgetZoneModel { Widgets = widgets, WidgetZone = widgetZone, Model = model };
					var result = helper.Action("WidgetsByZone", "Widget", new { zoneModel = zoneModel, model = model, area = "" });
					return result;
				}
			}

			return MvcHtmlString.Create("");
		}

        public static IHtmlString MetaAcceptLanguage(this HtmlHelper html)
        {
            var acceptLang = HttpUtility.HtmlAttributeEncode(Thread.CurrentThread.CurrentUICulture.ToString());
            return new HtmlString(string.Format("<meta name=\"accept-language\" content=\"{0}\"/>", acceptLang));
        }

        public static MvcHtmlString ControlGroupFor<TModel, TValue>(
            this HtmlHelper<TModel> html, 
            Expression<Func<TModel, TValue>> expression, 
            InputEditorType editorType = InputEditorType.TextBox,
            bool required = false,
            string helpHint = null)
        {
            if (editorType == InputEditorType.Hidden)
            {
                return html.HiddenFor(expression);
            }
            
            var sb = new StringBuilder("<div class='control-group'>");

            if (editorType != InputEditorType.Checkbox)
            {
                var className = "control-label" + (required ? " required" : "");
                var fieldId = html.IdFor(expression).ToString();
                sb.AppendLine(html.LabelFor(expression, new { @class = className, @for = fieldId }).ToString());
            }

            sb.AppendLine("<div class='controls'>");
            string inputHtml = "";
            object attrs = null;
            if (!required && (editorType == InputEditorType.TextBox || editorType == InputEditorType.Password))
            {
                attrs = new { placeholder = "Optional" /* TODO: Loc */  };
            }
            //var x = ModelMetadata.FromLambdaExpression(expression, html.ViewData).DisplayName;
            switch (editorType)
            {
                case InputEditorType.Checkbox:
                    inputHtml = string.Format("<label class='checkbox'>{0} {1}</label>",
                        html.EditorFor(expression).ToString(),
                        ModelMetadata.FromLambdaExpression(expression, html.ViewData).DisplayName); // TBD: ist das OK so?
                    break;
                case InputEditorType.Password:
                    inputHtml = html.PasswordFor(expression, attrs).ToString();
                    break;
                default:
                    inputHtml = html.TextBoxFor(expression, attrs).ToString();
                    break;
            }
            sb.AppendLine(inputHtml);
            sb.AppendLine(html.ValidationMessageFor(expression).ToString());
            if (helpHint.HasValue())
            {
                sb.AppendLine(string.Format("<div class='help-block muted'>{0}</div>", helpHint));
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

            sb.AppendFormat("<span class='input-append color sm-colorbox' data-color='{0}' data-color-format='hex'>", color);

            sb.AppendFormat(html.TextBox(name, isDefault ? "" : color, new { @class = "span2 colorval", placeholder = defaultColor }).ToHtmlString());
            sb.AppendFormat("<span class='add-on'><i style='background-color:{0}; border:1px solid #bbb'></i></span>", color);

            sb.Append("</span>");

            var bootstrapJsRoot = "~/Content/bootstrap/js/";
            html.AppendScriptParts(false,
                bootstrapJsRoot + "custom/bootstrap-colorpicker.js",
                bootstrapJsRoot + "custom/bootstrap-colorpicker-globalinit.js");

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

		public static MvcHtmlString SettingOverrideCheckbox<TModel, TValue>(this HtmlHelper<TModel> helper,
			Expression<Func<TModel, TValue>> expression, string parentSelector = null)
		{
			var data = helper.ViewData[StoreDependingSettingHelper.ViewDataKey] as StoreDependingSettingData;

			if (data != null && data.ActiveStoreScopeConfiguration > 0)
			{
				var settingKey = ExpressionHelper.GetExpressionText(expression);
				var localizeService = EngineContext.Current.Resolve<ILocalizationService>();

				if (!settingKey.Contains("."))
					settingKey = data.RootSettingClass + "." + settingKey;

				var overrideForStore = (data.OverrideSettingKeys.FirstOrDefault(x => x.IsCaseInsensitiveEqual(settingKey)) != null);
				var fieldId = settingKey + (settingKey.EndsWith("_OverrideForStore") ? "" : "_OverrideForStore");

				var sb = new StringBuilder();
				sb.Append("<div class=\"onoffswitch-container\"><div class=\"onoffswitch\">");

				sb.AppendFormat("<input type=\"checkbox\" id=\"{0}\" name=\"{0}\" class=\"onoffswitch-checkbox multi-store-override-option\"", fieldId);
				sb.AppendFormat(" onclick=\"Admin.checkOverriddenStoreValue(this)\" data-parent-selector=\"{0}\"{1} />", parentSelector.EmptyNull(), overrideForStore ? " checked=\"checked\"" : "");

				sb.AppendFormat("<label class=\"onoffswitch-label\" for=\"{0}\">", fieldId);
				sb.AppendFormat("<span class=\"onoffswitch-on\">{0}</span>", localizeService.GetResource("Common.On").Truncate(3).ToUpper());
				sb.AppendFormat("<span class=\"onoffswitch-off\">{0}</span>", localizeService.GetResource("Common.Off").Truncate(3).ToUpper());
				sb.Append("<span class=\"onoffswitch-switch\"></span>");
				sb.Append("<span class=\"onoffswitch-inner\"></span>");
				sb.Append("</label>");
				sb.Append("</div></div>\r\n");		// controls are not floating, so line-break prevents different distances between them

				return MvcHtmlString.Create(sb.ToString());
			}
			return MvcHtmlString.Empty;
		}

		public static MvcHtmlString SettingEditorFor<TModel, TValue>(this HtmlHelper<TModel> helper, 
			Expression<Func<TModel, TValue>> expression, 
			string parentSelector = null,
			object additionalViewData = null)
		{
			var checkbox = helper.SettingOverrideCheckbox(expression, parentSelector);
			var editor = helper.EditorFor(expression, additionalViewData);

			return MvcHtmlString.Create(checkbox.ToString() + editor.ToString());
		}

		public static MvcHtmlString CollapsedText(this HtmlHelper helper, string text)
		{
			if (text.IsEmpty())
				return MvcHtmlString.Empty;

			var catalogSettings = EngineContext.Current.Resolve<CatalogSettings>();

			if (!catalogSettings.EnableHtmlTextCollapser)
				return MvcHtmlString.Create(text);

			string options = "{{\"adjustheight\":{0}}}".FormatWith(
				catalogSettings.HtmlTextCollapsedHeight
			);

			string result = "<div class='more-less' data-options='{0}'><div class='more-block'>{1}</div></div>".FormatWith(
				options, text
			);

			return MvcHtmlString.Create(result);
		}

		public static MvcHtmlString IconForFileExtension(this HtmlHelper helper, string fileExtension, bool renderExtensionText)
		{
			string result = "";

			if (fileExtension != null && fileExtension.StartsWith("."))
			{
				fileExtension = fileExtension.Substring(1);
			}

			if (fileExtension.IsCaseInsensitiveEqual("xml"))
			{
				result = "<i class='fa fa-file-code-o' title='{0}'></i>";
			}
			else if (fileExtension.IsCaseInsensitiveEqual("xls") || fileExtension.IsCaseInsensitiveEqual("xlsx"))
			{
				result = "<i class='fa fa-file-excel-o' title='{0}'></i>";
			}
			else if (fileExtension.IsCaseInsensitiveEqual("pdf"))
			{
				result = "<i class='fa fa-file-pdf-o' title='{0}'></i>";
			}
			else if (fileExtension.IsCaseInsensitiveEqual("zip"))
			{
				result = "<i class='fa fa-file-archive-o' title='{0}'></i>";
			}
			else if (fileExtension.IsCaseInsensitiveEqual("txt") || fileExtension.IsCaseInsensitiveEqual("csv"))
			{
				result = "<i class='fa fa-file-text-o' title='{0}'></i>";
			}
			else if (fileExtension.IsCaseInsensitiveEqual("doc"))
			{
				result = "<i class='fa fa-file-word-o' title='{0}'></i>";
			}
			else if (fileExtension.IsCaseInsensitiveEqual("jpg") || fileExtension.IsCaseInsensitiveEqual("png") || fileExtension.IsCaseInsensitiveEqual("gif"))
			{
				result = "<i class='fa fa-file-image-o' title='{0}'></i>";
			}

			if (renderExtensionText)
			{
				if (fileExtension.IsEmpty())
					result = "<span class='muted'>{0}</span>".FormatInvariant("".NaIfEmpty());
				else
					result = result + "<span class='ml4'>{0}</span>";
			}

			return MvcHtmlString.Create(result.FormatInvariant(fileExtension.NaIfEmpty().ToUpper()));
		}
    }
}

