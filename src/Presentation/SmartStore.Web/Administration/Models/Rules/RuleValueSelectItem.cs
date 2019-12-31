namespace SmartStore.Admin.Models.Rules
{
    /// <summary>
    /// Represents rule value data for a select2 control.
    /// Properties must be in lower case to be recognized by select2.
    /// </summary>
    public class RuleValueSelectItem
    {
        /// <summary>
        /// Value.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Displayed text.
        /// </summary>
        public string text { get; set; }
        
        /// <summary>
        /// Option subtitle, e.g. the product SKU.
        /// </summary>
        public string subtitle { get; set; }

        /// <summary>
        /// Whether the item is selected.
        /// </summary>
        public bool selected { get; set; }
    }
}