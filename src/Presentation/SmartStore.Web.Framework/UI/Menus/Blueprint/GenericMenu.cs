using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Logging;
using SmartStore.Core.Domain.Cms;
using SmartStore.Collections;
using SmartStore.Services;
using SmartStore.Services.Cms;


namespace SmartStore.Web.Framework.UI
{
	/// <summary>
	/// A generic implementation of <see cref="IMenu" /> which represents a
	/// <see cref="MenuRecord"/> entity retrieved by <see cref="IMenuStorage"/>.
	/// </summary>
	public partial class GenericMenu : MenuBase
	{
		private readonly string _name;
		private readonly IMenuStorage _storage;

		public GenericMenu(string name)
		{
			Guard.NotEmpty(name, nameof(name));

			_name = name;

			// TODO: 
		}

		public override string Name
		{
			get { return _name; }
		}

		protected override string GetCacheKey()
		{
			throw new NotImplementedException();
		}

		protected override TreeNode<MenuItem> Build()
		{
			throw new NotImplementedException();
		}
	}
}