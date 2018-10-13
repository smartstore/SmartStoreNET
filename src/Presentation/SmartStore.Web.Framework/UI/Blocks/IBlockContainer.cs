using System;

namespace SmartStore.Web.Framework.UI.Blocks
{
	public interface IBlockContainer
	{
		string BlockType { get; }
		IBlock Block { get; }
		IBlockMetadata Metadata { get; }
		//IBlockHandler<T> Handler { get; }
		
		string Title { get; }
		bool IsInversed { get; }

		string HtmlId { get; }
		string CssClass { get; }
		string CssStyle { get; }
	}
}
