using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day10
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Any())
            {
                var points = ParseAsteroidMap(File.ReadLines(args.First()).ToArray()).ToArray();

                // Part 1
                var best = FindBestAsteroid(points);
                Console.WriteLine($"Best is {best.point.x},{best.point.y} with {best.visible} other asteroids detected");

                // Part 2
                var vaporizationOrder = from g in GroupByDirection(best.point, points)
                                        from p in SortByDistance(best.point, g).Select((p, i) => new { Point = p, Index = i })
                                        orderby p.Index, g.Key
                                        select p.Point;

                var count = 0;
                foreach (var item in vaporizationOrder)
                    Console.WriteLine($"The #{++count} asteroid to be vaporized is at {item}");

                var desiredPoint = vaporizationOrder.Skip(199).First();
                Console.WriteLine($"The 200th Point is: {desiredPoint}, Answer = {desiredPoint.x * 100 + desiredPoint.y}");
            }
            else
            {
                RunExamples();
            }
        }

        private static ((double x, double y) point, int visible) FindBestAsteroid(IEnumerable<(double x, double y)> map)
            => (from p in map
                let visible = GroupByDirection(p, map).Count()
                orderby visible descending
                select (p, visible)).First();

        private static IEnumerable<IGrouping<double, (double x, double y)>> GroupByDirection(
            (double x, double y) vantagePoint,
            IEnumerable<(double x, double y)> map)
            => from p in map.Where(p => p != vantagePoint)
               let direction = CalculateDirection(vantagePoint, p)
               group p by direction into g
               select g;

        private static IEnumerable<(double x, double y)> SortByDistance(
            (double x, double y) vantagePoint,
            IEnumerable<(double x, double y)> points)
            => from p in points
               orderby CalculateLength(p.x - vantagePoint.x, p.y - vantagePoint.y)
               select p;

        private static IEnumerable<(double x, double y)> ParseAsteroidMap(IEnumerable<string> map)
        {
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

        private static double CalculateDirection((double x, double y) from, (double x, double y) to)
        {
            var (dx, dy) = (to.x - from.x, to.y - from.y);
            var angle = Math.Atan2(dx, -dy);
            while (angle < 0)
                angle += 2 * Math.PI;

            return angle;
        }

        private static double CalculateLength(double dx, double dy)
            => (double)Math.Sqrt((double)(dx * dx + dy * dy));

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
