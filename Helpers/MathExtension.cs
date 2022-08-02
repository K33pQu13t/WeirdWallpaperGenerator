using System;
using System.Collections.Generic;
using System.Text;

namespace WallpaperGenerator.Helpers
{
    public static class MathExtension
    {
        public static int GCD(int num1, int num2)
        {
            int gcd = 0;
            for (int i = 1; i < (num2 * num1 + 1); i++)
            {
                if (num1 % i == 0 && num2 % i == 0)
                {
                    gcd = i;
                }
            }
            return gcd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number">a number which divisors need to get</param>
        /// <param name="minDivisor">minimal divisor to start with</param>
        /// <param name="maxDivisor">maximum divisor to stop searching other divisors</param>
        /// <returns>collection of divisors of number</returns>
        public static ICollection<int> GetAllDivisors(int number, int minDivisor = 1, int maxDivisor = -1)
        {
            if (minDivisor < 0)
                throw new Exception("divisor can't be less than zero");

            int divisor = minDivisor;
            ICollection<int> divisors = new List<int>();

            for (; divisor * divisor <= number; divisor++)
            {
                if (number % divisor == 0)
                    divisors.Add(divisor);
                if (divisor == maxDivisor)
                    break;
            }

            return divisors;
        }
    }
}
