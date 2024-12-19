using System.Collections.Immutable;

var (map, moves) = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var final = moves.Aggregate(map, (current, move) => current.MoveRobot(move));
var result1 = final.EnumBoxCoordinates().Sum();
Console.WriteLine($"Part 1 Result = {result1}");

static (Map map, List<Vector> moves) ParseInput(string[] input)
{
  var split = Array.IndexOf(input, "");
  var map = Map.Parse(input[0..split]);
  var moves = input[split..].SelectMany(ch => ch).Select(MoveCharToVector).ToList();
  return (map, moves);
}

static Vector MoveCharToVector(char ch)
  => ch switch
  {
    '<' => new Vector(-1, 0),
    '^' => new Vector(0, -1),
    '>' => new Vector(1, 0),
    'v' => new Vector(0, 1),
    _ => default
  };

record struct Vector(int X, int Y)
{
  public Vector Add(Vector other) => new Vector(X + other.X, Y + other.Y);
}

record Map(Vector Robot, ImmutableHashSet<Vector> Walls, ImmutableHashSet<Vector> Boxes)
{
  public Map MoveRobot(Vector dir)
  {
    var gap = FindGap(Robot, dir);
    if (gap.HasValue)
    {
      var robotNext = Robot.Add(dir);
      if (gap.Value == robotNext)
        return this with { Robot = robotNext };

      var builder = Boxes.ToBuilder();
      builder.Remove(robotNext);
      builder.Add(gap.Value);
      return this with { Robot = robotNext, Boxes = builder.ToImmutable() };
    }

    return this;
  }

  public IEnumerable<int> EnumBoxCoordinates()
    => Boxes.Select(b => b.X + (100 * b.Y));

  private Vector? FindGap(Vector start, Vector dir)
  {
    var current = start;
    while (true)
    {
      current = current.Add(dir);
      if (Walls.Contains(current))
        return default;
      else if (!Boxes.Contains(current))
        return current;
    }
  }

  public static Map Parse(IReadOnlyList<string> input)
  {
    Vector? robot = default;
    var walls = ImmutableHashSet.CreateBuilder<Vector>();
    var boxes = ImmutableHashSet.CreateBuilder<Vector>();

    using var e = input.GetEnumerator();
    for (var y = 0; y < input.Count; ++y)
    {
      var line = input[y];
      for (var x = 0; x < line.Length; ++x)
      {
        var pt = new Vector(x, y);
        switch (line[x])
        {
          case '#':
            walls.Add(pt);
            break;

          case 'O':
            boxes.Add(pt);
            break;

          case '@':
            robot = pt;
            break;
        }
      }
    }

    return new Map(robot ?? default, walls.ToImmutable(), boxes.ToImmutable());
  }
}