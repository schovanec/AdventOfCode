using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Day03
{
    class Program
    {
        const char Tree = '#';

        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var map = ReadMap(file);

            var simpleCount = CountTrees(map, (0, 0), (3, 1));
            Console.WriteLine($"Part 1 Result = {simpleCount}");

            Console.WriteLine();
            Console.WriteLine("Part 2:");
            var directions = new [] { (1, 1), (3, 1), (5, 1), (7, 1), (1, 2) };
            var counts = directions.Select(d => (direction: d, trees: CountTrees(map, (0, 0), d))).ToList();
            foreach (var count in counts)
                Console.WriteLine($"Count For {count.direction}: {count.trees}");

            var product = counts.Aggregate(1L, (p, c) => c.trees * p);
            Console.WriteLine($"Result: {product}");
        }

        private static long CountTrees(Map map, (int x, int y) start, (int dx, int dy) direction)
        {
            var (x, y) = start;
            var count = 0;
            while (y < map.Height)
            {
                if (map.Trees.Contains((x % map.Width, y)))
                    ++count;

                (x, y) = (x + direction.dx, y + direction.dy);
            }

            return count;
        }

        private static Map ReadMap(string file)
        {
            var result = ImmutableHashSet.CreateBuilder<(int x, int y)>();

            int width = 0;
            int y = 0;
            foreach (var line in File.ReadLines(file))
            {
                for (int x = 0; x < line.Length; ++x)
                {
                    if (x >= width)
                        width = x + 1;

                    if (line[x] == Tree)
                        result.Add((x, y));
                }

                ++y;
            }

            return new Map(width, y, result.ToImmutable());
        }

    }

    class Map
    {
        public Map(int width, int height, IImmutableSet<(int x, int y)> trees)
        {
            Width = width;
            Height = height;
            Trees = trees;
        }

        public int Width { get; }

        public int Height { get; }

        public IImmutableSet<(int x, int y)> Trees { get; }
    }
}
