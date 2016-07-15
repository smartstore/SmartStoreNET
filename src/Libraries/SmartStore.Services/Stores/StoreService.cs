using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Data.Caching2;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Stores
{
	/// <summary>
	/// Store service
	/// </summary>
	public partial class StoreService : IStoreService
	{
		#region Constants
		private const string STORES_ALL_KEY = "SmartStore.stores.all";
		private const string STORES_PATTERN_KEY = "SmartStore.stores.";
        private const string STORES_BY_ID_KEY = "SmartStore.stores.id-{0}";
		#endregion

		#region Fields

		private readonly IRepository<Store> _storeRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly IRequestCache _requestCache;
		private bool? _isSingleStoreMode = null;

		#endregion

		#region Ctor

		public StoreService(
			IRequestCache requestCache,
			IRepository<Store> storeRepository,
			IEventPublisher eventPublisher)
		{
			this._requestCache = requestCache;
			this._storeRepository = storeRepository;
			this._eventPublisher = eventPublisher;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Deletes a store
		/// </summary>
		/// <param name="store">Store</param>
		public virtual void DeleteStore(Store store)
		{
			if (store == null)
				throw new ArgumentNullException("store");

			var allStores = GetAllStores();
			if (allStores.Count == 1)
				throw new Exception("You cannot delete the only configured store.");

			_storeRepository.Delete(store);

			_requestCache.RemoveByPattern(STORES_PATTERN_KEY);

			//event notification
			_eventPublisher.EntityDeleted(store);
		}

		/// <summary>
		/// Gets all stores
		/// </summary>
		/// <returns>Store collection</returns>
		public virtual IList<Store> GetAllStores()
		{
			string key = STORES_ALL_KEY;
			return _requestCache.Get(key, () =>
			{
				var query = _storeRepository.Table
					.Expand(x => x.PrimaryStoreCurrency)
					.Expand(x => x.PrimaryExchangeRateCurrency)
					.OrderBy(x => x.DisplayOrder)
					.ThenBy(x => x.Name)
					.FromCache();

				var stores = query.ToList();
				return stores;
			});
		}

		/// <summary>
		/// Gets a store 
		/// </summary>
		/// <param name="storeId">Store identifier</param>
		/// <returns>Store</returns>
		public virtual Store GetStoreById(int storeId)
		{
			if (storeId == 0)
				return null;

            string key = string.Format(STORES_BY_ID_KEY, storeId);
            return _requestCache.Get(key, () => 
            { 
                return _storeRepository.GetById(storeId); 
            });
		}

		/// <summary>
		/// Inserts a store
		/// </summary>
		/// <param name="store">Store</param>
		public virtual void InsertStore(Store store)
		{
			if (store == null)
				throw new ArgumentNullException("store");

			_storeRepository.Insert(store);

			_requestCache.RemoveByPattern(STORES_PATTERN_KEY);

			//event notification
			_eventPublisher.EntityInserted(store);
		}

		/// <summary>
		/// Updates the store
		/// </summary>
		/// <param name="store">Store</param>
		public virtual void UpdateStore(Store store)
		{
			if (store == null)
				throw new ArgumentNullException("store");

			_storeRepository.Update(store);

			_requestCache.RemoveByPattern(STORES_PATTERN_KEY);

			//event notification
			_eventPublisher.EntityUpdated(store);
		}

		/// <summary>
		/// True if there's only one store. Otherwise False.
		/// </summary>
		public virtual bool IsSingleStoreMode()
		{
			if (!_isSingleStoreMode.HasValue)
			{
				_isSingleStoreMode = (_storeRepository.TableUntracked.Count() <= 1);
			}

			return _isSingleStoreMode.Value;
		}

		/// <summary>
		/// True if the store data is valid. Otherwise False.
		/// </summary>
		/// <param name="store">Store entity</param>
		public virtual bool IsStoreDataValid(Store store)
		{
			if (store == null)
				throw new ArgumentNullException("store");

			if (store.Url.IsEmpty())
				return false;

			try
			{
				var uri = new Uri(store.Url);
				var domain = uri.DnsSafeHost.EmptyNull().ToLower();

				switch (domain)
				{
					case "www.yourstore.com":
					case "yourstore.com":
					case "www.mystore.com":
					case "mystore.com":
					case "www.mein-shop.de":
					case "mein-shop.de":
						return false;
					default:
						return store.Url.IsWebUrl();
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		#endregion
	}
}