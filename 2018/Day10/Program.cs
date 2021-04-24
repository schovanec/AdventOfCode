using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day10
{
    class Program
    {
        static void Main(string[] args)
        {
            var points = File.ReadLines(args.DefaultIfEmpty("input.txt").First())
                             .Select(Point.Parse)
                             .ToImmutableList();

            Simulate(points);
        }

        private static void Simulate(ImmutableList<Point> input)
        {
            var image = input.Select(x => x.Position).ToImmutableList();
            var volume = GetVolume(image);
            var count = 0L;
            while (true)
            {
                var newImage = image.Select((p, i) => (p.x + input[i].Velocity.dx, p.y + input[i].Velocity.dy))
                                    .ToImmutableList();
                var newVolume = GetVolume(newImage);
                if (newVolume > volume)
                    break;

                image = newImage;
                volume = newVolume;
                ++count;
            }

            PrintImage(image);
            Console.WriteLine();
            Console.WriteLine($"Time = {count} seconds");
        }

        private static long GetVolume(IEnumerable<(long x, long y)> points)
        {
            var (minX, minY, maxX, maxY) = GetBounds(points);
            return Math.Abs(maxX - minX + 1) * Math.Abs(maxY - minY + 1);
        }

        private static (long minX, long minY, long maxX, long maxY) GetBounds(IEnumerable<(long x, long y)> points)
            => points.Aggregate(points.Select(p => (minX: p.x, minY: p.y, maxX: p.x, maxY: p.y)).First(),
                                (a, p) => (Math.Min(a.minX, p.x), Math.Min(a.minY, p.y), Math.Max(a.maxX, p.x), Math.Max(a.maxY, p.y)));

        private static void PrintImage(IImmutableList<(long x, long y)> points)
        {
            var (minX, minY, maxX, maxY) = GetBounds(points);

            for (long y = minY; y <= maxY; ++y)
            {
                for (long x = minX; x <= maxX; ++x)
                    Console.Write(points.Contains((x, y)) ? '#' : '.');

                Console.WriteLine();
            }
        }

        record Point((long x, long y) Position, (long dx, long dy) Velocity)
        {
            public static Point Parse(string input)
            {
                var m = Regex.Match(input, @"^position=<\s*(?<x>-?\d+),\s*(?<y>-?\d+)> velocity=<\s*(?<dx>-?\d+),\s*(?<dy>-?\d+)>$");
                if (!m.Success)
                {
                    Console.WriteLine(input);
                    throw new FormatException();
                }

                return new Point(
                    (long.Parse(m.Groups["x"].Value), long.Parse(m.Groups["y"].Value)),
                    (long.Parse(m.Groups["dx"].Value), long.Parse(m.Groups["dy"].Value)));
            }
        }
    }
}
