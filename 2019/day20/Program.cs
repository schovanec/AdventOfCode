using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace day20
{
    class Program
    {
        static void Main(string[] args)
        {
            var maze = Maze.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "sample1.txt"));

            var result1 = FindShortestPathLength(maze);
            Console.WriteLine($"Part 1 Result = {result1}");
        }

        static int FindShortestPathLength(Maze maze)
        {
            var openSet = new HashSet<(int x, int y)> { maze.Start };
            var cameFrom = new Dictionary<(int x, int y), (int x, int y)>();
            var gScore = new Dictionary<(int x, int y), int> { { maze.Start, 0 } };
            var fScore = new Dictionary<(int x, int y), int> { { maze.Start, Heuristic(maze.Start) } };

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(pt => fScore.GetValueOrDefault(pt, int.MaxValue))
                                     .ThenBy(pt => pt == maze.End ? 1 : 0)
                                     .First();
                if (current == maze.End)
                    return gScore[current];

                openSet.Remove(current);
                foreach (var neighbor in maze.EnumNeighbors(current))
                {
                    var tentative = gScore[current] + 1;
                    if (tentative < gScore.GetValueOrDefault(neighbor, int.MaxValue))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentative;
                        fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor);
                        openSet.Add(neighbor);
                    }
                }
            }

            return int.MaxValue;

#if true
            int Heuristic((int x, int y) pt) => 0;
#else
            int Heuristic((int x, int y) pt)
                => ((maze.End.x - pt.x) * (maze.End.x - pt.x)) + ((maze.End.y - pt.y) * (maze.End.y - pt.y));
#endif
        }
    }

    class Maze
    {
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
