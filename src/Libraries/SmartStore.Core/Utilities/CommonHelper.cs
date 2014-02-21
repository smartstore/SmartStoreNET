using System;
using System.Security.Cryptography;

namespace SmartStore.Utilities
{

    public partial class CommonHelper
    {
        /// <summary>
        /// Generate random digit code
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Result string</returns>
        public static string GenerateRandomDigitCode(int length)
        {
            var random = new Random();
            string str = string.Empty;
            for (int i = 0; i < length; i++)
                str = String.Concat(str, random.Next(10).ToString());
            return str;
        }

		/// <summary>
		/// Returns an random interger number within a specified rage
		/// </summary>
		/// <param name="min">Minimum number</param>
		/// <param name="max">Maximum number</param>
		/// <returns>Result</returns>
		public static int GenerateRandomInteger(int min = 0, int max = 2147483647)
		{
			var randomNumberBuffer = new byte[10];
			new RNGCryptoServiceProvider().GetBytes(randomNumberBuffer);
			return new Random(BitConverter.ToInt32(randomNumberBuffer, 0)).Next(min, max);
		}
    }
}
