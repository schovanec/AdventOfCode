using System.Collections.Immutable;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var result1 = map.FindGroups()
                 .Sum(g => CalculatePrice1(g.plant, g.points, map));
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = map.FindGroups()
                 .Sum(g => CalculatePrice2(g.plant, g.points, map));
Console.WriteLine($"Part 2 Result = {result2}");

static int CalculatePrice1(char group, IEnumerable<Point> points, Map map)
{
  var area = points.Count();

  var perimeter = points.SelectMany(p => p.EnumSides())
                        .Where(p => map[p] != group)
                        .Count();

  return area * perimeter;
}

static int CalculatePrice2(char group, IEnumerable<Point> points, Map map)
{
  var area = points.Count();

  var segments = 0;

  foreach (var row in points.GroupBy(p => p.Y).OrderBy(g => g.Key))
  {
    segments += CountRuns(row.Select(p => new Point(p.X, p.Y - 1)).Where(p => map[p] != group).Select(p => p.X));
    segments += CountRuns(row.Select(p => new Point(p.X, p.Y + 1)).Where(p => map[p] != group).Select(p => p.X));
  }

  foreach (var col in points.GroupBy(p => p.X).OrderBy(g => g.Key))
  {
    segments += CountRuns(col.Select(p => new Point(p.X - 1, p.Y)).Where(p => map[p] != group).Select(p => p.Y));
    segments += CountRuns(col.Select(p => new Point(p.X + 1, p.Y)).Where(p => map[p] != group).Select(p => p.Y));
  }

  return area * segments;
}

static int CountRuns(IEnumerable<int> seq)
{
  var result = 0;
  int? prev = default;
  foreach (var n in seq.OrderBy(x => x))
  {
    if (!prev.HasValue || prev < n - 1)
      ++result;

    prev = n;
  }

  return result;
}

record struct Point(int X, int Y)
{
  public IEnumerable<Point> EnumSides()
    => [new(X - 1, Y), new(X + 1, Y), new(X, Y - 1), new(X, Y + 1)];
}

record Map(int Width, int Height, ImmutableArray<string> Plots)
{
  public bool InBounds(Point pt)
    => pt.X >= 0 && pt.X < Width
    && pt.Y >= 0 && pt.Y < Height;

  public char this[Point pt]
    => InBounds(pt) ? Plots[pt.Y][pt.X] : default;

  public IEnumerable<(char plant, ImmutableList<Point> points)> FindGroups()
  {
    var allVisited = new HashSet<Point>();
    var group = new HashSet<Point>();
    var queue = new Queue<Point>();
    foreach (var start in EnumAllPoints())
    {
      if (!allVisited.Contains(start))
      {
        var ch = this[start];
        group.Clear();
        group.Add(start);

        queue.Clear();
        queue.Enqueue(start);
        while (queue.TryDequeue(out var pt))
        {
          foreach (var next in pt.EnumSides().Where(n => !group.Contains(n)))
          {
            if (this[next] == ch)
            {
              group.Add(next);
              queue.Enqueue(next);
            }
          }
        }

        yield return (ch, group.ToImmutableList());

        allVisited.UnionWith(group);
      }
    }
  }

  private IEnumerable<Point> EnumAllPoints()
    => from x in Enumerable.Range(0, Width)
       from y in Enumerable.Range(0, Height)
       select new Point(x, y);

  public static Map Parse(IReadOnlyList<string> input)
  {
    var width = input.First().Length;
    var height = input.Count;
    return new(width, height, input.ToImmutableArray());
  }
}