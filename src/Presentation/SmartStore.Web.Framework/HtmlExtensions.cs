using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Web.WebPages;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Framework.UI;
using Telerik.Web.Mvc.UI;
using SmartStore.Utilities;
using System.Web;
using System.Threading; // codehint: sm-add

namespace SmartStore.Web.Framework
{
    
    // codehint: sm-add
    public enum InputEditorType
    {   TextBox,
        Password,
        Hidden,
        Checkbox/*,
        RadioButton*/
    }
    
    public static class HtmlExtensions
    {
        public static MvcHtmlString ResolveUrl(this HtmlHelper htmlHelper, string url)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            return MvcHtmlString.Create(urlHelper.Content(url));
        }

        // codehint: sm-edit
        public static MvcHtmlString Hint(this HtmlHelper helper, string value)
        {
            // create a
            var a = new TagBuilder("a");
            a.MergeAttribute("href", "javascript:void(0)");
            a.MergeAttribute("rel", "tooltip");
            a.MergeAttribute("title", value);
            a.MergeAttribute("tabindex", "-1");
            a.AddCssClass("hint");

            // Create img
            var img = new TagBuilder("i");

            // Add attributes
            img.MergeAttribute("class", "icon-question-sign");

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
                                .Content(localizedTemplate
                                    (i).
                                    ToHtmlString
                                    ())
                                .ImageUrl("~/Content/images/flags/" + language.FlagImageFileName);
                        }
                    }).ToHtmlString();
                    writer.Write(tabStrip);
                    writer.Write("</div>");

                    #region OBSOLETE
                    //var tabStrip = helper.Telerik().TabStrip().Name(name).Items(x =>
                    //{
                    //    x.Add().Text("Standard").Content(standardTemplate(helper.ViewData.Model).ToHtmlString()).Selected(true);
                    //    for (int i = 0; i < helper.ViewData.Model.Locales.Count; i++)
                    //    {
                    //        var locale = helper.ViewData.Model.Locales[i];
                    //        var language = EngineContext.Current.Resolve<ILanguageService>().GetLanguageById(locale.LanguageId);
                    //        x.Add().Text(language.Name)
                    //            .Content(localizedTemplate
                    //                (i).
                    //                ToHtmlString
                    //                ())
                    //            .ImageUrl("~/Content/images/flags/" + language.FlagImageFileName);
                    //    }
                    //}).ToHtmlString();
                    //writer.Write(tabStrip);
                    #endregion
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
                ActionName = actionName
            };

            var window = helper.SmartStore().Window().Name(modalId)
                .Title(EngineContext.Current.Resolve<ILocalizationService>().GetResource("Admin.Common.AreYouSure"))
                .Modal(true)
                .Visible(false)
                .Content(helper.Partial("Delete", deleteConfirmationModel).ToHtmlString())
                .ToHtmlString();

            return MvcHtmlString.Create(script + window);
        }

        // codehint: sm-edit
        public static MvcHtmlString SmartLabelFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool displayHint = true)
        {
            var result = new StringBuilder();
            var metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            var hintResource = string.Empty;
            object value = null;
            result.Append("<div class='ctl-label'>");
            result.Append(helper.LabelFor(expression));
            if (metadata.AdditionalValues.TryGetValue("SmartResourceDisplayName", out value))
            {
                var resourceDisplayName = value as SmartResourceDisplayName;
                if (resourceDisplayName != null && displayHint)
                {
                    var langId = EngineContext.Current.Resolve<IWorkContext>().WorkingLanguage.Id;
                    hintResource =
                        EngineContext.Current.Resolve<ILocalizationService>()
                        .GetResource(resourceDisplayName.ResourceKey + ".Hint", langId, false, resourceDisplayName.DisplayName);

                    result.Append(helper.Hint(hintResource).ToHtmlString());
                }
            }
            result.Append("</div>");
            
            return MvcHtmlString.Create(result.ToString());
        }

        // codehint: sm-add
        public static MvcHtmlString SmartLabel<TModel>(this HtmlHelper<TModel> helper, string resourceKey, bool displayHint = true)
        {
            return null;
        }

        public static MvcHtmlString RequiredHint(this HtmlHelper helper, string additionalText = null)
        {
            // Create tag builder
            var builder = new TagBuilder("span");
            builder.AddCssClass("required");
            var innerText = "*";
            //add additinal text if specified
            if (!String.IsNullOrEmpty(additionalText))
                innerText += " " + additionalText;
            builder.SetInnerText(innerText);
            // Render tag
            return MvcHtmlString.Create(builder.ToString());
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
            int? selectedDay = null, int? selectedMonth = null, int? selectedYear = null, bool localizeLabels = true)
        {
            var daysList = new TagBuilder("select");
            //daysList.MergeAttribute("placeholder", "TAGE");
            //daysList.MergeAttribute("style", "width: 80px");
            var monthsList = new TagBuilder("select");
            var yearsList = new TagBuilder("select");

            daysList.Attributes.Add("name", dayName);
            monthsList.Attributes.Add("name", monthName);
            yearsList.Attributes.Add("name", yearName);
            
            daysList.Attributes.Add("class", "date-part");
            monthsList.Attributes.Add("class", "date-part");
            yearsList.Attributes.Add("class", "date-part");

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

            days.AppendFormat("<option value='{0}'>{1}</option>", "0", dayLocale);
            for (int i = 1; i <= 31; i++)
                days.AppendFormat("<option value='{0}'{1}>{0}</option>", i,
                    (selectedDay.HasValue && selectedDay.Value == i) ? " selected=\"selected\"" : null);


            months.AppendFormat("<option value='{0}'>{1}</option>", "0", monthLocale);
            for (int i = 1; i <= 12; i++)
            {
                months.AppendFormat("<option value='{0}'{1}>{2}</option>",
                                    i, 
                                    (selectedMonth.HasValue && selectedMonth.Value == i) ? " selected=\"selected\"" : null,
                                    CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(i));
            }


            years.AppendFormat("<option value='{0}'>{1}</option>", "0", yearLocale);

            if (beginYear == null)
                beginYear = DateTime.UtcNow.Year - 100;
            if (endYear == null)
                endYear = DateTime.UtcNow.Year;

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
                attrs = CollectionHelper.ObjectToDictionary(htmlAttributes);
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
            var result = helper.Action("WidgetsByZone", "Widget", new { widgetZone = widgetZone });
            return result;
            //return MvcHtmlString.Create("");
        }

        // codehint: sm-add
        public static IHtmlString MetaAcceptLanguage(this HtmlHelper html)
        {
            var acceptLang = HttpUtility.HtmlAttributeEncode(Thread.CurrentThread.CurrentUICulture.ToString());
            return new HtmlString(string.Format("<meta name=\"accept-language\" content=\"{0}\"/>", acceptLang));
        }

        // codehint: sm-add
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
            var sb = new StringBuilder();

            sb.AppendFormat("<div class='input-append color sm-colorbox' data-color='{0}' data-color-format='hex'>", color);

            sb.AppendFormat(html.TextBox(name, color, new { @class = "span2 colorval" }).ToHtmlString());
            sb.AppendFormat("<span class='add-on'><i style='background-color:{0}; border:1px solid #bbb'></i></span>", color);

            sb.Append("</div>");

            html.AppendScriptParts("~/bundles/colorbox");

            return MvcHtmlString.Create(sb.ToString());
        }

		// codehint: sm-add
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

    }
}

