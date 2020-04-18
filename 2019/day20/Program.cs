using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace day20
{
    class Program
    {
        static void Main(string[] args)
        {
            var maze = Maze.Parse(File.ReadAllLines(args.First()));

            var result1 = FindShortestPathLength(maze);
            Console.WriteLine($"Part 1 Result = {result1?.ToString() ?? "not found"}");

            var result2 = FindShortestPathLengthRecursive(maze);
            Console.WriteLine($"Part 2 Result = {result2?.ToString() ?? "not found"}");
        }

        static int? FindShortestPathLength(Maze maze)
            => FindShortestPath(
                start: maze.Start,
                goal: maze.End,
                neighbors: maze.EnumNeighbors
            );

        static int? FindShortestPathLengthRecursive(Maze maze)
            => FindShortestPath(
                start: (maze.Start, 0),
                goal: (maze.End, 0),
                neighbors: maze.EnumNeighbors
            );

        static int? FindShortestPath<T>(T start, T goal, Func<T, IEnumerable<T>> neighbors)
            where T : IEquatable<T>
        {
            var openSet = new HashSet<T> { start };
            var score = new Dictionary<T, int> { { start, 0 } };

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(pt => score.GetValueOrDefault(pt, int.MaxValue))
                                     .First();
                if (goal.Equals(current))
                    return score[current];

                openSet.Remove(current);
                foreach (var neighbor in neighbors(current))
                {
                    var tentative = score[current] + 1;
                    if (tentative < score.GetValueOrDefault(neighbor, int.MaxValue))
                    {
                        score[neighbor] = tentative;
                        openSet.Add(neighbor);
                    }
                }
            }

            return null;
        }
    }

    class Maze
    {
        private int minX;
        private int maxX;
        private int minY;
        private int maxY;

        public Maze(
            ImmutableHashSet<(int x, int y)> openPaths,
            ILookup<(int x, int y), (int x, int y)> portals,
            (int x, int y) start,
            (int x, int y) end)
        {
            OpenPaths = openPaths;
            Portals = portals;
            Start = start;
            End = end;

            minX = openPaths.Select(pt => pt.x).Min();
            maxX = openPaths.Select(pt => pt.x).Max();
            minY = openPaths.Select(pt => pt.y).Min();
            maxY = openPaths.Select(pt => pt.y).Max();
        }

        public (int x, int y) Start { get; }

        public (int x, int y) End { get; }

        public ImmutableHashSet<(int x, int y)> OpenPaths { get; }

        public ILookup<(int x, int y), (int x, int y)> Portals { get; }

        public IEnumerable<(int x, int y)> EnumNeighbors((int x, int y) location)
        {
            foreach (var offset in adjacentNeighborOffsets)
            {
                var pt = (location.x + offset.x, location.y + offset.y);
                if (OpenPaths.Contains(pt))
                    yield return pt;
            }

            foreach (var destination in Portals[location])
                yield return destination;
        }

        public IEnumerable<((int x, int y) pt, int depth)> EnumNeighbors(((int x, int y) pt, int depth) location)
        {
            foreach (var offset in adjacentNeighborOffsets)
            {
                var pt = (location.pt.x + offset.x, location.pt.y + offset.y);
                if (OpenPaths.Contains(pt))
                    yield return (pt, location.depth);
            }

            if (location.depth > 0 || !IsOutsidePoint(location.pt))
            {
                var newDepth = location.depth + (IsOutsidePoint(location.pt) ? -1 : 1);
                Debug.Assert(newDepth >= 0);

                foreach (var destination in Portals[location.pt])
                    yield return (destination, newDepth);
            }
        }

        public bool IsOutsidePoint((int x, int y) location)
            => location.x == minX
            || location.x == maxX
            || location.y == minY
            || location.y == maxY;

        private static readonly ImmutableArray<(int x, int y)> adjacentNeighborOffsets
            = ImmutableArray.Create<(int x, int y)>((-1, 0), (1, 0), (0, -1), (0, 1));

        public static Maze Parse(string[] data)
        {
            var paths = ImmutableHashSet.CreateBuilder<(int x, int y)>();
            var portals = new List<(string name, (int x, int y) loc)>();

            for (var row = 0; row < data.Length; ++row)
            {
                var line = data[row];
                for (var col = 0; col < line.Length; ++col)
                {
                    var pt = (col, row);
                    if (line[col] == '.')
                    {
                        paths.Add(pt);

                        if (row > 1 && IsPortalTile(data[row - 2][col]) && IsPortalTile(data[row - 1][col]))
                            portals.Add((string.Concat(data[row - 2][col], data[row - 1][col]), pt));

                        if (row < data.Length - 2 && IsPortalTile(data[row + 1][col]) && IsPortalTile(data[row + 2][col]))
                            portals.Add((string.Concat(data[row + 1][col], data[row + 2][col]), pt));

                        if (col > 1 && IsPortalTile(data[row][col - 2]) && IsPortalTile(data[row][col - 1]))
                            portals.Add((string.Concat(data[row][col - 2], data[row][col - 1]), pt));

                        if (col < line.Length - 2 && IsPortalTile(data[row][col + 1]) && IsPortalTile(data[row][col + 2]))
                            portals.Add((string.Concat(data[row][col + 1], data[row][col + 2]), pt));
                    }
                }
            }

            var portalMap = portals.Where(p => p.name != "AA" && p.name != "ZZ")
                                   .Join(portals, p1 => p1.name, p2 => p2.name, (p1, p2) => (from: p1.loc, to: p2.loc))
                                   .Where(x => x.from != x.to)
                                   .ToLookup(x => x.from, x => x.to);

            var start = portals.Single(p => p.name == "AA").loc;
            var end = portals.Single(p => p.name == "ZZ").loc;

            return new Maze(
                paths.ToImmutable(),
                portalMap,
                start,
                end);

            bool IsPortalTile(char ch) => ch >= 'A' && ch <= 'Z';
        }
    }
}
