namespace SmartStore.Web.Framework.UI
{
    public class EntityPicker : Component
    {
        public EntityPicker()
        {
            EntityType = "product";
            IconCssClass = "fa fa-search";
            HtmlAttributes["type"] = "button";
            HtmlAttributes.AppendCssClass("btn btn-secondary");
            HighlightSearchTerm = true;
            AppendMode = true;
            Delimiter = ",";
            FieldName = "id";
        }

        public string EntityType { get; set; }
        public int LanguageId { get; set; }

        public string TargetInputSelector
        {
            get => HtmlAttributes["data-target"] as string;
            set => HtmlAttributes["data-target"] = value;
        }

        public string Caption { get; set; }
        public string IconCssClass { get; set; }

        public string DialogTitle { get; set; }
        public string DialogUrl { get; set; }

        public bool DisableGroupedProducts { get; set; }
        public bool DisableBundleProducts { get; set; }
        public int[] DisabledEntityIds { get; set; }
        public string[] Selected { get; set; }

        public bool EnableThumbZoomer { get; set; }
        public bool HighlightSearchTerm { get; set; }

        public int MaxItems { get; set; }
        public bool AppendMode { get; set; }
        public string Delimiter { get; set; }
        public string FieldName { get; set; }

        public string OnDialogLoadingHandlerName { get; set; }
        public string OnDialogLoadedHandlerName { get; set; }
        public string OnSelectionCompletedHandlerName { get; set; }
    }
}
