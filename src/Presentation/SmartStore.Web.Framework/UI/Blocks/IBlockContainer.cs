using System;

namespace SmartStore.Web.Framework.UI.Blocks
{
	public interface IBlockContainer<out T> where T : IBlock
	{
		string BlockType { get; }
		T Block { get; }
		IBlockMetadata Metadata { get; }
		//IBlockHandler<T> Handler { get; }
		
		string Title { get; }
		bool IsInversed { get; }

		string HtmlId { get; }
		string CssClass { get; }
		string CssStyle { get; }
	}
}
