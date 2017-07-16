using System;
using System.Collections.Generic;
using System.Web.Mvc;
using AutoMapper;
using Newtonsoft.Json;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Modelling
{
	public sealed class CustomPropertiesDictionary : Dictionary<string, object>
	{
	}

	[Serializable]
	public abstract partial class ModelBase
    {
		private readonly static ContextState<Dictionary<ModelBase, IDictionary<string, object>>> _contextState;

		static ModelBase()
		{
			_contextState = new ContextState<Dictionary<ModelBase, IDictionary<string, object>>>("ModelBase.CustomThreadProperties");
		}

		protected ModelBase()
        {
			CustomProperties = new CustomPropertiesDictionary();
        }
        
        public virtual void BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
        }

		/// <summary>
		/// Gets a custom property value either from the thread local or the static storage (in this particular order)
		/// </summary>
		/// <typeparam name="TProperty">Type of property</typeparam>
		/// <param name="key">Custom property key</param>
		/// <returns>The property value or null</returns>
		public TProperty Get<TProperty>(string key)
		{
			Guard.NotEmpty(key, nameof(key));

			IDictionary<string, object> dict;
			object value;

			if (TryGetCustomThreadProperties(false, out dict) && dict.TryGetValue(key, out value))
			{
				return (TProperty)value;
			}

			if (CustomProperties.TryGetValue(key, out value))
			{
				return (TProperty)value;
			}

			return default(TProperty);
		}

        /// <summary>
        /// Use this property to store any custom value for your models. 
        /// </summary>
		[IgnoreMap]
		public CustomPropertiesDictionary CustomProperties { get; set; }

		/// <summary>
		/// A data bag for custom model properties which only
		/// lives during a thread/request lifecycle
		/// </summary>
		/// <remarks>
		/// Use thread properties whenever you need to persist request-scoped data,
		/// but the model is potentially cached statically.
		/// </remarks>
		[IgnoreMap, JsonIgnore]
		public IDictionary<string, object> CustomThreadProperties
		{
			get
			{
				IDictionary<string, object> dict;
				TryGetCustomThreadProperties(true, out dict);
				return dict;
			}
		}

		private bool TryGetCustomThreadProperties(bool create, out IDictionary<string, object> dict)
		{
			dict = null;
			var state = _contextState.GetState();

			if (state == null && create)
			{
				state = new Dictionary<ModelBase, IDictionary<string, object>>();
				_contextState.SetState(state);
			}

			if (state != null)
			{
				if (!state.TryGetValue(this, out dict))
				{
					if (create)
					{
						dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
						state[this] = dict;
					}
				}

				return dict != null;
			}

			return false;
		}
	}


    public abstract partial class EntityModelBase : ModelBase
    {
        [SmartResourceDisplayName("Admin.Common.Entity.Fields.Id")]
        public virtual int Id { get; set; }
    }


	public abstract partial class TabbableModel : EntityModelBase
	{
		[IgnoreMap]
		public virtual string[] LoadedTabs { get; set; }
	}

}
