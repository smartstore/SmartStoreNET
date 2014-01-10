using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Plugin.Api.WebApi.Models;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Security;
using System.Linq;
using Telerik.Web.Mvc;

namespace SmartStore.Plugin.Api.WebApi.Services
{
	public class WebApiPluginService : IWebApiPluginService
	{
		private readonly IRepository<GenericAttribute> _genericAttributes;
		private readonly IRepository<Customer> _customers;
		private readonly ICustomerService _customerService;
		private readonly IGenericAttributeService _genericAttributeService;

		public WebApiPluginService(
			IRepository<GenericAttribute> genericAttributes,
			IRepository<Customer> customers,
			ICustomerService customerService,
			IGenericAttributeService genericAttributeService)
		{
			_genericAttributes = genericAttributes;
			_customers = customers;
			_customerService = customerService;
			_genericAttributeService = genericAttributeService;
		}

		public IPagedList<WebApiUserModel> GetUsers(int pageIndex, int pageSize)
		{
			var registeredRoleId = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Id;

			var query =
				from c in _customers.Table
				join a in
					(
						from a in _genericAttributes.Table
						where a.KeyGroup == "Customer" && a.Key == WebApiUserCacheData.Key
						select a
					)
				on c.Id equals a.EntityId into ga
				from a in ga.DefaultIfEmpty()
				where !c.Deleted && c.CustomerRoles.Select(r => r.Id).Contains(registeredRoleId)
				orderby a.Value descending
				select new WebApiUserModel
				{
					Id = c.Id,
					Username = c.Username,
					Email = c.Email,
					AdminComment = c.AdminComment
				};

			var lst = new PagedList<WebApiUserModel>(query, pageIndex, pageSize);

			var cacheData = WebApiCaching.UserData();

			foreach (var itm in lst)
			{
				var cacheItem = cacheData.FirstOrDefault(x => x.CustomerId == itm.Id);
				if (cacheItem != null)
				{
					itm.PublicKey = cacheItem.PublicKey;
					itm.SecretKey = cacheItem.SecretKey;
					itm.Enabled = cacheItem.Enabled;
					if (cacheItem.LastRequest.HasValue)
						itm.LastRequest = cacheItem.LastRequest.ToLocalTime();
					else
						itm.LastRequest = null;
				}
			}

			return lst;
		}
		public GridModel<WebApiUserModel> GetGridModel(int pageIndex, int pageSize)
		{
			var apiUsers = GetUsers(pageIndex, pageSize);

			var model = new GridModel<WebApiUserModel>()
			{
				Data = apiUsers,
				Total = apiUsers.TotalCount
			};

			return model;
		}

		public bool CreateKeys(int customerId)
		{
			if (customerId != 0)
			{
				var hmac = new HmacAuthentication();
				var userData = WebApiCaching.UserData();
				string key1, key2;

				for (int i = 0; i < 9999; ++i)
				{
					if (hmac.CreateKeys(out key1, out key2) && !userData.Exists(x => x.PublicKey.IsCaseInsensitiveEqual(key1)))
					{
						var apiUser = new WebApiUserCacheData()
						{
							CustomerId = customerId,
							PublicKey = key1,
							SecretKey = key2,
							Enabled = true
						};

						RemoveKeys(customerId);

						var attribute = new GenericAttribute()
						{
							EntityId = customerId,
							KeyGroup = "Customer",
							Key = WebApiUserCacheData.Key,
							Value = apiUser.ToString()
						};

						_genericAttributeService.InsertAttribute(attribute);

						WebApiCaching.Remove(WebApiUserCacheData.Key);
						return true;
					}
				}
			}
			return false;
		}
		public void RemoveKeys(int customerId)
		{
			if (customerId != 0)
			{
				var data = (
					from a in _genericAttributes.Table
					where a.EntityId == customerId && a.KeyGroup == "Customer" && a.Key == WebApiUserCacheData.Key
					select a).ToList();

				if (data.Count > 0)
				{
					foreach (var itm in data)
						_genericAttributeService.DeleteAttribute(itm);

					WebApiCaching.Remove(WebApiUserCacheData.Key);
				}
			}
		}
		public void EnableOrDisableUser(int customerId, bool enable)
		{
			if (customerId != 0)
			{
				var cacheData = WebApiCaching.UserData();
				var apiUser = cacheData.FirstOrDefault(x => x.CustomerId == customerId);

				if (apiUser != null)
				{
					apiUser.Enabled = enable;

					var attribute = _genericAttributeService.GetAttributeById(apiUser.GenericAttributeId);
					if (attribute != null)
					{
						attribute.Value = apiUser.ToString();
						_genericAttributeService.UpdateAttribute(attribute);
					}
				}
			}
		}
	}
}
