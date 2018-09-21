using System;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI.Blocks
{
	public interface IBlockHandler
	{
	}

	public interface IBlockHandler<T> : IBlockHandler where T : IBlock
	{
		T Create(IBlockEntity entity);
		T Load(IBlockEntity entity, bool editMode);
		void Save(T block, IBlockEntity entity);

		void Render(IBlockContainer<T> element, string[] templates, HtmlHelper htmlHeper);
		IHtmlString ToHtmlString(IBlockContainer<T> element, string[] templates, HtmlHelper htmlHelper);
	}
}
