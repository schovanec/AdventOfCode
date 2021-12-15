using System.Collections.Immutable;

var map = Map.Parse(File.ReadLines(args.FirstOrDefault() ?? "input.txt"));

var lowestRisk1 = FindLowestRiskPathLength(map, map.Start, map.End);
Console.WriteLine($"Part 1 Result = {lowestRisk1}");

var expanded = map.Expand(5);
var lowestRisk2 = FindLowestRiskPathLength(expanded, expanded.Start, expanded.End);
Console.WriteLine($"Part 2 Result = {lowestRisk2}");

int FindLowestRiskPathLength(Map map, (int x, int y) from, (int x, int y) to)
{
  var dist = new Dictionary<(int x, int y), int> { [from] = 0 };
  var visited = new HashSet<(int x, int y)>();
  var queue = new PriorityQueue<(int x, int y), int>();

  queue.Enqueue(from, 0);
  while (queue.Count > 0)
  {
    var pt = queue.Dequeue();
    if (visited.Contains(pt))
      continue;

    visited.Add(pt);

    if (to == pt)
      break;

    var adjacent = map.GetAdjacent(pt.x, pt.y).Where(x => !visited.Contains(x));
    var cost = dist[pt];
    foreach (var next in adjacent)
    {
      var alt = cost + map[next.x, next.y];
      if (alt < dist.GetValueOrDefault(next, int.MaxValue))
      {
        dist[next] = alt;
        queue.Enqueue(next, alt);
      }
    }
  }

  return dist[to];
}

record Map(ImmutableArray<int> RiskLevels, int Width, int RepeatCount = 1)
{
  public int Height { get; } = RiskLevels.Length / Width;

  public int ExpandedWidth { get; } = Width * RepeatCount;

  public int ExpandedHeight { get; } = (RiskLevels.Length / Width) * RepeatCount;

  public int this[int x, int y]
    => 1 + ((GetRawRisk(x % Width, y % Height) + (x / Width) + (y / Height) - 1) % 9);

  private int GetRawRisk(int x, int y) => RiskLevels[(y * Width) + x];

  public (int x, int y) Start => (0, 0);

  public (int x, int y) End => (ExpandedWidth - 1, ExpandedHeight - 1);

  public IEnumerable<(int x, int y)> GetAdjacent(int x, int y)
    => GetAdjacentCoordinates(x, y).Where(pt => IsValid(pt.x, pt.y));

  public Map Expand(int repeatCount) => new Map(RiskLevels, Width, repeatCount);

  private bool IsValid(int x, int y)
    => x >= 0 && x < ExpandedWidth && y >= 0 && y < ExpandedHeight;

  private IEnumerable<(int x, int y)> GetAdjacentCoordinates(int x, int y)
  {
    yield return (x - 1, y);
    yield return (x + 1, y);
    yield return (x, y - 1);
    yield return (x, y + 1);
  }

  public static Map Parse(IEnumerable<string> input)
  {
    var result = ImmutableArray.CreateBuilder<int>();
    var width = 0;

    foreach (var line in input)
    {
      width = line.Length;
      result.AddRange(line.Select(ch => ch - '0'));
    }

    return new Map(result.ToImmutable(), width);
  }
}