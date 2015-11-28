using System;
using SmartStore.Core.Infrastructure;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Core.Tests
{
    public abstract class TypeFindingBase : TestsBase
    {
        protected ITypeFinder typeFinder;

        protected abstract Type[] GetTypes();

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeFinder = new Fakes.FakeTypeFinder(typeof(TypeFindingBase).Assembly, GetTypes());
        }
    }
}
