using System.Collections.Immutable;

var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(Valve.Parse)
                .ToList();

var graph = SimplifyGraph(input, "AA").ToDictionary(x => x.Name);
var openValves = graph.Keys.Where(v => v != "AA").ToImmutableSortedSet();

var clock = System.Diagnostics.Stopwatch.StartNew();
var result1 = Search(
  "AA",
  openValves,
  (n, m) => graph[n].Distance[m],
  n => graph[n].Rate,
  30);
clock.Stop();
Console.WriteLine($"Part 1 Result = {result1}");
Console.WriteLine($"Part 1 Time = {clock.Elapsed.TotalMilliseconds:#,##0.000}ms");

clock.Restart();
var result2 = Search(
  "AA",
  openValves,
  (n, m) => graph[n].Distance[m],
  n => graph[n].Rate,
  26,
  2);
clock.Stop();
Console.WriteLine($"Part 2 Result = {result2}");
Console.WriteLine($"Part 2 Time = {clock.Elapsed.TotalMilliseconds:#,##0.000}ms");

static int Search(string start, ImmutableSortedSet<string> valves, Func<string, string, int> distance, Func<string, int> rate, int time, int actors = 1)
{
  Dictionary<(string, string, int, int), int> cache = new();

  return SearchInner(start, valves, time, actors - 1);

  int SearchInner(string node, ImmutableSortedSet<string> open, int remaining, int actorsLeft)
  {
    if (open.Count == 0)
      return 0;

    var cacheKey = (node, string.Concat(open), remaining, actorsLeft);
    if (cache.TryGetValue(cacheKey, out var cached))
      return cached;

    var best = 0;
    foreach (var next in open)
    {
      var cost = distance(node, next) + 1;
      var after = remaining - cost;
      if (after > 0)
      {
        var total = after * rate(next);
        total += SearchInner(next, open.Remove(next), after, actorsLeft);
        if (total > best)
          best = total;
      }
    }

    if (actorsLeft > 0)
    {
      var total = SearchInner(start, open, time, actorsLeft - 1);
      if (total > best)
        best = total;
    }

    return cache[cacheKey] = best;
  }
}

static IEnumerable<Valve> SimplifyGraph(IList<Valve> valves, params string[] keep)
{
  var lookup = valves.SelectMany(x => x.Tunnels, (v, t) => (v.Name, t)).ToLookup(x => x.Name, x => x.t);
  var important = valves.Where(v => keep.Contains(v.Name) || v.Rate > 0).ToDictionary(x => x.Name);
  foreach (var item in important.Values)
  {
    yield return item with
    {
      Distance = FindShortestPathsFromNode(item.Name, n => lookup[n])
        .Where(x => important.ContainsKey(x.node))
        .ToImmutableDictionary(x => x.node, x => x.cost)
    };
  }
}

static IEnumerable<(string node, int cost)> FindShortestPathsFromNode(string start, Func<string, IEnumerable<string>> edges)
{
  Dictionary<string, int> distance = new() { [start] = 0 };

  PriorityQueue<string, int> queue = new();
  queue.Enqueue(start, 0);

  while (queue.Count > 0)
  {
    var current = queue.Dequeue();
    foreach (var next in edges(current))
    {
      var d = distance[current] + 1;
      if (d < distance.GetValueOrDefault(next, int.MaxValue))
      {
        distance[next] = d;
        queue.Enqueue(next, d);
      }
    }
  }

  return distance.Where(x => x.Key != start).Select(x => (x.Key, x.Value));
}

record Valve(string Name, int Rate, ImmutableArray<string> Tunnels)
{
  public ImmutableDictionary<string, int> Distance { get; init; } = ImmutableDictionary<string, int>.Empty;

  public static Valve Parse(string line)
  {
    var parts = line.Split(' ');

    var name = parts[1];
    var rate = int.Parse(parts[4].TrimEnd(';').Split('=', 2)[1]);
    var tunnels = parts.Skip(9)
                       .Select(x => x.TrimEnd(','))
                       .ToImmutableArray();

    return new(name, rate, tunnels);
  }
}