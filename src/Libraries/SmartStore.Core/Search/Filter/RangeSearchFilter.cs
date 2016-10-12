using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Search
{
	public class RangeSearchFilter : SearchFilter, IRangeSearchFilter
	{
		public object UpperTerm
		{
			get;
			protected internal set;
		}

		public bool IncludesLower
		{
			get;
			protected internal set;
		}

		public bool IncludesUpper
		{
			get;
			protected internal set;
		}
	}
}
