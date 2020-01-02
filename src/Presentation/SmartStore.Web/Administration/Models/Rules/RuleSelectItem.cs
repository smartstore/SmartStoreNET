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
        /// Option subtitle, e.g. the product SKU.
        /// </summary>
        [JsonProperty("subtitle")]
        public string SubTitle { get; set; }

        /// <summary>
        /// Whether the item is selected.
        /// </summary>
        [JsonProperty("selected")]
        public bool Selected { get; set; }
    }
}