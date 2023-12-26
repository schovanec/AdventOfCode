var bricks = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                 .Select(Brick.Parse)
                 .ToArray();

var dropped = DropBricks(bricks);
var result1 = CountRedundantSupports(dropped);
Console.WriteLine($"Part 1 Result = {result1}");

static List<Brick> DropBricks(ICollection<Brick> start)
{
  var occupied = start.SelectMany(brick => brick.EnumPoints())
                      .ToHashSet();

  List<Brick> result = new();
  foreach (var brick in start.OrderBy(b => b.Start.Z))
  {
    var z = brick.Start.Z;
    var bottom = brick.EnumBottomRow().ToArray();
    var dz = 0;
    while (z + dz > 1)
    {
      var dzNew = dz - 1;
      var dropped = bottom.Select(pt => pt.Move(dz: dzNew));
      if (dropped.Any(occupied.Contains))
        break;

      dz = dzNew;
    }

    if (dz == 0)
    {
      result.Add(brick);
    }
    else
    {
      occupied.ExceptWith(brick.EnumPoints());
      var newBrick = brick.Move(dz: dz);
      occupied.UnionWith(newBrick.EnumPoints());
      result.Add(newBrick);
    }
  }

  return result;
}

static int CountRedundantSupports(ICollection<Brick> bricks)
{
  var allPoints = from b in bricks
                  from pt in b.EnumPoints()
                  select (point: pt, brick: b);
  var lookup = allPoints.ToDictionary(x => x.point, x => x.brick);

  foreach (var x in lookup.Keys.Where(pt => pt.Z < 1))
    Console.WriteLine(x);

  var supports = from b in bricks
                 from pt in b.EnumBottomRow()
                 let below = lookup.GetValueOrDefault(pt.Move(dz: -1))
                 where below != null && below != b
                 group below by b into g
                 select (block: g.Key, supports: g.Distinct().ToArray());

  var singles = supports.Where(g => g.supports.Length == 1)
                        .SelectMany(g => g.supports)
                        .Distinct();

  return bricks.Count - singles.Count();
}

record struct Point(int X, int Y, int Z)
{
  public Point Move(int dx = 0, int dy = 0, int dz = 0)
    => new(X + dx, Y + dy, Z + dz);

  public static Point Parse(string input)
  {
    var parts = input.Split(',', 3);
    return new(
      int.Parse(parts[0]),
      int.Parse(parts[1]),
      int.Parse(parts[2]));
  }
}

record Brick(Point Start, Point End)
{
  public Point GetSize()
    => new Point(
      End.X - Start.X + 1,
      End.Y - Start.Y + 1,
      End.Z - Start.Z + 1);

  public IEnumerable<Point> EnumPoints()
    => from x in Enumerable.Range(Start.X, End.X - Start.X + 1)
       from y in Enumerable.Range(Start.Y, End.Y - Start.Y + 1)
       from z in Enumerable.Range(Start.Z, End.Z - Start.Z + 1)
       select new Point(x, y, z);

  public IEnumerable<Point> EnumBottomRow()
    => GetSize().Z == 1
      ? EnumPoints()
      : [Start];

  public Brick Move(int dx = 0, int dy = 0, int dz = 0)
    => new(Start.Move(dx, dy, dz), End.Move(dx, dy, dz));

  public static Brick Parse(string input)
  {
    var parts = input.Split('~', 2);
    return new(Point.Parse(parts[0]), Point.Parse(parts[1]));
  }
}