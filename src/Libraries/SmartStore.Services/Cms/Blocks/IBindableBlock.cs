using System.Collections.Generic;
using Newtonsoft.Json;

namespace SmartStore.Services.Cms.Blocks
{
	/// <summary>
	/// When implemented on <see cref="IBlock"/> types, makes the block bindable to
	/// product, category and manufacturer entities. The UI will display a 'Data binding'
	/// section which allows selection of an entity.
	/// </summary>
	/// <remarks>
	/// The handler for a bindable block type MUST implement <see cref="IBindableBlockHandler"/>.
	/// </remarks>
	public interface IBindableBlock : IBlock
    {
        /// <summary>
        /// The name of the bound entity, e.g. 'product'.
        /// </summary>
        [JsonIgnore]
        string BindEntityName { get; set; }

        /// <summary>
        /// The id of the bound entity.
        /// </summary>
        [JsonIgnore]
        int? BindEntityId { get; set; }

        /// <summary>
        /// Returns a value to indictae whether the block can be bound.
        /// </summary>
        bool CanBind { get; }

        /// <summary>
        /// Returns a value to indictae whether the dataitem is already loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// The data item of the bound entity.
        /// </summary>
        IDictionary<string, object> DataItem { get; set; }

        /// <summary>
        /// Returns a value to indictae whether the block is already bound
        /// </summary>
        bool IsBound { get; set; }

        /// <summary>
        /// Resets the data item of the bound entity
        /// </summary>
        void Reset();
    }

    public abstract class BindableBlockBase : IBindableBlock
    {
        public virtual string BindEntityName { get; set; }
        public virtual int? BindEntityId { get; set; }

        public bool CanBind
        {
            get
            {
                return BindEntityName.HasValue() && BindEntityId.HasValue;
            }
        }

        public bool IsLoaded
        {
            get { return DataItem != null; }
        }

        public IDictionary<string, object> DataItem { get; set; }

        public bool IsBound { get; set; }

        public void Reset()
        {
            DataItem = null;
            IsBound = false;
        }
    }
}
