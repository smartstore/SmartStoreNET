using System;
using System.Diagnostics;

namespace SmartStore.Tests
{
    public static class Chronometer
    {
        public static void Measure(int cycles, string text, Action<int> action)
        {
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < cycles; i++)
            {
                action(i);
            }
            watch.Stop();

            Console.WriteLine("{0}: {1} ms.".FormatCurrent(text, watch.ElapsedMilliseconds.ToString()));
        }
    }
}
