using System.Collections.Immutable;
using Edge = (string a, string b);
using DirectedEdge = (string from, string to);

var edges = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .SelectMany(ParseEdges)
                .Select(Canonicalize)
                .ToArray();

var allNodes = edges.SelectMany(x => new[] { x.a, x.b }).ToHashSet();

var shuffledNodes = Shuffle(allNodes.ToImmutableList());
var pathsToFind = from i in Enumerable.Range(0, shuffledNodes.Count / 2)
                  select (start: shuffledNodes[i], goal: shuffledNodes[i + 1]);

var lookup = EdgesToLookup(edges);

var mostFrequentEdges = from pair in pathsToFind.Take(1000)
                        let path = FindPath(pair.start, pair.goal, n => lookup[n])
                        from edge in path.Zip(path.Skip(1)).Select(Canonicalize)
                        group edge by edge into g
                        orderby g.Count() descending
                        select g.Key;

var edgesToRemove = mostFrequentEdges.Take(3)
                                     .SelectMany(UndirectToDirected)
                                     .ToList();

var remainingEdgeLookup = EdgesToLookup(edges.Except(edgesToRemove));

var components = FindConnectedComponents(allNodes, n => remainingEdgeLookup[n]).ToList();

var result1 = components[0] * components[1];
Console.WriteLine($"Part 1 Result = {result1}");

static IEnumerable<int> FindConnectedComponents(IEnumerable<string> nodes, Func<string, IEnumerable<string>> adjacent)
{
  HashSet<string> visited = new();
  foreach (var node in nodes)
  {
    if (!visited.Contains(node))
      yield return Search(node);
  }

  int Search(string node)
  {
    visited.Add(node);
    return 1 + adjacent(node).Where(n => !visited.Contains(n))
                             .Sum(Search);
  }
}

static ILookup<string, string> EdgesToLookup(IEnumerable<Edge> edges)
  => edges.SelectMany(UndirectToDirected)
          .ToLookup(e => e.from, e => e.to);

static IEnumerable<DirectedEdge> UndirectToDirected(Edge edge)
{
  yield return edge;
  yield return (edge.b, edge.a);
}

static Edge Canonicalize(Edge edge)
  => string.Compare(edge.a, edge.b, StringComparison.Ordinal) <= 0
    ? edge
    : (edge.b, edge.a);

static ImmutableList<string> FindPath(string start, string goal, Func<string, IEnumerable<string>> adjacent)
{
  Queue<string> queue = new([start]);
  HashSet<string> visited = [start];
  Dictionary<string, string> parents = new();

  while (queue.TryDequeue(out var v))
  {
    if (v == goal)
      break;

    foreach (var w in adjacent(v).Where(n => !visited.Contains(n)))
    {
      visited.Add(w);
      parents[w] = v;
      queue.Enqueue(w);
    }
  }

  var result = ImmutableList.CreateBuilder<string>();
  var current = goal;
  while (parents.TryGetValue(current, out var parent))
  {
    result.Insert(0, parent);
    current = parent;
  }

  return result.ToImmutable();
}

static ImmutableList<T> Shuffle<T>(ImmutableList<T> list)
{
  var rng = Random.Shared;
  var builder = list.ToBuilder();
  var n = list.Count;
  while (n > 1)
  {
    --n;
    int k = rng.Next(n + 1);
    (builder[k], builder[n]) = (builder[n], builder[k]);
  }

  return builder.ToImmutable();
}

static IEnumerable<Edge> ParseEdges(string line)
{
  var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
  var a = parts[0];
  return parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                 .Select(b => (a, b));
}
