using System.Collections.Immutable;
using System.Diagnostics;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var start = map.Start;
var cycle = FindCycle(map, start);
var result1 = (cycle.Length + 1) / 2;
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = FindPointsInside([start, .. cycle, start]);
Console.WriteLine($"Part 2 Result = {result2}");

static ImmutableArray<Point> FindCycle(Map map, Point start)
{
  var stack = new Stack<(Point pos, ImmutableDictionary<Point, int> visited)>();
  stack.Push((start, ImmutableDictionary<Point, int>.Empty));

  while (stack.Count > 0)
  {
    var (pos, visited) = stack.Pop();
    foreach (var next in map.EnumNext(pos))
    {
      if (next == start)
      {
        if (visited.Count > 1)
        {
          return visited.OrderBy(x => x.Value)
                        .Select(x => x.Key)
                        .ToImmutableArray();
        }
      }
      else if (!visited.ContainsKey(next))
      {
        stack.Push((next, visited.Add(next, visited.Count)));
      }
    }
  }

  return ImmutableArray<Point>.Empty;
}

int FindPointsInside(IEnumerable<Point> path)
{
  var pointsOnPath = path.Select(p => new Point(p.X * 2, p.Y * 2)).ToArray();
  var bounds = Box.FromPoints(pointsOnPath).Grow(1);
  HashSet<Point> visited = [bounds.Min, .. EnumPointsOnPath(pointsOnPath)];
  Queue<Point> queue = new([bounds.Min]);
  while (queue.Count > 0)
  {
    var current = queue.Dequeue();
    foreach (var pt in current.EnumAdjacent().Where(pt => bounds.Contains(pt)))
    {
      if (!visited.Contains(pt))
      {
        visited.Add(pt);
        queue.Enqueue(pt);
      }
    }
  }

  var pathBounds = Box.FromPoints(path);
  var area = (pathBounds.Max.X - pathBounds.Min.X + 1) * (pathBounds.Max.Y - pathBounds.Min.Y + 1);
  var outside = visited.Count(pt => pt.X % 2 == 0 && pt.Y % 2 == 0);
  return area - outside;
}

IEnumerable<Point> EnumPointsOnPath(IEnumerable<Point> path)
{
  Point prev = new();
  bool first = true;
  foreach (var next in path)
  {

    if (!first)
      yield return new((prev.X + next.X) / 2, (prev.Y + next.Y) / 2);

    yield return next;

    prev = next;
    first = false;
  }
}

record struct Point(int X, int Y)
{
  public Point Offset(int dx = 0, int dy = 0)
    => new(X + dx, Y + dy);

  public IEnumerable<Point> EnumAdjacent()
  {
    yield return Offset(dx: -1);
    yield return Offset(dx: 1);
    yield return Offset(dy: -1);
    yield return Offset(dy: 1);
  }
}

record struct Box(Point Min, Point Max)
{
  private static readonly Box Empty = new(
    new Point(int.MaxValue, int.MaxValue),
    new Point(int.MinValue, int.MinValue));

  public Box Grow(int amount = 1)
    => new(new(Min.X - amount, Min.Y - amount),
           new(Max.X + amount, Max.Y + amount));

  public bool Contains(Point pt)
    => pt.X >= Min.X && pt.X <= Max.X
    && pt.Y >= Min.Y && pt.Y <= Max.Y;

  public static Box FromPoints(IEnumerable<Point> points)
    => points.Aggregate(Empty,
      (b, p) => new Box(
        new Point(
          Math.Min(b.Min.X, p.X),
          Math.Min(b.Min.Y, p.Y)),
        new Point(
          Math.Max(b.Max.X, p.X),
          Math.Max(b.Max.Y, p.Y))));
}

record Map(ImmutableDictionary<Point, char> Nodes, ILookup<Point, Point> Edges)
{
  public Point Start => Nodes.First(x => x.Value == 'S').Key;

  public IEnumerable<Point> EnumNext(Point pos) => Edges[pos];

  public static Map Parse(string[] input)
  {
    var result = ImmutableDictionary.CreateBuilder<Point, char>();

    for (var y = 0; y < input.Length; ++y)
    {
      var line = input[y];
      for (var x = 0; x < line.Length; ++x)
      {
        var ch = line[x];
        if (ch != '.')
          result[new Point(x, y)] = ch;
      }
    }

    var nodes = result.ToImmutable();
    return new(nodes, FindEdges(nodes));
  }

  private static ILookup<Point, Point> FindEdges(ImmutableDictionary<Point, char> map)
  {
    var candidates = map.ToDictionary(x => x.Key, x => EnumJoints(x.Value, x.Key));

    var edges = from item in candidates
                from next in item.Value
                where candidates.GetValueOrDefault(next, Enumerable.Empty<Point>()).Contains(item.Key)
                select (start: item.Key, end: next);

    return edges.ToLookup(e => e.start, e => e.end);
  }
  private static IEnumerable<Point> EnumJoints(char type, Point pos)
    => type switch
    {
      '|' => [pos.Offset(dy: -1), pos.Offset(dy: 1)],
      '-' => [pos.Offset(dx: -1), pos.Offset(dx: 1)],
      'L' => [pos.Offset(dy: -1), pos.Offset(dx: 1)],
      'J' => [pos.Offset(dy: -1), pos.Offset(dx: -1)],
      '7' => [pos.Offset(dy: 1), pos.Offset(dx: -1)],
      'F' => [pos.Offset(dy: 1), pos.Offset(dx: 1)],
      'S' => pos.EnumAdjacent(),
      _ => Enumerable.Empty<Point>()
    };
}