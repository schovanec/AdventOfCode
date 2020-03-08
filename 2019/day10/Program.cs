using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day10
{
    class Program
    {
        const int Digits = 3;

        static void Main(string[] args)
        {
            if (args.Any())
            {
                var points = ParseAsteroidMap(File.ReadLines(args.First()).ToArray()).ToArray();

                // Part 1
                var best = FindBestAsteroid(points);
                Console.WriteLine($"Best is {best.point.x},{best.point.y} with {best.visible} other asteroids detected");
            }
            else
            {
                RunExamples();
            }
        }

        private static ((decimal x, decimal y) point, int visible) FindBestAsteroid(IEnumerable<(decimal x, decimal y)> map)
        {
            // calculate all of the unique directions in which we can see an asteroid
            var directions = from p in map
                             let visible = CountLineOfSightAsteroids(p, map)
                             orderby visible descending
                             select new { Point = p, Visble = visible };

            // return the best one
            return directions.Select(x => (x.Point, x.Visble)).First();
        }

        private static int CountLineOfSightAsteroids((decimal x, decimal y) vantagePoint, IEnumerable<(decimal x, decimal y)> map)
        {
            return map.Where(p => p != vantagePoint)
                .Select(p => CalculateDirection(vantagePoint, p))
                .Distinct()
                .Count();
        }

        private static IEnumerable<(decimal x, decimal y)> ParseAsteroidMap(IEnumerable<string> map)
        {
            Console.WriteLine(map.Count());

            var y = 0;
            foreach (var row in map)
            {
                for (var x = 0; x < row.Length; ++x)
                {
                    if (row[x] == '#')
                        yield return (x, y);
                }

                ++y;
            }
        }

        private static (decimal dx, decimal dy) CalculateDirection((decimal x, decimal y) from, (decimal x, decimal y) to)
        {
            var (dx, dy) = (to.x - from.x, to.y - from.y);
            var length = CalculateLength(dx, dy);
            return (Round(dx / length), Round(dy / length));
        }

        private static decimal CalculateLength(decimal dx, decimal dy)
            => (decimal)Math.Sqrt((double)(dx * dx + dy * dy));

        private static decimal Round(decimal value)
            => Math.Round(value, Digits);

        private static void RunExamples()
        {
            var examples = new[]
            {
                new[] {
                    ".#..#",
                    ".....",
                    "#####",
                    "....#",
                    "...##"
                },
                new[]
                {
                    "......#.#.",
                    "#..#.#....",
                    "..#######.",
                    ".#.#.###..",
                    ".#..#.....",
                    "..#....#.#",
                    "#..#....#.",
                    ".##.#..###",
                    "##...#..#.",
                    ".#....####"
                },
                new[]
                {
                    "#.#...#.#.",
                    ".###....#.",
                    ".#....#...",
                    "##.#.#.#.#",
                    "....#.#.#.",
                    ".##..###.#",
                    "..#...##..",
                    "..##....##",
                    "......#...",
                    ".####.###.",
                },
                new[]
                {
                    ".#..#..###",
                    "####.###.#",
                    "....###.#.",
                    "..###.##.#",
                    "##.##.#.#.",
                    "....###..#",
                    "..#.#..#.#",
                    "#..#.#.###",
                    ".##...##.#",
                    ".....#.#..",
                },
                new[]
                {
                    ".#..##.###...#######",
                    "##.############..##.",
                    ".#.######.########.#",
                    ".###.#######.####.#.",
                    "#####.##.#.##.###.##",
                    "..#####..#.#########",
                    "####################",
                    "#.####....###.#.#.##",
                    "##.#################",
                    "#####.##.###..####..",
                    "..######..##.#######",
                    "####.##.####...##..#",
                    ".#####..#.######.###",
                    "##...#.##########...",
                    "#.##########.#######",
                    ".####.#.###.###.#.##",
                    "....##.##.###..#####",
                    ".#.#.###########.###",
                    "#.#.#.#####.####.###",
                    "###.##.####.##.#..##",
                }
            };

            var count = 0;
            foreach (var example in examples)
            {
                Console.WriteLine($"Example #{++count}...");
                var points = ParseAsteroidMap(example).ToArray();
                var best = FindBestAsteroid(points);
                Console.WriteLine($"Best is {best.point.x},{best.point.y} with {best.visible} other asteroids detected");
                Console.WriteLine();
            }
        }
    }
}
