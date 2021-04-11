using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day06
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = File.ReadLines(args.DefaultIfEmpty("input.txt").First())
                            .Select(Point.Parse)
                            .ToList();

            Part1(input);
            Part2(input);
        }

        private static void Part1(IReadOnlyList<Point> points)
        {
            var width = points.Max(p => p.X) + 1;
            var height = points.Max(p => p.Y) + 1;

            var areas = points.ToDictionary(p => p, _ => 0);
            var infinite = new HashSet<Point>();

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    var closest = (from p in points
                                   let d = p.ManhattanDistance(x, y)
                                   group p by d into g
                                   orderby g.Key
                                   select g).First();

                    if (closest.Count() == 1)
                    {
                        var p = closest.Single();
                        areas[p]++;

                        if (x == 0 || y == 0 || x == (width - 1) || y == (height - 1))
                            infinite.Add(p);
                    }
                }
            }

            var largest = (from a in areas
                           where !infinite.Contains(a.Key)
                           orderby a.Value descending
                           select a).First();

            Console.WriteLine($"Part 1 Result = {largest.Value}");
        }

        private static void Part2(IReadOnlyList<Point> points, int limit = 10000)
        {
            var width = points.Max(p => p.X) + 1;
            var height = points.Max(p => p.Y) + 1;

            var regionSize = 0;

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    var total = points.Sum(p => p.ManhattanDistance(x, y));
                    if (total < limit)
                        ++regionSize;
                }
            }

            Console.WriteLine($"Part 2 Result = {regionSize}");
        }

        private record Point(int X, int Y)
        {
            public int ManhattanDistance(int x, int y)
                => Math.Abs(X - x) + Math.Abs(Y - y);

            public static Point Parse(string text)
            {
                var parts = text.Split(',', 2, StringSplitOptions.TrimEntries);
                return new Point(int.Parse(parts[0]), int.Parse(parts[1]));
            }
        }
    }
}
