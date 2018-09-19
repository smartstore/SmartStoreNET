using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Web.Framework.UI.Blocks
{
	public interface IBlockContainer<out T> where T : IBlock
	{
		T Block { get; }

		string TagLine { get; }
		string Title { get; }
		string SubTitle { get; }
		string Body { get; }
		string MediaBody { get; }
		bool IsInversed { get; }
	}
}
