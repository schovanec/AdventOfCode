var edges = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(x => x.Split('-'))
                .SelectMany(x => new[] { (from: x[0], to: x[1]), (from: x[1], to: x[0]) })
                .ToLookup(x => x.from, x => x.to);

var paths1 = FindAllPaths(
  edges, "start", "end",
  (n, v) => IsLargeCave(n) || v.GetValueOrDefault(n) == 0);

Console.WriteLine($"Part 1 Result = {paths1.Count()}");

var paths2 = FindAllPaths(
  edges, "start", "end",
  (n, v) => n switch
  {
    "start" or "end" => v.GetValueOrDefault(n) == 0,
    _ when IsSmallCave(n) => v.GetValueOrDefault(n) == 0
                             || !v.Any(x => IsSmallCave(x.Key) && x.Value > 1),
    _ => true
  });

Console.WriteLine($"Part 2 Result = {paths2.Count()}");

IEnumerable<Path> FindAllPaths(ILookup<string, string> edges, string from, string to,
                               Func<string, Dictionary<string, int>, bool> canVisitNode,
                               Dictionary<string, int>? visited = default)
{
  visited ??= new Dictionary<string, int>();
  visited[from] = visited.GetValueOrDefault(from) + 1;

  var path = new Path(from);

  if (from == to)
  {
    yield return path;
  }
  else
  {
    foreach (var node in edges[from])
    {
      if (canVisitNode(node, visited))
      {
        foreach (var result in FindAllPaths(edges, node, to, canVisitNode, visited))
          yield return path with { Tail = result };
      }
    }
  }

  visited[from]--;
}

bool IsLargeCave(string node) => node.All(char.IsUpper);

bool IsSmallCave(string node) => node is not "start" or "end" && !IsLargeCave(node);

record Path(string Node, Path? Tail = null);