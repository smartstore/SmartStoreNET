using Newtonsoft.Json;

namespace SmartStore.Web.Framework.Modelling
{
    /// <summary>
    /// Represents a selectable list item.
    /// It can be used together with smartstore.selectwrapper.js to create an extended select list.
    /// </summary>
    public class ChoiceListItem
    {
        /// <summary>
        /// The ID value, e.g. an entity ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Displayed text.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// A value indicating whether the item is selected.
        /// </summary>
        [JsonProperty("selected")]
        public bool Selected { get; set; }

        /// <summary>
        /// Optional description.
        /// </summary>
        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// Optional item hint, e.g. the product SKU.
        /// </summary>
        [JsonProperty(PropertyName = "hint", NullValueHandling = NullValueHandling.Ignore)]
        public string Hint { get; set; }

        /// <summary>
        /// URL to add a link to the list item.
        /// </summary>
        [JsonProperty(PropertyName = "url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        /// <summary>
        /// Optional title for the item link.
        /// </summary>
        [JsonProperty(PropertyName = "urlTitle", NullValueHandling = NullValueHandling.Ignore)]
        public string UrlTitle { get; set; }
    }
}
