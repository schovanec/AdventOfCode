using System.Collections.Immutable;

var edges = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(x => x.Split('-'))
                .SelectMany(x => new[] { (from: x[0], to: x[1]), (from: x[1], to: x[0]) })
                .ToLookup(x => x.from, x => x.to);

var paths1 = CountAllPaths(
  edges, "start", "end",
  (n, v) => IsLargeCave(n) || v.GetValueOrDefault(n) == 0);

Console.WriteLine($"Part 1 Result = {paths1}");

var paths2 = CountAllPaths(
  edges, "start", "end",
  (n, v) => n switch
  {
    "start" or "end" => v.GetValueOrDefault(n) == 0,
    _ when IsSmallCave(n) => v.GetValueOrDefault(n) == 0
                             || !v.Any(x => IsSmallCave(x.Key) && x.Value > 1),
    _ => true
  });

Console.WriteLine($"Part 2 Result = {paths2}");

int CountAllPaths(ILookup<string, string> edges, string from, string to,
                  Func<string, Dictionary<string, int>, bool> canVisitNode,
                  Dictionary<string, int>? visited = default)
{
  if (from == to)
    return 1;

  visited ??= new Dictionary<string, int>();
  visited[from] = visited.GetValueOrDefault(from) + 1;

  var result = edges[from].Where(n => canVisitNode(n, visited))
                          .Sum(n => CountAllPaths(edges, n, to, canVisitNode, visited));

  visited[from]--;
  return result;
}

bool IsLargeCave(string node) => node.All(char.IsUpper);

bool IsSmallCave(string node) => node is not "start" or "end" && !IsLargeCave(node);