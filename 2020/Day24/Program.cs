using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Day24
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var input = File.ReadLines(file)
                            .Select(ParseSteps);

            var blackTiles = FindBlackTiles(input);
            Console.WriteLine($"Part 1 Result = {blackTiles.Count}");

            var finalBlackTiles = Enumerable.Range(1, 100)
                                            .Aggregate(blackTiles, (t, _) => FlipTiles(t));
            Console.WriteLine($"Part 2 Result = {finalBlackTiles.Count}");
        }

        static ImmutableHashSet<(int x, int y)> FindBlackTiles(IEnumerable<ImmutableList<Step>> input)
        {
            var blackTiles = ImmutableHashSet.CreateBuilder<(int x, int y)>();

            foreach (var item in input)
            {
                var pt = item.Aggregate((0, 0), MovePoint);

                if (blackTiles.Contains(pt))
                    blackTiles.Remove(pt);
                else
                    blackTiles.Add(pt);
            }

            return blackTiles.ToImmutable();
        }

        static ImmutableHashSet<(int x, int y)> FlipTiles(ImmutableHashSet<(int x, int y)> blackTiles)
        {
            var result = ImmutableHashSet.CreateBuilder<(int x, int y)>();
            var whiteTilesToCheck = new HashSet<(int x, int y)>();

            foreach (var pt in blackTiles)
            {
                var adjacentWhiteTiles = FindAdjacentTiles(pt).Where(t => !blackTiles.Contains(t)).ToArray();
                if (adjacentWhiteTiles.Length < 6 && adjacentWhiteTiles.Length >= 4)
                    result.Add(pt);

                foreach (var wpt in adjacentWhiteTiles)
                    whiteTilesToCheck.Add(wpt);
            }

            foreach (var pt in whiteTilesToCheck)
            {
                var adjacentBlackTiles = FindAdjacentTiles(pt).Count(blackTiles.Contains);
                if (adjacentBlackTiles == 2)
                    result.Add(pt);
            }

            return result.ToImmutable();
        }

        static IEnumerable<(int x, int y)> FindAdjacentTiles((int x, int y) pt)
        {
            yield return MovePoint(pt, Step.East);
            yield return MovePoint(pt, Step.NorthEast);
            yield return MovePoint(pt, Step.NorthWest);
            yield return MovePoint(pt, Step.West);
            yield return MovePoint(pt, Step.SouthWest);
            yield return MovePoint(pt, Step.SouthEast);
        }

        static (int x, int y) MovePoint((int x, int y) pt, Step step)
        {
            var even = pt.y % 2 == 0;
            return step switch 
            {
                Step.East => (pt.x - 1, pt.y),
                Step.West => (pt.x + 1, pt.y),

                Step.SouthEast when even => (pt.x, pt.y + 1),
                Step.SouthEast when !even => (pt.x - 1, pt.y + 1),
                Step.SouthWest when even => (pt.x + 1, pt.y + 1),
                Step.SouthWest when !even => (pt.x, pt.y + 1),

                Step.NorthEast when even => (pt.x, pt.y - 1),
                Step.NorthEast when !even => (pt.x - 1, pt.y - 1),
                Step.NorthWest when even => (pt.x + 1, pt.y - 1),
                Step.NorthWest when !even => (pt.x, pt.y - 1),
                
                _ => throw new InvalidOperationException()
            };
        }

        static ImmutableList<Step> ParseSteps(string steps)
        {
            var result = ImmutableList.CreateBuilder<Step>();
            for (var i = 0; i < steps.Length; ++i)
            {
                var direction = (steps[i], i+1 < steps.Length ? steps[i+1] : default) switch
                {
                    ('e', _) => Step.East,
                    ('w', _) => Step.West,
                    ('n', 'e') => Step.NorthEast,
                    ('n', 'w') => Step.NorthWest,
                    ('s', 'e') => Step.SouthEast,
                    ('s', 'w') => Step.SouthWest,
                    _ => throw new FormatException()
                };

                i += direction switch { Step.East or Step.West => 0, _ => 1 };

                result.Add(direction);
            }

            return result.ToImmutable();
        }

        enum Step { East, SouthEast, SouthWest, West, NorthWest, NorthEast }
    }
}
