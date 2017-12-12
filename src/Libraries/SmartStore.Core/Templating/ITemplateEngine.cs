using System;
using SmartStore.Core;

namespace SmartStore.Templating
{
	public interface ITestModel
	{
		string ModelName { get; }
	}

	public interface ITemplateEngine
	{
		ITemplate Compile(string template);
		string Render(string source, object data, IFormatProvider formatProvider);

		ITestModel CreateTestModelFor(BaseEntity entity, string modelPrefix);
	}
}
