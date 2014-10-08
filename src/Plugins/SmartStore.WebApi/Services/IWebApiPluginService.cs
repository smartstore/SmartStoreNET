using SmartStore.Core;
using SmartStore.WebApi.Models;
using Telerik.Web.Mvc;

namespace SmartStore.WebApi.Services
{
	public partial interface IWebApiPluginService
	{
		IPagedList<WebApiUserModel> GetUsers(int pageIndex, int pageSize);
		GridModel<WebApiUserModel> GetGridModel(int pageIndex, int pageSize);

		bool CreateKeys(int customerId);
		void RemoveKeys(int customerId);
		void EnableOrDisableUser(int customerId, bool enable);
	}
}
