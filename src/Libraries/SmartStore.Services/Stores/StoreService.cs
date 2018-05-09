﻿using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Data.Caching;

namespace SmartStore.Services.Stores
{
	public partial class StoreService : IStoreService
	{
		private readonly IRepository<Store> _storeRepository;
		private readonly IEventPublisher _eventPublisher;
		private bool? _isSingleStoreMode = null;

		public StoreService(
			IRepository<Store> storeRepository,
			IEventPublisher eventPublisher)
		{
			this._storeRepository = storeRepository;
			this._eventPublisher = eventPublisher;
		}

		public virtual void DeleteStore(Store store)
		{
			if (store == null)
				throw new ArgumentNullException("store");

			var allStores = GetAllStores();
			if (allStores.Count == 1)
				throw new Exception("You cannot delete the only configured store.");

			_storeRepository.Delete(store);

			//event notification
			_eventPublisher.EntityDeleted(store);
		}

		public virtual IList<Store> GetAllStores()
		{
			var query = _storeRepository.Table
				.Expand(x => x.PrimaryStoreCurrency)
				.Expand(x => x.PrimaryExchangeRateCurrency)
				.OrderBy(x => x.DisplayOrder)
				.ThenBy(x => x.Name);

			var stores = query.ToListCached("db.store.all");
			return stores;
		}

		public virtual Store GetStoreById(int storeId)
		{
			if (storeId == 0)
				return null;

			return _storeRepository.GetByIdCached(storeId, "db.store.id-" + storeId);
		}

		public virtual void InsertStore(Store store)
		{
			if (store == null)
				throw new ArgumentNullException("store");

			_storeRepository.Insert(store);

			//event notification
			_eventPublisher.EntityInserted(store);
		}

		public virtual void UpdateStore(Store store)
		{
			if (store == null)
				throw new ArgumentNullException("store");

			_storeRepository.Update(store);

			//event notification
			_eventPublisher.EntityUpdated(store);
		}

		public virtual bool IsSingleStoreMode()
		{
			if (!_isSingleStoreMode.HasValue)
			{
				_isSingleStoreMode = (_storeRepository.TableUntracked.Count() <= 1);
			}

			return _isSingleStoreMode.Value;
		}

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
	}
}