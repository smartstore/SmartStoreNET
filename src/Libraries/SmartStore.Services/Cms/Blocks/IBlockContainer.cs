using System;

namespace SmartStore.Services.Cms.Blocks
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
