var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(ParsePath)
                .ToList();

var walls = (from path in input
             from p in EnumPointsInPath(path)
             select p).ToHashSet();

var start = new Point(500, 0);
var bottom = walls.Select(p => p.Y).Max();

var sand1 = PourSand(start, bottom, walls.Contains);
Console.WriteLine($"Part 1 Result = {sand1.Count}");

var floor = bottom + 2;
var sand2 = PourSand(start, int.MaxValue, p => p.Y == floor || walls.Contains(p));
Console.WriteLine($"Part 2 Result = {sand2.Count}");

static HashSet<Point> PourSand(Point start, int bottom, Func<Point, bool> wall)
{
  HashSet<Point> sand = new();

  while (true)
  {
    var sandPos = PlaceSandUnit(start, bottom, p => wall(p) || sand.Contains(p));
    if (!sandPos.HasValue)
      break;

    sand.Add(sandPos.Value);
  }

  return sand;
}

static Point? PlaceSandUnit(Point start, int bottom, Func<Point, bool> blocked)
{
  if (blocked(start))
    return null;

  var pos = start;
  while (pos.Y <= bottom)
  {
    var next = (from p in EnumNextPositions(pos)
                where !blocked(p)
                select (Point?)p).FirstOrDefault();
    if (!next.HasValue)
      return pos;

    pos = next.Value;
  }

  return null;
}

static IEnumerable<Point> EnumNextPositions(Point point)
{
  var newY = point.Y + 1;
  yield return new(point.X, newY);
  yield return new(point.X - 1, newY);
  yield return new(point.X + 1, newY);
}

static IEnumerable<Point> EnumPointsInPath(IEnumerable<Point> path)
  => from pair in path.Zip(path.Skip(1))
     from p in EnumPointsInLine(pair.First, pair.Second)
     select p;

static IEnumerable<Point> EnumPointsInLine(Point start, Point end)
{
  if (start.X == end.X)
  {
    var minY = Math.Min(start.Y, end.Y);
    var count = Math.Abs(end.Y - start.Y) + 1;
    return from y in Enumerable.Range(minY, count)
           select new Point(start.X, y);
  }
  else if (start.Y == end.Y)
  {
    var minX = Math.Min(start.X, end.X);
    var count = Math.Abs(end.X - start.X) + 1;
    return from x in Enumerable.Range(minX, count)
           select new Point(x, start.Y);
  }
  else
  {
    return Enumerable.Empty<Point>();
  }
}

static Point[] ParsePath(string input)
  => input.Split("->", StringSplitOptions.TrimEntries)
          .Select(p => p.Split(',', 2))
          .Select(p => new Point(int.Parse(p[0]), int.Parse(p[1])))
          .ToArray();

record struct Point(int X, int Y);