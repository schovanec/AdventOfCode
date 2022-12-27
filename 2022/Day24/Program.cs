using System.Collections.Immutable;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var (tripTime1, mapAfterTrip1) = FindPath(map);
Console.WriteLine($"Part 1 Resullt = {tripTime1}");

var (returnTime, mapAfterReturn) = FindPath(mapAfterTrip1, map.Goal, map.Start);
var (tripTime2, _) = FindPath(mapAfterReturn);
Console.WriteLine($"Part 2 Result = {tripTime1 + returnTime + tripTime2}");

static (int, Map) FindPath(Map initial, Point? startPos = default, Point? goalPos = default)
{
  var best = int.MaxValue;
  var start = startPos ?? initial.Start;
  var goal = goalPos ?? initial.Goal;
  Dictionary<int, Map> maps = new() { [0] = initial };
  HashSet<(int, Point)> visited = new();

  PriorityQueue<(int, Point), int> queue = new();
  queue.Enqueue((0, start), start.DistanceTo(goal));
  while (queue.TryDequeue(out var current, out var min))
  {
    var (time, pos) = current;
    if (time >= best || min >= best || visited.Contains(current))
      continue;

    if (pos == goal)
    {
      if (time < best)
        best = time;
    }
    else
    {
      visited.Add(current);

      var newTime = time + 1;
      var map = GetMapForTime(newTime);
      foreach (var next in map.GetValidMoves(pos))
      {
        var nextMin = newTime + next.DistanceTo(goal);
        if (nextMin < best && !visited.Contains((newTime, pos)))
          queue.Enqueue((newTime, next), nextMin);
      }
    }
  }

  return (best, GetMapForTime(best));

  Map GetMapForTime(int time)
  {
    if (maps.TryGetValue(time, out var result))
      return result;

    return maps[time] = GetMapForTime(time - 1).Step();
  }
}

record struct Point(int X, int Y)
{
  public IEnumerable<Point> EnumCardinalMoves()
  {
    var (x, y) = this;
    yield return new(x - 1, y);
    yield return new(x + 1, y);
    yield return new(x, y - 1);
    yield return new(x, y + 1);
  }

  public int DistanceTo(Point other)
    => Math.Abs(other.X - X) + Math.Abs(other.Y - Y);
}

record Blizzard(Point Location, char Direction)
{
  public int X => Location.X;

  public int Y => Location.Y;

  public Blizzard Step(int mapWidth, int mapHeight)
    => (Direction switch
    {
      '>' => this with { Location = new(X + 1, Y) },
      '<' => this with { Location = new(X - 1, Y) },
      '^' => this with { Location = new(X, Y - 1) },
      'v' => this with { Location = new(X, Y + 1) },
      _ => this
    }).Wrap(mapWidth, mapHeight);

  private Blizzard Wrap(int mapWidth, int mapHeight)
  {
    if (X >= mapWidth)
      return this with { Location = new(0, Y) };
    else if (X < 0)
      return this with { Location = new(mapWidth - 1, Y) };
    else if (Y >= mapHeight)
      return this with { Location = new(X, 0) };
    else if (Y < 0)
      return this with { Location = new(X, mapHeight - 1) };
    else
      return this;
  }
}

record Map(int Width, int Height, ImmutableArray<Blizzard> Blizzards, Point Start, Point Goal)
{
  private HashSet<Point> BlizzardLocations { get; init; } = GetBlizzardLocations(Blizzards);

  public Map Step()
  {
    var builder = Blizzards.ToBuilder();
    for (var i = 0; i < builder.Count; ++i)
      builder[i] = builder[i].Step(Width, Height);

    var newBlizzards = builder.ToImmutable();
    return this with
    {
      Blizzards = newBlizzards,
      BlizzardLocations = GetBlizzardLocations(newBlizzards)
    };
  }

  public IEnumerable<Point> GetValidMoves(Point location)
    => from p in location.EnumCardinalMoves().Concat(new[] { location })
       where IsValidLocation(p)
       select p;

  public bool IsValidLocation(Point pt)
     => (IsInBounds(pt) || pt == Start || pt == Goal)
     && !BlizzardLocations.Contains(pt);

  public bool IsInBounds(Point pt)
    => pt.X >= 0 && pt.X < Width
    && pt.Y >= 0 && pt.Y < Height;

  private static HashSet<Point> GetBlizzardLocations(IEnumerable<Blizzard> blizzards)
    => blizzards.Select(b => b.Location).ToHashSet();

  public static Map Parse(string[] input)
  {
    var start = new Point(input.First().IndexOf('.') - 1, -1);
    var goal = new Point(input.Last().IndexOf('.') - 1, input.Length - 2);

    var width = input.First().Length - 2;
    var height = input.Length - 2;

    var blizzards = ImmutableArray.CreateBuilder<Blizzard>();
    var inner = input[1..^1];
    for (var y = 0; y < inner.Length; ++y)
    {
      for (var x = 0; x < width; ++x)
      {
        var ch = inner[y][x + 1];
        if (ch != '.')
          blizzards.Add(new(new(x, y), ch));
      }
    }

    return new(width, height, blizzards.ToImmutable(), start, goal);
  }
}