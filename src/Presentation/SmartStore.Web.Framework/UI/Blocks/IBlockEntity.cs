using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Web.Framework.UI.Blocks
{
	public interface IBlockEntity
	{
		int Id { get; set; }
		string BlockType { get; set; }
		string Model { get; set; }
		string TagLine { get; set; }
		string Title { get; set; }
		string SubTitle { get; set; }
		string Body { get; set; }
		string Template { get; set; }
	}
}
