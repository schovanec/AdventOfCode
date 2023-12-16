using System.Collections.Immutable;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var visited = FindVisitedPoints(map, new(-1, 0), Direction.Right);
var result1 = visited.Count();
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = map.EnumStartPositions().AsParallel()
                 .Max(p => FindVisitedPoints(map, p.pos, p.dir).Count());
Console.WriteLine($"Part 2 Result = {result2}");

static IEnumerable<Point> FindVisitedPoints(Map map, Point start, Direction direction)
{
  HashSet<(Point pos, Direction dir)> visited = new();
  Queue<(Point pos, Direction dir)> queue = new([(start, direction)]);

  while (queue.Count > 0)
  {
    var (pos, dir) = queue.Dequeue();
    var next = pos.Move(dir);
    if (!visited.Contains((next, dir)) && next.X >= 0 && next.Y >= 0 && next.X < map.Width && next.Y < map.Height)
    {
      visited.Add((next, dir));

      if (map.Devices.TryGetValue(next, out var device))
      {
        foreach (var nextDir in device.EnumOutDirections(dir))
          queue.Enqueue((next, nextDir));
      }
      else
      {
        queue.Enqueue((next, dir));
      }
    }
  }

  return visited.Select(x => x.pos).Distinct();
}

enum Direction { Up, Right, Down, Left }

record struct Point(int X, int Y)
{
  public Point Move(Direction direction)
    => direction switch
    {
      Direction.Up => new(X, Y - 1),
      Direction.Right => new(X + 1, Y),
      Direction.Down => new(X, Y + 1),
      Direction.Left => new(X - 1, Y),
      _ => this
    };
}

record Device(Point Position, char Type)
{
  public IEnumerable<Direction> EnumOutDirections(Direction incoming)
    => (Type, incoming) switch
    {
      ('|', Direction.Left or Direction.Right) => [Direction.Up, Direction.Down],
      ('|', _) => [incoming],
      ('-', Direction.Down or Direction.Up) => [Direction.Left, Direction.Right],
      ('-', _) => [incoming],
      ('\\', Direction.Up) => [Direction.Left],
      ('\\', Direction.Right) => [Direction.Down],
      ('\\', Direction.Down) => [Direction.Right],
      ('\\', Direction.Left) => [Direction.Up],
      ('/', Direction.Up) => [Direction.Right],
      ('/', Direction.Right) => [Direction.Up],
      ('/', Direction.Down) => [Direction.Left],
      ('/', Direction.Left) => [Direction.Down],
      _ => []
    };
}

record Map(int Width, int Height, ImmutableDictionary<Point, Device> Devices)
{
  public IEnumerable<(Point pos, Direction dir)> EnumStartPositions()
  {
    for (var y = 0; y < Height; ++y)
    {
      yield return (new(-1, y), Direction.Right);
      yield return (new(Width, y), Direction.Left);
    }

    for (var x = 0; x < Width; ++x)
    {
      yield return (new(x, -1), Direction.Down);
      yield return (new(x, Height), Direction.Up);
    }
  }

  public static Map Parse(string[] input)
  {
    var width = input[0].Length;
    var height = input.Length;

    var devices = (from y in Enumerable.Range(0, input.Length)
                   let line = input[y]
                   from x in Enumerable.Range(0, line.Length)
                   let ch = line[x]
                   where ch != '.'
                   select new Device(new(x, y), ch)).ToImmutableDictionary(x => x.Position);

    return new(width, height, devices);
  }
}