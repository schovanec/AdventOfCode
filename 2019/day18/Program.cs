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
                "########################",
                "#@..............ac.GI.b#",
                "###d#e#f################",
                "###A#B#C################",
                "###g#h#i################",
                "########################"
            };
#endif
            Console.WriteLine("Building Graph...");
            var graph = BuildGraph(map);

            Console.WriteLine($"Graph Completed - {graph.Nodes.Length} nodes and {graph.Edges.Count} edges");

            var minPathLength = FindShortestPathLength(graph, graph.Nodes.OrderByDescending(x => x.Value.Length).First());
            Console.WriteLine($"Part 1 Result = {minPathLength}");
        }

        private static int FindShortestPathLength(Graph graph, Node target, Node start = default(Node))
        {
            var unvisited = new HashSet<Node>(graph.Nodes);
            var cost = unvisited.ToDictionary(x => x, x => int.MaxValue);
            var prev = unvisited.ToDictionary(x => x, x => default(Node?));

            cost[start] = 0;
            while (unvisited.Count > 0)
            {
                var current = unvisited.OrderBy(v => cost[v]).FirstOrDefault();
                unvisited.Remove(current);

                if (current.Equals(target))
                    break;

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

            return cost[target];
        }

        private static Graph BuildGraph(string[] map)
        {
            var cost = new Dictionary<(Node from, Node to), int>();
            var nodes = new HashSet<Node>();

            var queue = new Queue<Node>();
            queue.Enqueue(default(Node));

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                var lastKey = node.Value.DefaultIfEmpty(StartNode).Last();

                nodes.Add(node);

                var reachable = FindReachableKeys(map, lastKey, node);
                foreach (var item in reachable)
                {
                    var newNode = node.WithKey(item.Key);

                    var edge = (node, newNode);
                    if (!cost.TryGetValue(edge, out var minEdgeCost) || minEdgeCost > item.Value)
                    {
                        cost[edge] = item.Value;
                        queue.Enqueue(newNode);
                    }
                }
            }

            return new Graph(
                nodes,
                cost.Select(x => (x.Key.from, x.Key.to, x.Value))
            );
        }

        private static ImmutableDictionary<char, int> FindReachableKeys(string[] map, char start, Node foundKeys)
        {
            var reachable = ImmutableDictionary.CreateBuilder<char, int>();

            var startPosition = FindLocationInMap(map, start);
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
                        reachable[current] = distance;
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

        private static (int row, int col) FindLocationInMap(string[] map, char node)
        {
            for (var row = 0; row < map.Length; ++row)
            {
                var col = map[row].IndexOf(node);
                if (col >= 0)
                    return (row, col);
            }

            throw new InvalidOperationException();
        }

        private static IEnumerable<(char type, int row, int col)> FindNodes(string[] map)
        {
            for (var row = 0; row < map.Length; ++row)
            {
                var line = map[row];
                for (var col = 0; col < line.Length; ++col)
                {
                    var type = line[col];
                    if (type == StartNode ||
                        IsDoor(type) ||
                        IsKey(type))
                    {
                        yield return (type, row, col);
                    }
                }
            }
        }

        private static bool IsDoor(char ch) => ch >= 'A' && ch <= 'Z';

        private static bool IsKey(char ch) => ch >= 'a' && ch <= 'z';

        private static (int row, int col) FindStart(string[] map)
        {
            for (var row = 0; row < map.Length; ++row)
            {
                var col = map[row].IndexOf(StartNode);
                if (col >= 0)
                    return (row, col);
            }

            throw new InvalidOperationException();
        }

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

            public Node(string value)
            {
                this.value = value;
            }

            public string Value => value ?? string.Empty;

            public bool Contains(char key) => Value.IndexOf(key) >= 0;

            public Node WithKey(char key)
            {
                var result = new StringBuilder(Value.Length + 1);
                foreach (var ch in Value.OrderBy(x => x).Where(c => c != key))
                    result.Append(ch);

                result.Append(key);
                return new Node(result.ToString());
            }

            public bool Equals([AllowNull] Node other)
                => string.Equals(Value, other.Value, StringComparison.Ordinal);

            public override bool Equals(object obj)
                => obj is Node node ? Equals(node) : false;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value;
        }
    }
}
