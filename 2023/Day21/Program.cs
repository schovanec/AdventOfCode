using System.Collections.Immutable;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault() ?? /*@"..\..\..\sample1.txt"));*/ "input.txt"));

Part1();
Part2();

void Part1()
{
  var stepsGoal = args.Skip(1)
                      .Select(int.Parse)
                      .FirstOrDefault(64);

  var result1 = CountReachable(map, stepsGoal);
  Console.WriteLine($"Part 1 Result = {result1}");
}

void Part2()
{
  const long largeStepsGoal = 26501365L;
  const int a = 131;
  const int b = 65;
  const long k = (largeStepsGoal - b) / a;

  Console.WriteLine($"{largeStepsGoal} = {a}*k + {b} => k = {k}");

  var samples = FindReachable(map, (4 * a) + b, true);
  var points = (from i in Enumerable.Range(0, 5)
                where i % 2 == 0
                let goal = (i * a) + b
                select (n: (long)i, r: (long)samples[goal])).ToArray();

  Console.WriteLine($"Samples: [{string.Join("; ", points)}]");

  var poly = Interpolate(points);

  Console.WriteLine($"Poly Terms: {string.Join(", ", poly)}");

  var result2 = poly[0] + (poly[1] * k) + (poly[2] * k * k);
  Console.WriteLine($"Part 2 Result = {result2}");
}

static int CountReachable(Map map, int goal)
  => FindReachable(map, goal).GetValueOrDefault(goal);

static Dictionary<int, int> FindReachable(Map map, int goal, bool infinite = false)
{
  HashSet<(Point pt, int dist)> visited = [(map.Start, 0)];
  Queue<(Point, int)> queue = new(visited);

  while (queue.Count > 0)
  {
    var (pt, dist) = queue.Dequeue();
    var adjacent = pt.EnumAdjacent()
                     .Select(pt => (pt, dist: dist + 1))
                     .Where(x => map.IsPlot(x.pt, infinite) && !visited.Contains(x));

    foreach (var next in adjacent)
    {
      visited.Add(next);
      if (next.dist < goal)
        queue.Enqueue(next);
    }
  }

  return visited.GroupBy(x => x.dist)
                .ToDictionary(g => g.Key, g => g.Count());
}

static long[] Interpolate((long k, long n)[] points)
{
  var num = points.Length;
  var poly = new long[num];

  for (var i = 0; i < num; ++i)
  {
    var prod = Enumerable.Range(0, num)
                         .Where(j => i != j)
                         .Aggregate(1L, (a, j) => a * (points[i].k - points[j].k));
    prod = points[i].n / prod;

    var term = new long[num];
    term[0] = prod;
    for (var j = 0; j < num; ++j)
    {
      if (i == j)
        continue;

      for (var k = num - 1; k > 0; k--)
      {
        term[k] += term[k - 1];
        term[k - 1] *= -points[j].k;
      }
    }

    for (var j = 0; j < num; ++j)
      poly[j] += term[j];
  }

  return poly;
}

record struct Point(int X, int Y)
{
  public IEnumerable<Point> EnumAdjacent()
    => [new(X - 1, Y), new(X + 1, Y), new(X, Y - 1), new(X, Y + 1)];
}

record Map(int Width, int Height, ImmutableHashSet<Point> Rocks, Point Start)
{
  public bool Contains(Point pt)
    => pt.X >= 0 && pt.X < Width
    && pt.Y >= 0 && pt.Y < Height;

  public bool IsRock(Point pt, bool infinite = false)
  => Rocks.Contains(infinite ? MapInfinite(pt) : pt);

  public bool IsPlot(Point pt, bool infinite = false)
    => (infinite || Contains(pt))
    && !IsRock(pt, infinite);

  public Point MapInfinite(Point pt)
    => new(
      pt.X < 0 ? ((pt.X % Width) + Width) % Width : pt.X % Width,
      pt.Y < 0 ? ((pt.Y % Height) + Height) % Height : pt.Y % Height);

  public static Map Parse(string[] input)
  {
    var height = input.Length;
    var width = input[0].Length;
    var start = default(Point);
    var rocks = ImmutableHashSet.CreateBuilder<Point>();
    for (var y = 0; y < height; ++y)
    {
      var line = input[y];

      for (var x = 0; x < width; ++x)
      {
        var ch = line[x];
        if (ch == '#')
          rocks.Add(new(x, y));
        else if (ch == 'S')
          start = new(x, y);
      }
    }

    return new(width, height, rocks.ToImmutable(), start);
  }
}