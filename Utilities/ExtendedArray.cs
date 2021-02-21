using System;
using System.Collections.Generic;
using System.Text;

namespace BenchmarkLogGenerator.Utilities
{
    public static class ExtendedArray
    {
        public static T[] SlowRemoveByIndex<T>(this T[] array, int index)
        {
            if (array == null || array.Length == 0 || index < 0 || index >= array.Length)
            {
                // Nothing really to do
                return array;
            }

            if (array.Length == 1 && index == 0)
            {
                // Just removed the last element
                return new T[0];
            }

            var ret = new T[array.Length - 1];

            // We could use two Array.Copy calls, but this is simpler
            // (and easier to prove is right):
            for (int i = 0, j = 0; i < array.Length; i++)
            {
                if (i != index)
                {
                    ret[j] = array[i];
                    j++;
                }
            }
            return ret;
        }
    }
}
