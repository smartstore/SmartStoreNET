using NUnit.Framework;
using SmartStore.Core.Domain.Directory;
using SmartStore.Tests;

namespace SmartStore.Core.Tests.Domain.Directory
{
	[TestFixture]
    public class CurrencyTests
    {
		//[TestCase(CurrencyRoundingMethod.Default, 9.4548, 9.45)]
		//[TestCase(CurrencyRoundingMethod.Default, 9.4568, 9.46)]
		//[TestCase(CurrencyRoundingMethod.Down005, 9.4368, 9.40)]
		//[TestCase(CurrencyRoundingMethod.Down005, 9.4468, 9.45)]
		//[TestCase(CurrencyRoundingMethod.Down005, 9.4668, 9.45)]
		//[TestCase(CurrencyRoundingMethod.Up005, 9.0011, 9.00)]
		//[TestCase(CurrencyRoundingMethod.Up005, 9.4468, 9.50)]
		//[TestCase(CurrencyRoundingMethod.Up005, 9.4668, 9.50)]
		//[TestCase(CurrencyRoundingMethod.Down01, 9.4568, 9.50)]
		//[TestCase(CurrencyRoundingMethod.Down01, 9.5568, 9.60)]
		//[TestCase(CurrencyRoundingMethod.Up01, 9.0011, 9.00)]
		//[TestCase(CurrencyRoundingMethod.Up01, 9.5568, 9.60)]
		//[TestCase(CurrencyRoundingMethod.Interval05, 9.2011, 9.00)]
		//[TestCase(CurrencyRoundingMethod.Interval05, 9.4468, 9.50)]
		//[TestCase(CurrencyRoundingMethod.Interval05, 9.7368, 9.50)]
		//[TestCase(CurrencyRoundingMethod.Interval05, 9.9968, 10.00)]
		//[TestCase(CurrencyRoundingMethod.Interval1, 9.4668, 9.00)]
		//[TestCase(CurrencyRoundingMethod.Interval1, 9.5568, 10.00)]
		//[TestCase(CurrencyRoundingMethod.Up1, 9.0011, 9.00)]
		//[TestCase(CurrencyRoundingMethod.Up1, 9.0068, 10.00)]
		public void Currency_can_round(CurrencyRoundingMethod method, decimal value, decimal result)
        {
            value.Round(method).ShouldEqual(result);
		}
	}
}
