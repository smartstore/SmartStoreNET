﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.UI
{
	public interface IMenuPublisher
	{
		void RegisterMenus(TreeNode<MenuItem> rootNode, string menuName);
	}

	public class MenuPublisher : IMenuPublisher
	{
		private readonly ITypeFinder _typeFinder;
		private readonly IRequestCache _requestCache;

		public MenuPublisher(ITypeFinder typeFinder, IRequestCache requestCache)
		{
			this._typeFinder = typeFinder;
			this._requestCache = requestCache;
		}

		public void RegisterMenus(TreeNode<MenuItem> rootNode, string menuName)
		{
			Guard.ArgumentNotNull(() => rootNode);
			Guard.ArgumentNotEmpty(() => menuName);

			var providers = _requestCache.Get("sm.menu.providers.{0}".FormatInvariant(menuName), () => 
			{
				var allInstances = _requestCache.Get("sm.menu.allproviders", () =>
				{
					var instances = new List<IMenuProvider>();
					var providerTypes = _typeFinder.FindClassesOfType<IMenuProvider>(ignoreInactivePlugins: true);

					foreach (var type in providerTypes)
					{
						try
						{
							var provider = Activator.CreateInstance(type) as IMenuProvider;
							instances.Add(provider);
						}
						catch { }
					}

					return instances;
				});
				
				return allInstances.Where(x => x.MenuName.IsCaseInsensitiveEqual(menuName)).OrderBy(x => x.Ordinal).ToList();
			});

			providers.Each(x => x.BuildMenu(rootNode));
		}

	}
}
