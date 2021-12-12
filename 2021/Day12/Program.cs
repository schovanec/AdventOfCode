using System.Collections.Immutable;
using System.Text;

var edges = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(x => x.Split('-'))
                .SelectMany(x => new[] { (from: x[0], to: x[1]), (from: x[1], to: x[0]) })
                .ToLookup(x => x.from, x => x.to);

var count1 = FindAllPathsPart1(edges, "start", "end").Count();
Console.WriteLine($"Part 1 Result = {count1}");

var count2 = FindAllPathsPart2(edges, "start", "end").Count();
Console.WriteLine($"Part 2 Result = {count2}");

IEnumerable<Path> FindAllPathsPart1(ILookup<string, string> edges, string from, string to)
  => FindAllPathsInternal(edges,
                          from,
                          to,
                          (n, v) => IsLargeCave(n) || !v.ContainsKey(n));

IEnumerable<Path> FindAllPathsPart2(ILookup<string, string> edges, string from, string to)
  => FindAllPathsInternal(edges,
                          from,
                          to,
                          (n, v) => n switch
                          {
                            "start" or "end" => !v.ContainsKey(n),
                            _ when IsSmallCave(n) => !v.ContainsKey(n) || !v.Any(x => IsSmallCave(x.Key) && x.Value > 1),
                            _ => true
                          });

IEnumerable<Path> FindAllPathsInternal(ILookup<string, string> edges,
                                       string from,
                                       string to,
                                       Func<string, ImmutableDictionary<string, int>, bool> canVisitNode,
                                       ImmutableDictionary<string, int>? visited = default)
{
  visited ??= ImmutableDictionary.Create<string, int>();
  visited = visited.SetItem(from, visited.GetValueOrDefault(from) + 1);

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
        foreach (var result in FindAllPathsInternal(edges, node, to, canVisitNode, visited))
          yield return path with { Tail = result };
      }
    }
  }
}

bool IsLargeCave(string node) => node.All(char.IsUpper);

bool IsSmallCave(string node) => node is not "start" or "end" && !IsLargeCave(node);

record Path(string Node, Path? Tail = null)
{
  public override string ToString()
  {
    var result = new StringBuilder();

    var current = this;
    while (current != null)
    {
      if (result.Length > 0)
        result.Append(',');

      result.Append(current.Node);
      current = current.Tail;
    }

    return result.ToString();
  }
}