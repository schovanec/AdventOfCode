using System.Collections.Immutable;

var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(line => line.Split(' ', 2))
                .Select(x => (direction: x[0][0], count: int.Parse(x[1])))
                .ToList();

var result1 = CountTailPositions(input);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = CountTailPositions(input, 10);
Console.WriteLine($"Part 2 Result = {result2}");

static int CountTailPositions(IEnumerable<(char direction, int count)> input, int knots = 2)
{
  var moves = from cmd in input
              from d in Enumerable.Repeat(cmd.direction, cmd.count)
              select d;

  var current = Rope.Create(knots);
  HashSet<Point> tailPositions = new() { current.Tail };
  foreach (var direction in moves)
  {
    current = current.Move(direction);
    tailPositions.Add(current.Tail);
  }

  return tailPositions.Count;
}

record struct Point(int X = 0, int Y = 0)
{
  public Point Move(char direction)
    => direction switch
    {
      'L' => this with { X = X - 1 },
      'R' => this with { X = X + 1 },
      'U' => this with { Y = Y - 1 },
      'D' => this with { Y = Y + 1 },
      _ => this
    };

  public Point Follow(Point head)
  {
    var xdist = X - head.X;
    var xabs = Math.Abs(xdist);

    var ydist = Y - head.Y;
    var yabs = Math.Abs(ydist);

    if (xabs <= 1 && yabs <= 1)
      return this;
    else if (xdist == 0 && Math.Abs(ydist) > 1)
      return this with { Y = Y - Math.Sign(ydist) };
    else if (ydist == 0 && Math.Abs(xdist) > 1)
      return this with { X = X - Math.Sign(xdist) };
    else
      return new(X - Math.Sign(xdist), Y - Math.Sign(ydist));
  }
}

record struct Rope(ImmutableArray<Point> Knots)
{
  public Point Tail => Knots.Last();

  public Rope Move(char direction)
  {
    var result = Knots.ToBuilder();
    result[0] = result[0].Move(direction);
    for (var i = 1; i < result.Count; ++i)
      result[i] = result[i].Follow(result[i - 1]);

    return new(result.ToImmutable());
  }

  public static Rope Create(int length)
    => new(Enumerable.Repeat(new Point(), length).ToImmutableArray());
}