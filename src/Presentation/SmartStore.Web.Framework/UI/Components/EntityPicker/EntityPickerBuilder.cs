using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.Web.Framework.UI
{
    public class EntityPickerBuilder<TModel> : ComponentBuilder<EntityPicker, EntityPickerBuilder<TModel>, TModel>
    {
        public EntityPickerBuilder(EntityPicker component, HtmlHelper<TModel> htmlHelper)
            : base(component, htmlHelper)
        {
            WithRenderer(new ViewBasedComponentRenderer<EntityPicker>("EntityPicker"));
            DialogUrl(UrlHelper.GenerateUrl(
                null,
                "Picker",
                "Entity",
                new RouteValueDictionary { { "area", "" } },
                RouteTable.Routes,
                htmlHelper.ViewContext.RequestContext,
                false));
        }

        public EntityPickerBuilder<TModel> EntityType(string value)
        {
            base.Component.EntityType = value;
            return this;
        }

        public EntityPickerBuilder<TModel> LanguageId(int value)
        {
            base.Component.LanguageId = value;
            return this;
        }

        public EntityPickerBuilder<TModel> Caption(string value)
        {
            base.Component.Caption = value;
            return this;
        }

        public EntityPickerBuilder<TModel> IconCssClass(string value)
        {
            base.Component.IconCssClass = value;
            return this;
        }

        public EntityPickerBuilder<TModel> Tooltip(string value)
        {
            base.Component.HtmlAttributes["title"] = value;
            return this;
        }

        public EntityPickerBuilder<TModel> DialogTitle(string value)
        {
            base.Component.DialogTitle = value;
            return this;
        }

        public EntityPickerBuilder<TModel> DialogUrl(string value)
        {
            base.Component.DialogUrl = value;
            return this;
        }


        public EntityPickerBuilder<TModel> For<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            Guard.NotNull(expression, nameof(expression));

            return For(ExpressionHelper.GetExpressionText(expression));
        }

        public EntityPickerBuilder<TModel> For(string expression)
        {
            base.Component.TargetInputSelector = "#" + this.HtmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(expression);
            return this;
        }


        public EntityPickerBuilder<TModel> DisableGroupedProducts(bool value)
        {
            base.Component.DisableGroupedProducts = value;
            return this;
        }

        public EntityPickerBuilder<TModel> DisableBundleProducts(bool value)
        {
            base.Component.DisableBundleProducts = value;
            return this;
        }

        public EntityPickerBuilder<TModel> DisabledEntityIds(params int[] values)
        {
            base.Component.DisabledEntityIds = values;
            return this;
        }

        public EntityPickerBuilder<TModel> Selected(params string[] values)
        {
            base.Component.Selected = values;
            return this;
        }

        public EntityPickerBuilder<TModel> EnableThumbZoomer(bool value)
        {
            base.Component.EnableThumbZoomer = value;
            return this;
        }

        public EntityPickerBuilder<TModel> HighlightSearchTerm(bool value)
        {
            base.Component.HighlightSearchTerm = value;
            return this;
        }


        public EntityPickerBuilder<TModel> MaxItems(int value)
        {
            base.Component.MaxItems = value;
            return this;
        }

        public EntityPickerBuilder<TModel> AppendMode(bool value)
        {
            base.Component.AppendMode = value;
            return this;
        }

        public EntityPickerBuilder<TModel> Delimiter(string value)
        {
            base.Component.Delimiter = value;
            return this;
        }

        public EntityPickerBuilder<TModel> FieldName(string value)
        {
            base.Component.FieldName = value;
            return this;
        }


        public EntityPickerBuilder<TModel> OnDialogLoading(string handlerName)
        {
            base.Component.OnDialogLoadingHandlerName = handlerName;
            return this;
        }

        public EntityPickerBuilder<TModel> OnDialogLoaded(string handlerName)
        {
            base.Component.OnDialogLoadedHandlerName = handlerName;
            return this;
        }

        public EntityPickerBuilder<TModel> OnSelectionCompleted(string handlerName)
        {
            base.Component.OnSelectionCompletedHandlerName = handlerName;
            return this;
        }
    }
}
