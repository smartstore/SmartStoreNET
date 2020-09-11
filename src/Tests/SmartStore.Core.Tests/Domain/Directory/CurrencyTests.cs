using System;
using NUnit.Framework;
using SmartStore.Tests;

namespace SmartStore.Core.Tests.Domain.Directory
{
    [TestFixture]
    public class CurrencyTests
    {
        [TestCase(0.05, 9.225, 9.25)]
        [TestCase(0.05, 9.43, 9.45)]
        [TestCase(0.05, 9.46, 9.45)]
        [TestCase(0.05, 9.48, 9.50)]
        [TestCase(0.1, 9.47, 9.50)]
        [TestCase(0.1, 9.44, 9.40)]
        [TestCase(0.5, 9.24, 9.00)]
        [TestCase(0.5, 9.25, 9.50)]
        [TestCase(0.5, 9.76, 10.00)]
        [TestCase(1.0, 9.49, 9.00)]
        [TestCase(1.0, 9.50, 10.00)]
        [TestCase(1.0, 9.77, 10.00)]
        public void Currency_round_to_nearest(decimal denomination, decimal value, decimal result)
        {
            value.RoundToNearest(denomination, MidpointRounding.AwayFromZero).ShouldEqual(result);
        }

        [TestCase(0.05, 9.225, 9.20, MidpointRounding.ToEven)]
        [TestCase(0.1, 9.45, 9.40, MidpointRounding.ToEven)]
        [TestCase(0.5, 9.25, 9.00, MidpointRounding.ToEven)]
        public void Currency_round_to_nearest(decimal denomination, decimal value, decimal result, MidpointRounding midpoint)
        {
            value.RoundToNearest(denomination, midpoint).ShouldEqual(result);
        }

        [TestCase(0.05, 9.225, 9.20, false)]
        [TestCase(0.05, 9.225, 9.25, true)]
        [TestCase(0.05, 9.24, 9.20, false)]
        [TestCase(0.05, 9.26, 9.30, true)]
        [TestCase(0.1, 9.47, 9.40, false)]
        [TestCase(0.1, 9.47, 9.50, true)]
        [TestCase(0.5, 9.24, 9.00, false)]
        [TestCase(0.5, 9.24, 9.50, true)]
        [TestCase(1.0, 9.77, 9.00, false)]
        [TestCase(1.0, 9.77, 10.00, true)]
        public void Currency_round_to_nearest(decimal denomination, decimal value, decimal result, bool roundUp)
        {
            value.RoundToNearest(denomination, roundUp).ShouldEqual(result);
        }
    }
}
