var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(Sensor.Parse)
                .ToList();

var row = args.Skip(1).Select(int.Parse).DefaultIfEmpty(2000000).First();

var result1 = DoPart1(input, row);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = DoPart2(input, row * 2);
Console.WriteLine($"Part 2 Result = {result2}");

static int DoPart1(IList<Sensor> sensors, int row)
{
  var taken = from s in sensors
              let p = s.ClosestBeacon
              where p.Y == row
              select p.X;

  return sensors.SelectMany(s => s.GetSlice(row).Values)
                .Except(taken)
                .Count();
}

static long DoPart2(IList<Sensor> sensors, int size)
{
  var limit = new Range(0, size);

  for (var y = 0; y <= size; ++y)
  {
    var covered = sensors.Select(s => s.GetSlice(y));
    var gap = FindGap(covered, limit);
    if (gap is int x)
      return (x * 4000000L) + y;
  }

  return 0;
}

static int? FindGap(IEnumerable<Range> ranges, Range limit)
{
  var ordered = ranges.Select(r => r.Intersect(limit))
                      .Where(r => !r.IsEmpty)
                      .OrderBy(r => r.Min)
                      .ThenBy(r => r.Max);

  int max = limit.Min - 1;
  foreach (var r in ordered)
  {
    if (max + 1 < r.Min)
      return max + 1;

    max = Math.Max(max, r.Max);
  }

  return max < limit.Max
    ? max + 1
    : null;
}

record struct Range(int Min, int Max)
{
  public static Range Empty = new(0, -1);

  public bool IsEmpty => Min > Max;

  public IEnumerable<int> Values
    => IsEmpty ? Enumerable.Empty<int>() : Enumerable.Range(Min, Max - Min + 1);

  public bool Overlaps(Range other)
    => !IsEmpty
    && !other.IsEmpty
    && Min <= other.Max
    && Max >= other.Min;

  public Range Intersect(Range other)
    => Overlaps(other) ? new(Math.Max(Min, other.Min), Math.Min(Max, other.Max)) : Empty;
}

record struct Point(int X, int Y)
{
  public int ManhattanDistance(Point other)
    => Math.Abs(other.X - X) + Math.Abs(other.Y - Y);
}

record Sensor(Point Position, Point ClosestBeacon)
{
  public int BeaconDistance { get; } = Position.ManhattanDistance(ClosestBeacon);

  public Range GetSlice(int y)
  {
    var dy = Math.Abs(y - Position.Y);
    if (dy > BeaconDistance)
      return Range.Empty;

    var dx = BeaconDistance - dy;
    return new(Position.X - dx, Position.X + dx);
  }

  public static Sensor Parse(string line)
  {
    var parts = line.Split(' ');

    var sensorPos = new Point(
      int.Parse(parts[2][2..^1]),
      int.Parse(parts[3][2..^1]));

    var beaconPos = new Point(
      int.Parse(parts[8][2..^1]),
      int.Parse(parts[9][2..]));

    return new(sensorPos, beaconPos);
  }
}