using System;
using System.Web.Routing;

namespace SmartStore.Web.Framework.UI.Blocks
{
	public interface IBlock
	{
		string SystemName { get; }
		BlockRouteInfo SelectRoute(BlockUiType uiType, string publicTemplateName);
	}

	public enum BlockUiType
	{
		Public,
		Preview,
		Edit
	}

	public class BlockRouteInfo : RouteInfo
	{
		public BlockRouteInfo(string action, string controller, string viewPath, object routeValues)
			: base(action, controller, routeValues)
		{
			Guard.NotEmpty(viewPath, nameof(viewPath));

			ViewPath = viewPath;
		}

		public string ViewPath { get; set; }
	}
}
