using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI.Blocks
{
	public enum StoryViewMode
	{
		Public,
		Preview,
		GridEdit,
		Edit
	}

	public interface IBlockHandler
	{
		void Render(IBlockContainer element, IEnumerable<string> templates, HtmlHelper htmlHeper);
		IHtmlString ToHtmlString(IBlockContainer element, IEnumerable<string> templates, HtmlHelper htmlHelper);
	}

	public interface IBlockHandler<T> : IBlockHandler where T : IBlock
	{
		T Create(IBlockEntity entity);

		T Load(IBlockEntity entity, StoryViewMode viewMode);

		void Save(T block, IBlockEntity entity);

		string Clone(IBlockEntity sourceEntity, IBlockEntity clonedEntity);
	}
}
