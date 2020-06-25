using Newtonsoft.Json;

namespace SmartStore.Admin.Models.Rules
{
    /// <summary>
    /// Represents a select list option for rules.
    /// </summary>
    public class RuleSelectItem
    {
        /// <summary>
        /// Value.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Displayed text.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Option hint, e.g. the product SKU.
        /// </summary>
        [JsonProperty("hint")]
        public string Hint { get; set; }

        /// <summary>
        /// Whether the item is selected.
        /// </summary>
        [JsonProperty("selected")]
        public bool Selected { get; set; }
    }
}