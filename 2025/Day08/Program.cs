using System.Collections.Immutable;

var points = File.ReadLines(args.FirstOrDefault("input.txt"))
                 .Select(Point.Parse)
                 .ToImmutableList();

var pairsToConnect = int.Parse(args.Skip(1).FirstOrDefault("1000"));

var (circuits, _, _) = FindCircuits(points, pairsToConnect);
var result1 = circuits.OrderByDescending(x => x.Count)
                      .Take(3)
                      .Select(x => x.Count)
                      .Aggregate(1, (a, b) => a*b);
Console.WriteLine($"Part 1 Result = {result1}");

var (_, lastA, lastB) = FindCircuits(points, int.MaxValue);
var result2 = lastA.X * lastB.X;
Console.WriteLine($"Part 2 Result = {result2}");

static (IEnumerable<ImmutableHashSet<Point>> circuits, Point lastA, Point lastB) FindCircuits(ImmutableList<Point> points, int limit)
{
  List<ImmutableHashSet<Point>> circuits = points.Select(p => ImmutableHashSet.Create(p))
                                                 .ToList();

  var wires = CalculateDistances(points).OrderBy(x => x.distance);

  foreach (var w in wires.Take(limit))
  {
    var firstIdx = circuits.FindIndex(x => x.Contains(w.first));
    var secondIdx = circuits.FindIndex(x => x.Contains(w.second));

    if (firstIdx != secondIdx)
    {
      circuits[firstIdx] = circuits[firstIdx].Union(circuits[secondIdx]);
      circuits.RemoveAt(secondIdx);
    }

    if (circuits.Count == 1)
      return (circuits, w.first, w.second);
  }

  return (circuits, default, default);
}

static IEnumerable<(Point first, Point second, double distance)> CalculateDistances(ImmutableList<Point> points)
{
  for (var i = 0; i < points.Count; ++i)
  {
    var first = points[i];
    for (var j = i + 1; j < points.Count; ++j)
    {
      var second = points[j];
      yield return (first, second, first.DistanceToSquared(second));
    }
  }
}

record struct Point(long X, long Y, long Z)
{
  public long LengthSquared
    => X*X + Y*Y + Z*Z;

  public Point Subtract(Point other)
    => new (other.X - X, other.Y - Y, other.Z - Z);

  public long DistanceToSquared(Point other)
    => Subtract(other).LengthSquared;

  public static Point Parse(string input)
  {
    var parts = input.Split(',', 3, StringSplitOptions.TrimEntries);
    return new (
      long.Parse(parts[0]),
      long.Parse(parts[1]),
      long.Parse(parts[2])
    );
  }
}