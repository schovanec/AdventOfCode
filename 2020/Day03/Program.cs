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

            var simpleCount = CountTrees(map, 3, 1);
            Console.WriteLine($"Part 1 Result = {simpleCount}");

            Console.WriteLine();
            Console.WriteLine("Part 2:");
            var directions = new (int dx, int dy)[] { (1, 1), (3, 1), (5, 1), (7, 1), (1, 2) };
            var counts = directions.ToDictionary(x => x, d => CountTrees(map, d.dx, d.dy));
            foreach (var item in counts)
                Console.WriteLine($"Count For {item.Key}: {item.Value}");

            var product = counts.Values.Aggregate((a, b) => a * b);
            Console.WriteLine($"Result: {product}");
        }

        private static long CountTrees(Map map, int deltaX, int deltaY, int startX = 0, int startY = 0)
        {
            var (x, y) = (startX, startY);
            var count = 0;
            while (y < map.Height)
            {
                if (map.Trees.Contains((x % map.Width, y)))
                    ++count;

                (x, y) = (x + deltaX, y + deltaY);
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
