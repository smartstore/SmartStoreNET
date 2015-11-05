using SmartStore.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace SmartStore.Services.DataExchange.Internal
{
	internal class ExpandoEntity : Expando
	{
		private readonly object _entity;

		public ExpandoEntity(object entity)
			: base(entity)
		{
		}

		public object Entity
		{
			get { return base.WrappedObject; }
		}
	}
}
