using System.Collections.Immutable;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var result1 = CountVisitedPoints(map);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = map.EnumObstructionOptions()
                 .AsParallel()
                 .Count(m => !CountVisitedPoints(m).HasValue);
Console.WriteLine($"Part 2 Result = {result2}");

static int? CountVisitedPoints(Map map)
{
  var patrolled = new HashSet<Point>();
  var visited = new HashSet<(Point, Point)>();
  var pt = map.GuardLocation;
  var dir = new Point(0, -1);

  while (map.Contains(pt))
  {
    if (visited.Contains((pt, dir)))
      return default;

    visited.Add((pt, dir));
    patrolled.Add(pt);
    var next = pt.Offset(dir);
    if (map.HasObstacle(next))
      dir = dir.RotateRight();
    else
      pt = next;
  }

  return patrolled.Count;
}

record struct Point(int X, int Y)
{
  public Point RotateRight() => new(-Y, X);

  public Point Offset(Point dir) => new(X + dir.X, Y + dir.Y);
}

record Map(int Width, int Height, ImmutableHashSet<Point> Obstacles, Point GuardLocation)
{
  public bool Contains(Point pt)
    => pt.X >= 0 && pt.Y >= 0
    && pt.X < Width && pt.Y < Height;

  public bool HasObstacle(Point pt) => Obstacles.Contains(pt);

  public IEnumerable<Map> EnumObstructionOptions()
    => from x in Enumerable.Range(0, Width)
       from y in Enumerable.Range(0, Height)
       let pt = new Point(x, y)
       where pt != GuardLocation && !Obstacles.Contains(pt)
       select this with { Obstacles = Obstacles.Add(pt) };

  public static Map Parse(IList<string> input)
  {
    var width = input.First().Length;
    var height = input.Count;

    var obstacles = ImmutableHashSet.CreateBuilder<Point>();
    Point? guardLocation = default;

    for (var y = 0; y < height; ++y)
    {
      var line = input[y];
      for (var x = 0; x < width; ++x)
      {
        switch (line[x])
        {
          case '#':
            obstacles.Add(new(x, y));
            break;

          case '^':
            guardLocation = new(x, y);
            break;
        }
      }
    }

    return new(width, height, obstacles.ToImmutable(), guardLocation!.Value);
  }
}