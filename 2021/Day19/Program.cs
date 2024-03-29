﻿using System.Collections.Immutable;

var input = Parse(File.ReadLines(args.FirstOrDefault() ?? "input.txt"));

var matched = AlignAllScanners(input);

var uniquePoints = matched.Select(s => s.AbsoluteBeacons)
                          .Aggregate((a, b) => a.Union(b))
                          .ToList();
Console.WriteLine($"Part 1 Result = {uniquePoints.Count}");

var locations = matched.Select(x => x.Position).ToList();
var maxDistance = (from i in Enumerable.Range(0, locations.Count - 1)
                   from j in Enumerable.Range(i + 1, locations.Count - i - 1)
                   select locations[i].ManhattanDistance(locations[j])).Max();
Console.WriteLine($"Part 2 Result = {maxDistance}");

IList<Scanner> AlignAllScanners(IEnumerable<Scanner> scanners)
{
  var unmatched = scanners.GroupBy(x => x.Id)
                          .ToDictionary(g => g.Key, g => g.ToImmutableList());
  var matched = new Dictionary<int, Scanner>();

  matched.Add(0, unmatched[0].First());
  unmatched.Remove(0);

  var queue = new Queue<int>();
  queue.Enqueue(0);

  while (queue.Count > 0 && unmatched.Count > 0)
  {
    var id = queue.Dequeue();
    var matches = AlignAnyScanners(matched[id], unmatched.Values);
    foreach (var m in matches)
    {
      matched[m.Id] = m;
      queue.Enqueue(m.Id);
      unmatched.Remove(m.Id);
    }
  }

  return matched.Values.ToList();
}

IEnumerable<Scanner> AlignAnyScanners(Scanner target, IEnumerable<IEnumerable<Scanner>> scanners)
  => scanners.SelectMany(g => AlignAnyOrientation(target, g));

IEnumerable<Scanner> AlignAnyOrientation(Scanner target, IEnumerable<Scanner> scanners)
  => scanners.SelectMany(x => AlignSingleOrientation(target, x))
             .Take(1);

IEnumerable<Scanner> AlignSingleOrientation(Scanner target, Scanner scanner)
  => (from t in target.AbsoluteBeacons
      from s in scanner.RelativeBeacons
      let offset = t.Subtract(s)
      let moved = scanner with { Position = offset }
      where target.AbsoluteBeacons.Intersect(moved.AbsoluteBeacons).Count() >= 12
      select moved).Take(1);

ImmutableList<Scanner> Parse(IEnumerable<string> input)
{
  var scanners = ImmutableList.CreateBuilder<Scanner>();

  var id = -1;
  var beacons = new List<Vector>();
  foreach (var line in input)
  {
    if (line.StartsWith("---"))
    {
      if (beacons.Count > 0)
        scanners.AddRange(Scanner.CreateOrientations(id, beacons));

      beacons.Clear();
      ++id;
    }
    else if (id >= 0 && !string.IsNullOrEmpty(line))
    {
      var coords = line.Split(',');
      beacons.Add(new Vector(
        int.Parse(coords[0]),
        int.Parse(coords[1]),
        int.Parse(coords[2])
      ));
    }
  }

  if (beacons.Count > 0)
    scanners.AddRange(Scanner.CreateOrientations(id, beacons));

  return scanners.ToImmutable();
}

record struct Vector(int X, int Y, int Z)
{
  public Vector Subtract(Vector other)
    => new(X - other.X, Y - other.Y, Z - other.Z);

  public Vector Add(Vector other)
    => new(X + other.X, Y + other.Y, Z + other.Z);

  public int ManhattanDistance(Vector other)
    => Math.Abs(other.X - X) + Math.Abs(other.Y - Y) + Math.Abs(other.Z - Z);

  public IEnumerable<Vector> EnumFacingDirections()
  {
    var current = this;
    for (var i = 0; i < 3; ++i)
    {
      yield return current;
      yield return new(-current.X, -current.Y, current.Z);

      current = new(current.Y, current.Z, current.X);
    }
  }

  public IEnumerable<Vector> EnumRotations()
  {
    var current = this;
    for (var i = 0; i < 4; ++i)
    {
      yield return current;
      current = new(current.X, -current.Z, current.Y);
    }
  }

  public IEnumerable<Vector> EnumOrientations()
    => EnumFacingDirections().SelectMany(v => v.EnumRotations());
};

record Scanner(int Id, ImmutableHashSet<Vector> RelativeBeacons, Vector Position = default)
{
  public IEnumerable<Vector> AbsoluteBeacons
    => RelativeBeacons.Select(v => v.Add(Position));

  public static IEnumerable<Scanner> CreateOrientations(int id, IEnumerable<Vector> beacons)
  {
    return beacons.SelectMany(b => b.EnumOrientations().Select((v, i) => (index: i, vector: v)))
                  .GroupBy(v => v.index, g => g.vector)
                  .Select(g => new Scanner(id, g.ToImmutableHashSet()));
  }
};
