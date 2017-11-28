using System;
using System.Collections.Generic;

namespace SmartStore.Templating
{
	public interface ITemplateManager
	{
		IReadOnlyDictionary<string, ITemplate> All();

		bool Contains(string name);

		ITemplate Get(string name);

		void Put(string name, ITemplate template);

		ITemplate GetOrAdd(string name, Func<string> template);

		bool TryRemove(string name, out ITemplate template);

		void Clear();
	}
}
