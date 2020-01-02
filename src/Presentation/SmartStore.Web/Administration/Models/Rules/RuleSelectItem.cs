namespace SmartStore.Admin.Models.Rules
{
    /// <summary>
    /// Represents rule option for a select control.
    /// Properties must be in lower case to be recognized by select control.
    /// </summary>
    public class RuleSelectItem
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