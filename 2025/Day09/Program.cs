using System.Diagnostics;

var input = File.ReadLines(args.FirstOrDefault("input.txt"))
                .Select(Point.Parse)
                .ToList();

var result1 = FindAreas(input).Max();
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = FindAreasContained(input).Max();
Console.WriteLine($"Part 2 Result = {result2}");

static IEnumerable<long> FindAreas(IReadOnlyList<Point> points)
{
  for (var i = 0; i < points.Count; ++i)
  {
    var a = points[i];
    for (var j = i+1; j < points.Count; ++j)
    {
      var b = points[j];
      yield return (Math.Abs(b.X - a.X) + 1) * (Math.Abs(b.Y - a.Y) + 1);
    }
  }
}

static IEnumerable<long> FindAreasContained(IReadOnlyList<Point> points)
{
  var valid = FindValidRanges(points);
  for (var i = 0; i < points.Count; ++i)
  {
    var a = points[i];
    for (var j = i+1; j < points.Count; ++j)
    {
      var b = points[j];
      if (IsContainedInValidRange(a, b, valid))
        yield return (Math.Abs(b.X - a.X) + 1) * (Math.Abs(b.Y - a.Y) + 1);
    }
  }
}

static bool IsContainedInValidRange(Point start, Point end, IDictionary<long, (long min, long max)> valid)
{
  var minX = Math.Min(start.X, end.X);
  var maxX = Math.Max(start.X, end.X);
  var minY = Math.Min(start.Y, end.Y);
  var maxY = Math.Max(start.Y, end.Y);

  for (var y = minY; y <= maxY; ++y)
  {
    if (!valid.TryGetValue(y, out var range))
      return false;

    if (minX < range.min || maxX >= range.max)
      return false;
  }

  return true;
}

static IEnumerable<(Point start, Point end)> FindEdges(IReadOnlyList<Point> points)
{
  var prev = points.Last();
  foreach (var pt in points)
  {
    yield return (prev, pt);
    prev = pt;
  }
}

static Dictionary<long, (long min, long max)> FindValidRanges(IReadOnlyList<Point> points)
{
  var minY = points.Min(pt => pt.Y);
  var maxY = points.Max(pt => pt.Y);
  var edges = FindEdges(points).ToList();
  var result = new Dictionary<long, (long min, long max)>();
  for (var y = minY; y <= maxY; ++y)
  {
    var crossed = edges.Where(e => (e.start.Y <= y && e.end.Y >= y)
                                || (e.end.Y <= y && e.start.Y >= y))
                       .ToArray();
    if (crossed.Length > 0)
    {
      var minX = crossed.Min(e => Math.Min(e.start.X, e.end.X));
      var maxX = crossed.Max(e => Math.Max(e.start.X, e.end.X));
      result[y] = (minX, maxX);
    }
  }

  return result;
}

record struct Point(long X, long Y)
{
  public IEnumerable<Point> EnumAdjacent()
  {
    yield return new (X - 1, Y);
    yield return new (X + 1, Y);
    yield return new (X, Y - 1);
    yield return new (X, Y + 1);
  }

  public static Point Parse(string input)
  {
    var parts = input.Split(',', StringSplitOptions.TrimEntries);
    return new(long.Parse(parts[0]), long.Parse(parts[1]));
  }
}