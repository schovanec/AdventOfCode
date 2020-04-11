using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace day18
{
    class Program
    {
        private const char StartNode = '@';
        private const char WallNode = '#';
        private const char OpenNode = '.';

        static void Main(string[] args)
        {
#if true
            var map = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt");
#else
            var map = new string[]
            {
                "#############",
                "#g#f.D#..h#l#",
                "#F###e#E###.#",
                "#dCba...BcIJ#",
                "#####.@.#####",
                "#nK.L...G...#",
                "#M###N#H###.#",
                "#o#m..#i#jk.#",
                "#############"
            };
#endif
            DoPart1(map);

            DoPart2(map);
        }

        private static void DoPart1(string[] map)
        {
            Console.WriteLine("Building Graph...");
            var graph = BuildGraph(map);

            Console.WriteLine($"Graph Completed - {graph.Nodes.Length} nodes and {graph.Edges.Count} edges");

            var start = graph.Nodes.Where(g => g.Value.Length == 0).Single();
            var minPathLength = FindShortestPathLength(graph, start);
            Console.WriteLine($"Part 1 Result = {minPathLength}");
        }

        private static string[] replacement = new[]
        {
            "@#@",
            "###",
            "@#@"
        };

        private static void DoPart2(string[] map)
        {
            ReplaceMapStart(map);

            Console.WriteLine("Building Graph...");
            var graph = BuildGraph(map);

            Console.WriteLine($"Graph Completed - {graph.Nodes.Length} nodes and {graph.Edges.Count} edges");

            var start = graph.Nodes.Where(g => g.Value.Length == 0).Single();
            var minPathLength = FindShortestPathLength(graph, start);
            Console.WriteLine($"Part 2 Result = {minPathLength}");
        }

        private static void ReplaceMapStart(string[] map)
        {
            var pos = FindLocationInMap(map, StartNode).Single();

            for (int i = 0; i < replacement.Length; ++i)
            {
                var line = map[pos.row - 1 + i];
                map[pos.row - 1 + i] = line.Substring(0, pos.col - 1) + replacement[i] + line.Substring(pos.col + 2);
            }
        }

        private static int FindShortestPathLength(Graph graph, Node start)
        {
            var unvisited = new HashSet<Node>(graph.Nodes);
            var cost = unvisited.ToDictionary(x => x, x => int.MaxValue);
            var prev = unvisited.ToDictionary(x => x, x => default(Node?));

            cost[start] = 0;
            while (unvisited.Count > 0)
            {
                var current = unvisited.OrderBy(v => cost[v]).FirstOrDefault();
                unvisited.Remove(current);

                foreach (var neighbor in graph.Edges[current].Where(n => unvisited.Contains(n.next)))
                {
                    var nextCost = cost[current] + neighbor.cost;
                    if (nextCost < cost[neighbor.next])
                    {
                        cost[neighbor.next] = nextCost;
                        prev[neighbor.next] = current;
                    }
                }
            }

            return cost.GroupBy(c => c.Key.Value.Length)
                       .OrderByDescending(g => g.Key)
                       .Select(g => g.Min(x => x.Value))
                       .First();
        }

        private static Graph BuildGraph(string[] map)
        {
            var cost = new Dictionary<(Node from, Node to), int>();
            var nodes = new HashSet<Node>();

            var starts = FindLocationInMap(map, StartNode).ToImmutableArray();

            var queue = new Queue<Node>();
            queue.Enqueue(new Node(null, starts));

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                //var lastKey = node.Value.DefaultIfEmpty(StartNode).Last();

                nodes.Add(node);

                var reachable = from index in Enumerable.Range(0, node.Positions.Length)
                                from item in FindReachableKeys(map, node.Positions[index], node)
                                select (pos: item.Key, cost: item.Value, index);
                foreach (var item in reachable)
                {
                    var newNode = node.WithKeyAndPositions(map[item.pos.row][item.pos.col],
                                                           node.Positions.SetItem(item.index, item.pos));

                    var edge = (node, newNode);
                    if (!cost.TryGetValue(edge, out var minEdgeCost) || minEdgeCost > item.cost)
                    {
                        cost[edge] = item.cost;
                        queue.Enqueue(newNode);
                    }
                }
            }

            return new Graph(
                nodes,
                cost.Select(x => (x.Key.from, x.Key.to, x.Value))
            );
        }

        private static ImmutableDictionary<(int row, int col), int> FindReachableKeys(string[] map, (int row, int col) startPosition, Node foundKeys)
        {
            var reachable = ImmutableDictionary.CreateBuilder<(int row, int col), int>();

            var minCost = ImmutableDictionary.CreateBuilder<(int row, int col), int>();

            var queue = new Queue<((int row, int col) pos, int distance)>();
            queue.Enqueue((startPosition, 0));

            while (queue.Count > 0)
            {
                var (pos, distance) = queue.Dequeue();

                if (!minCost.TryGetValue(pos, out var cost) || cost > distance)
                {
                    minCost[pos] = distance;
                    var current = map[pos.row][pos.col];

                    if (IsKey(current) && !foundKeys.Contains(current))
                    {
                        reachable[pos] = distance;
                    }
                    else
                    {
                        foreach (var nextPos in EnumAllMoves(pos.row, pos.col, map))
                        {
                            var next = map[nextPos.row][nextPos.col];
                            if (next == WallNode)
                                continue;

                            if (!IsDoor(next) || foundKeys.Contains(char.ToLowerInvariant(next)))
                            {
                                queue.Enqueue((nextPos, distance + 1));
                            }
                        }
                    }
                }

            }

            return reachable.ToImmutable();
        }

        private static IEnumerable<(int row, int col)> FindLocationInMap(string[] map, char node)
        {
            for (var row = 0; row < map.Length; ++row)
            {
                var line = map[row];
                for (var col = 0; col < line.Length; ++col)
                {
                    if (line[col] == node)
                        yield return (row, col);
                }
            }
        }

        private static bool IsDoor(char ch) => ch >= 'A' && ch <= 'Z';

        private static bool IsKey(char ch) => ch >= 'a' && ch <= 'z';

        private static IEnumerable<(int row, int col)> EnumAllMoves(int row, int col, string[] map)
        {
            if (row > 0)
                yield return (row - 1, col);

            if (row < map.Length - 1)
                yield return (row + 1, col);

            if (col > 0)
                yield return (row, col - 1);

            if (col < map[row].Length - 1)
                yield return (row, col + 1);
        }

        private class Graph
        {
            public Graph(IEnumerable<Node> nodes, IEnumerable<(Node from, Node to, int cost)> edges)
            {
                Nodes = ImmutableArray.CreateRange(nodes ?? throw new ArgumentNullException(nameof(nodes)));
                Edges = edges.ToLookup(x => x.from, x => (x.to, x.cost));
            }

            public ImmutableArray<Node> Nodes { get; }

            public ILookup<Node, (Node next, int cost)> Edges { get; }
        }

        private struct Node : IEquatable<Node>
        {
            private readonly string value;

            public Node(string value, ImmutableArray<(int row, int col)> positions)
            {
                this.value = value;
                this.Positions = positions;
            }

            public string Value => value ?? string.Empty;

            public ImmutableArray<(int row, int col)> Positions { get; }

            public bool Contains(char key) => Value.IndexOf(key) >= 0;

            public Node WithKeyAndPositions(char key, ImmutableArray<(int row, int col)> positions)
            {
                var result = new StringBuilder(Value.Length + 1);
                foreach (var ch in Value.Concat(new[] { key }).OrderBy(x => x))
                    result.Append(ch);

                return new Node(result.ToString(), positions);
            }

            public bool Equals([AllowNull] Node other)
                => Positions.SequenceEqual(other.Positions)
                && string.Equals(Value, other.Value, StringComparison.Ordinal);

            public override bool Equals(object obj)
                => obj is Node node ? Equals(node) : false;

            public override int GetHashCode()
            {
                int hash = 17;
                hash = (hash * 23) + Value.GetHashCode();
                if (!Positions.IsDefaultOrEmpty)
                    hash = Positions.Aggregate(hash, (h, n) => (h * 23) + n.GetHashCode());

                return hash;
            }

            public override string ToString()
                => $"{Value}-{{{string.Join(",", Positions)}}}";
        }
    }
}
