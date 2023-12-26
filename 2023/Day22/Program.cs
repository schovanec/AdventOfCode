var bricks = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                 .Select(Brick.Parse)
                 .ToList();

var (dropped, _) = DropBricks(bricks);
var supports = FindSupports(dropped);
var critical = FindCriticalBricks(supports).ToList();
var result1 = bricks.Count - critical.Count;
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = critical.AsParallel()
                      .Sum(block =>
                      {
                        var destroyed = dropped.Where(b => b != block).ToList();
                        var (_, count) = DropBricks(destroyed);
                        return count;
                      });
Console.WriteLine($"Part 2 Result = {result2}");

static (List<Brick>, int dropped) DropBricks(ICollection<Brick> start, int floor = 1)
{
  var bricks = start.Where(brick => brick.Start.Z >= floor);
  var occupied = bricks.SelectMany(brick => brick.EnumPoints())
                       .ToHashSet();

  List<Brick> result = new();
  var count = 0;
  foreach (var brick in bricks.OrderBy(b => b.Start.Z))
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
      ++count;
    }
  }

  return (result, count);
}

static ILookup<Brick, Brick> FindSupports(ICollection<Brick> bricks)
{
  var allPoints = from b in bricks
                  from pt in b.EnumPoints()
                  select (point: pt, brick: b);
  var lookup = allPoints.ToLookup(x => x.point, x => x.brick);

  var supports = from b in bricks
                 from pt in b.EnumBottomRow()
                 from below in lookup[pt.Move(dz: -1)]
                 where below != b
                 select (block: b, support: below);

  return supports.Distinct()
                 .ToLookup(x => x.block, x => x.support);
}

static IEnumerable<Brick> FindCriticalBricks(ILookup<Brick, Brick> supports)
  => supports.Where(g => g.Count() == 1)
             .SelectMany(g => g)
             .Distinct();

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