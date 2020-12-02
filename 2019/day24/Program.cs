using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace day24
{
    class Program
    {

        static void Main(string[] args)
        {
#if false
            var input = new[]
            {
                "....#",
                "#..#.",
                "#..##",
                "..#..",
                "#...."
            };
#else
            var input = new[]
            {
                "##...",
                "#.###",
                ".#.#.",
                "#....",
                "..###"
            };
#endif

            var map = Map.Parse(input);

            var seen = new HashSet<Map>() { };
            var current = map;
            while (!seen.Contains(current))
            {
                seen.Add(current);
                current = NextGeneration(current);
            }

            Console.WriteLine($"Part 1 Result = {current}");
        }

        private static void DumpMap(Map map)
        {
            for (var y = 0; y < Map.Width; ++y)
            {
                for (var x = 0; x < Map.Height; ++x)
                    Console.Write(map.HasBug(x, y) ? '#' : '.');

                Console.WriteLine();
            }

            Console.WriteLine();
        }

        private static Map NextGeneration(Map map)
        {
            var updated = map;

            for (var x = 0; x < Map.Width; ++x)
            {
                for (var y = 0; y < Map.Height; ++y)
                {
                    var adjacent = CountAdjacent(map, x, y);

                    if (map.HasBug(x, y))
                    {
                        if (adjacent != 1)
                            updated = updated.WithoutBugAt(x, y);
                    }
                    else if (adjacent == 1 || adjacent == 2)
                    {
                        updated = updated.WithBugAt(x, y);
                    }
                }
            }

            return updated;
        }

        private static int CountAdjacent(Map map, int x, int y)
        {
            var result = 0;

            if (map.HasBug(x - 1, y))
                ++result;

            if (map.HasBug(x + 1, y))
                ++result;

            if (map.HasBug(x, y - 1))
                ++result;

            if (map.HasBug(x, y + 1))
                ++result;

            return result;
        }

        private struct Map : IEquatable<Map>
        {
            public const int Width = 5;
            public const int Height = 5;

            private readonly long data;

            private Map(long data)
            {
                this.data = data;
            }

            public long Value => data;

            public bool HasBug(int x, int y)
            {
                if (x < 0 || y < 0 || x >= Width || y >= Height)
                    return false;

                var mask = GetMask(x, y);
                return (data & mask) == mask;
            }

            public override string ToString() => $"{data}";

            public Map WithBugAt(int x, int y) => WithUpdatedLocation(x, y, true);

            public Map WithoutBugAt(int x, int y) => WithUpdatedLocation(x, y, false);

            private Map WithUpdatedLocation(int x, int y, bool hasBug)
            {
                if (x < 0 || x >= Width)
                    throw new ArgumentOutOfRangeException(nameof(x));

                if (y < 0 || y >= Height)
                    throw new ArgumentOutOfRangeException(nameof(x));

                var mask = GetMask(x, y);
                if (hasBug)
                    return new Map(data | mask);
                else
                    return new Map(data & ~mask);
            }

            private long GetMask(int x, int y) => 1L << ((y * Width) + x);

            public static Map Parse(string[] input)
            {
                int bit = 0;
                long result = 0;
                foreach (var line in input.Take(Height))
                {
                    for (var x = 0; x < Width; ++x)
                    {
                        if (line[x] == '#')
                            result |= (1L << bit);

                        ++bit;
                    }
                }

                return new Map(result);
            }

            public bool Equals([AllowNull] Map other) => data == other.data;

            public override bool Equals(object obj) => obj is Map other && Equals(other);

            public override int GetHashCode() => data.GetHashCode();
        }
    }
}
