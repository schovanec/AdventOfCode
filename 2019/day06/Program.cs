using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day06
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = args.Any()
                ? File.ReadLines(args.First()).Where(x => !string.IsNullOrEmpty(x))
                : TestInput;

            var orbits = input.Select(x => x.ToUpper().Split(')'))
                              .Where(x => x.Length == 2)
                              .Select(x => (inner: x[0], outer: x[1]))
                              .Distinct()
                              .ToLookup(x => x.inner, x => x.outer);

            foreach (var group in orbits)
            {
                foreach (var item in group)
                {
                    Console.WriteLine($"{group.Key} ) {item}");
                }
            }

            var totalOrbits = CountTotalOrbits(orbits);
            Console.WriteLine(totalOrbits);
        }

#if true
        private static int CountTotalOrbits(ILookup<string, string> orbits, string node = "COM", int depth = 0)
            => depth + orbits[node].Sum(x => CountTotalOrbits(orbits, x, depth + 1));
#else
        private static int CountTotalOrbits(ILookup<string, string> orbits, string node = "COM", int depth = 0)
        {
            var result = depth;

            foreach (var item in orbits[node])
            {
                result += CountTotalOrbits(orbits, item, depth + 1);
            }

            return result;
        }
#endif

        private static IEnumerable<string> TestInput =>
            new[]
            {
                "COM)B",
                "B)C",
                "C)D",
                "D)E",
                "E)F",
                "B)G",
                "G)H",
                "D)I",
                "E)J",
                "J)K",
                "K)L"
            };
    }
}
