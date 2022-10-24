using System;
using System.Linq;
using System.Collections.Generic;

namespace ThunderWire.Helpers
{
    public class RandomHelper
    {
        private int _old;

        /// <summary>
        /// Function to generate List of random integer numbers.
        /// </summary>
        public static List<int> RandomList(int min, int max, int count)
        {
            return Enumerable.Range(min, max).OrderBy(x => Guid.NewGuid()).Take(count).ToList();
        }

        /// <summary>
        /// Function to generate random integer number (No Duplicates).
        /// </summary>
        public int Range(int min, int max)
        {
            if (min == max)
                return min;

            Random rnd = new Random();
            return _old = Enumerable.Range(min, max).OrderBy(x => rnd.Next()).Where(x => x != _old).Take(1).Single();
        }
    }
}
