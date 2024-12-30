using System.Collections.Immutable;

var (map, moves) = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var final1 = moves.Aggregate(map, (current, move) => current.MoveRobot(move));
var result1 = final1.EnumBoxCoordinates().Sum();
Console.WriteLine($"Part 1 Result = {result1}");

var otherMap = map.Transform();
var final2 = moves.Aggregate(otherMap, (current, move) => current.MoveRobot(move));
var result2 = final2.EnumBoxCoordinates().Sum();
Console.WriteLine($"Part 2 Result = {result2}");

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

record Box(Vector Position, int Width = 1)
{
  public IEnumerable<Vector> AllCoordinates
    => Enumerable.Range(0, Width).Select(i => Position.Add(new(i, 0)));

  public Box Move(Vector dir)
    => this with { Position = Position.Add(dir) };
}

record Map(Vector Robot, ImmutableHashSet<Vector> Walls, ImmutableDictionary<Vector, Box> Boxes)
{
  public Map Transform()
  {
    var newWalls = (from p in Walls
                    let x = p.X * 2
                    from i in Enumerable.Range(0, 2)
                    select new Vector(x + i, p.Y)).ToImmutableHashSet();

    var newBoxes = (from b in Boxes.Values.Distinct()
                    let c = new Box(b.Position with { X = b.Position.X * 2 }, 2)
                    from p in c.AllCoordinates
                    select (key: p, value: c)).ToImmutableDictionary(x => x.key, x => x.value);

    var newRobot = new Vector(Robot.X * 2, Robot.Y);

    return new Map(newRobot, newWalls, newBoxes);
  }

  public Map MoveRobot(Vector dir)
  {
    if (TryFindBoxesToMove(Robot, dir, out var boxes))
    {
      var robotNext = Robot.Add(dir);

      var builder = Boxes.ToBuilder();
      builder.RemoveRange(boxes.SelectMany(b => b.AllCoordinates));
      builder.AddRange(from b in boxes
                       let next = b.Move(dir)
                       from p in next.AllCoordinates
                       select KeyValuePair.Create(p, next));
      return this with { Robot = robotNext, Boxes = builder.ToImmutable() };
    }

    return this;
  }

  public IEnumerable<int> EnumBoxCoordinates()
    => Boxes.Values
            .Select(b => b.Position)
            .Distinct()
            .Select(b => b.X + (100 * b.Y));

  private bool TryFindBoxesToMove(Vector start, Vector dir, out ImmutableHashSet<Box> boxes)
  {
    boxes = ImmutableHashSet<Box>.Empty;

    var result = ImmutableHashSet.CreateBuilder<Box>();
    var queue = new Queue<Vector>([start]);
    while (queue.TryDequeue(out var pt))
    {
      var next = pt.Add(dir);

      if (Walls.Contains(next))
        return false;

      if (Boxes.TryGetValue(next, out var box) && !result.Contains(box))
      {
        result.Add(box);
        foreach (var p in box.AllCoordinates)
          queue.Enqueue(p);
      }
    }

    boxes = result.ToImmutable();
    return true;
  }

  public static Map Parse(IReadOnlyList<string> input)
  {
    Vector? robot = default;
    var walls = ImmutableHashSet.CreateBuilder<Vector>();
    var boxes = ImmutableDictionary.CreateBuilder<Vector, Box>();

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
            boxes.Add(pt, new Box(pt));
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