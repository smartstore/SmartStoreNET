using System;
using SmartStore.Core;

namespace SmartStore.Templating
{
    public class NullTemplateEngine : ITemplateEngine
    {
        private readonly static ITemplateEngine _instance = new NullTemplateEngine();

        public static ITemplateEngine Instance => _instance;

        public ITemplate Compile(string template)
        {
            return new NullTemplate(template);
        }

        public string Render(string source, object data, IFormatProvider formatProvider)
        {
            return source;
        }

        public ITestModel CreateTestModelFor(BaseEntity entity, string modelPrefix)
        {
            return new NullTestModel();
        }

        internal class NullTestModel : ITestModel
        {
            public string ModelName => "TestModel";
        }

        internal class NullTemplate : ITemplate
        {
            private readonly string _source;

            public NullTemplate(string source)
            {
                _source = source;
            }

            public string Source => _source;

            public string Render(object data, IFormatProvider formatProvider)
            {
                return _source;
            }
        }
    }
}
