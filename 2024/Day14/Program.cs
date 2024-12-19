using System.Collections.Immutable;

var robots = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                 .Select(Robot.Parse)
                 .ToImmutableList();

var size = Vector.Parse(args.Skip(1).FirstOrDefault() ?? "101,103");

var after1 = MoveRobots(robots, size);
var result1 = CountQuadrants(after1, size).Aggregate((a, b) => a * b);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = CountSecondsToPicture(robots, size);
Console.WriteLine($"Part 2 Result = {result2}");

static ImmutableList<Robot> MoveRobots(ImmutableList<Robot> robots, Vector size, int n = 100)
{
  return robots.Select(r => r.Move(size, n))
               .ToImmutableList();
}

static IEnumerable<int> CountQuadrants(ImmutableList<Robot> robots, Vector size)
{
  var quadrants = EnumQuadrants(size).ToList();

  var result = quadrants
    .Select(q => robots.Count(r => r.IsInBounds(q.start, q.end)))
    .ToList();

  return result;
}

static IEnumerable<(Vector start, Vector end)> EnumQuadrants(Vector size)
{
  var midX = size.X / 2;
  var midY = size.Y / 2;
  yield return (new(0, 0), new(midX, midY));
  yield return (new(midX + 1, 0), new(size.X, midY));
  yield return (new(midX + 1, midY + 1), new(size.X, size.Y));
  yield return (new(0, midY + 1), new(midX, size.Y));
}

static int CountSecondsToPicture(ImmutableList<Robot> robots, Vector size)
{
  var current = robots;
  var n = 0;
  while (true)
  {
    ++n;
    current = MoveRobots(current, size, 1);

    if (current.Select(r => r.Position).Distinct().Count() == current.Count)
      return n;
  }
}

record struct Vector(int X, int Y)
{
  public static Vector Parse(string input)
  {
    var parts = input.Split(',', 2, StringSplitOptions.TrimEntries);
    var x = int.Parse(parts[0]);
    var y = int.Parse(parts[1]);
    return new Vector(x, y);
  }
}

record Robot(Vector Position, Vector Velocity)
{
  public Robot Move(Vector size, int n)
  {
    var x = (((Position.X + (Velocity.X * n)) % size.X) + size.X) % size.X;
    var y = (((Position.Y + (Velocity.Y * n)) % size.Y) + size.Y) % size.Y;
    return this with { Position = new Vector(x, y) };
  }

  public bool IsInBounds(Vector start, Vector end)
    => Position.X >= start.X
    && Position.Y >= start.Y
    && Position.X < end.X
    && Position.Y < end.Y;

  public static Robot Parse(string input)
  {
    var parts = input.Split(' ', 2, StringSplitOptions.TrimEntries);
    var pos = Vector.Parse(parts[0].Split('=', 2, StringSplitOptions.TrimEntries)[1]);
    var velocity = Vector.Parse(parts[1].Split('=', 2, StringSplitOptions.TrimEntries)[1]);
    return new Robot(pos, velocity);
  }
}