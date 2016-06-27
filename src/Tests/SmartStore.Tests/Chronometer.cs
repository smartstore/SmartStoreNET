using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace SmartStore.Tests
{
	public static class Chronometer
	{
		public static void Measure(int cycles, string text, Action<int> action)
		{
			var watch = new Stopwatch();
			watch.Start();
			for (int i = 0; i < cycles; i++)
			{
				action(i);
			}
			watch.Stop();

			Console.WriteLine("{0}: {1} ms.".FormatCurrent(text, watch.ElapsedMilliseconds.ToString()));
		}
	}
}
