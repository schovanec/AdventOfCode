using System.Collections.Immutable;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var result1 = FindPathLength(map, map.Start, map.Goal);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = map.Heights
                 .Where(x => x.Value == 1)
                 .Select(x => FindPathLength(map, x.Key, map.Goal))
                 .Where(p => p.HasValue)
                 .Min();
Console.WriteLine($"Part 2 Result = {result2}");

static int? FindPathLength(Map map, Point start, Point goal)
{
  var depth = new Dictionary<Point, int>() { [start] = 0 };
  var queue = new Queue<Point>(depth.Keys);
  while (queue.Count > 0)
  {
    var pt = queue.Dequeue();
    if (pt == goal)
      break;

    var d = depth[pt];
    var adjacent = map.GetValidMoves(pt)
                      .Where(x => !depth.ContainsKey(x));
    foreach (var item in adjacent)
    {
      depth[item] = d + 1;
      queue.Enqueue(item);
    }
  }

  return depth.TryGetValue(goal, out var result)
    ? result
    : default(int?);
}

record struct Point(int X, int Y)
{
  public IEnumerable<Point> GetAdjacentPoints()
  {
    yield return new(X - 1, Y);
    yield return new(X, Y - 1);
    yield return new(X + 1, Y);
    yield return new(X, Y + 1);
  }
}

record Map(ImmutableDictionary<Point, int> Heights, Point Start, Point Goal)
{
  public IEnumerable<Point> GetValidMoves(Point location)
  {
    if (!Heights.TryGetValue(location, out var height))
      return Enumerable.Empty<Point>();

    var max = height + 1;
    return from pt in location.GetAdjacentPoints()
           where Heights.TryGetValue(pt, out var ptHeight) && ptHeight <= max
           select pt;
  }

  public static Map Parse(string[] input)
  {
    Point start = default,
          goal = default;
    var result = ImmutableDictionary.CreateBuilder<Point, int>();

    for (var row = 0; row < input.Length; ++row)
    {
      var line = input[row];
      for (var col = 0; col < line.Length; ++col)
      {
        var pos = new Point(col, row);
        var ch = line[col];
        if (ch == 'S')
          (result[pos], start) = (1, pos);
        else if (ch == 'E')
          (result[pos], goal) = (26, pos);
        else
          result[pos] = ch - 'a' + 1;
      }
    }

    return new(result.ToImmutable(), start, goal);
  }
}