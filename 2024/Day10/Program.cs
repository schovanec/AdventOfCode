using System.Collections.Immutable;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var result1 = map.EnumTrailHeads()
                 .Sum(h => CountSummits(map, h));
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = map.EnumTrailHeads()
                 .Sum(h => CountTrails(map, h));
Console.WriteLine($"Part 2 Result = {result2}");

static int CountSummits(Map map, Point start, int goal = 9)
{
  var visited = new HashSet<Point>();
  var queue = new Queue<Point>([start]);
  var goalCount = 0;

  while (queue.TryDequeue(out var pt))
  {
    if (map.GetHeight(pt) == goal)
      ++goalCount;

    foreach (var next in map.EnumNextSteps(pt))
    {
      if (!visited.Contains(next))
      {
        visited.Add(next);
        queue.Enqueue(next);
      }
    }
  }

  return goalCount;
}

static int CountTrails(Map map, Point start, int goal = 9)
{
  var queue = new Queue<Point>([start]);
  var goalCount = 0;

  while (queue.TryDequeue(out var pt))
  {
    if (map.GetHeight(pt) == goal)
      ++goalCount;

    foreach (var next in map.EnumNextSteps(pt))
      queue.Enqueue(next);
  }

  return goalCount;
}

record struct Point(int X, int Y);

record Map(ImmutableArray<int> Heights, int Width)
{
  public int Height = Heights.Length / Width;

  public int GetHeight(Point pt) => Heights[(pt.Y * Width) + pt.X];

  public IEnumerable<Point> EnumTrailHeads()
    => Heights.Index()
              .Where(x => x.Item == 0)
              .Select(x => new Point(x.Index % Width, x.Index / Width));

  public IEnumerable<Point> EnumNextSteps(Point pt)
  {
    var h = GetHeight(pt);
    return EnumAdjacent(pt).Where(pt => GetHeight(pt) == h + 1);
  }

  public static Map Parse(IReadOnlyList<string> input)
  {
    var width = input.First().Length;

    var data = ImmutableArray.CreateBuilder<int>();
    foreach (var line in input)
      data.AddRange(line.Select(ch => ch - '0'));

    return new(data.ToImmutable(), width);
  }

  private IEnumerable<Point> EnumAdjacent(Point pt)
    => from i in Enumerable.Range(-1, 3)
       from j in Enumerable.Range(-1, 3)
       where i != j && (i == 0 || j == 0)
       let adj = new Point(pt.X + i, pt.Y + j)
       where adj.X >= 0 && adj.X < Width
          && adj.Y >= 0 && adj.Y < Height
       select adj;
}