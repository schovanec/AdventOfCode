using System.Collections.Immutable;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault() ?? @"input.txt"));
var stepsGoal = args.Skip(1).Select(int.Parse).FirstOrDefault(64);

var result1 = CountReachable(map, stepsGoal).Count(x => x.dist == stepsGoal);
Console.WriteLine($"Part 1 Result = {result1}");

static HashSet<(Point pt, int dist)> CountReachable(Map map, int goal, IEnumerable<Point>? starts = default)
{
  HashSet<(Point, int)> visited = [.. (starts ?? [map.Start]).Select(pt => (pt, 0))];
  Queue<(Point, int)> queue = new(visited);

  while (queue.Count > 0)
  {
    var (pt, dist) = queue.Dequeue();
    var adjacent = pt.EnumAdjacent()
                     .Select(pt => (pt, dist: dist + 1))
                     .Where(x => map.IsPlot(x.pt) && !visited.Contains(x));

    foreach (var next in adjacent)
    {
      visited.Add(next);
      if (next.dist < goal)
        queue.Enqueue(next);
    }
  }

  return visited;
}

record struct Point(int X, int Y)
{
  public IEnumerable<Point> EnumAdjacent()
    => [new(X - 1, Y), new(X + 1, Y), new(X, Y - 1), new(X, Y + 1)];
}

record Map(int Width, int Height, ImmutableHashSet<Point> Rocks, Point Start)
{
  public bool Contains(Point pt)
    => pt.X >= 0 && pt.X < Width
    && pt.Y >= 0 && pt.Y < Height;

  public bool IsRock(Point pt) => Rocks.Contains(pt);

  public bool IsPlot(Point pt) => Contains(pt) && !IsRock(pt);

  public static Map Parse(string[] input)
  {
    var height = input.Length;
    var width = input[0].Length;
    var start = default(Point);
    var rocks = ImmutableHashSet.CreateBuilder<Point>();
    for (var y = 0; y < height; ++y)
    {
      var line = input[y];

      for (var x = 0; x < width; ++x)
      {
        var ch = line[x];
        if (ch == '#')
          rocks.Add(new(x, y));
        else if (ch == 'S')
          start = new(x, y);
      }
    }

    return new(width, height, rocks.ToImmutable(), start);
  }
}