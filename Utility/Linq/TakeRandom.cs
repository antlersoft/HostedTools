using System;
using System.Collections.Generic;
using System.Linq;

namespace com.antlersoft.HostedTools.Utility.Linq
{
    public static class RandomTaker
    {
        public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> source, int count, Random random)
        {
            var array = source.ToArray();
            return ShuffleInternal(array, Math.Min(count, array.Length), random).Take(count);
        }

        private static IEnumerable<T> ShuffleInternal<T>(T[] array, int count, Random random)
        {
            for (var n = 0; n < count; n++)
            {
                var k = random.Next(n, array.Length);
                var temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }

            return array;
        }
    }
}
