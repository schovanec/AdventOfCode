var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(line => line.Split(' ', 2))
                .Select(x => (direction: x[0][0], count: int.Parse(x[1])))
                .ToList();

var moves = from cmd in input
            from d in Enumerable.Repeat(cmd.direction, cmd.count)
            select d;

Rope current = new();
HashSet<Point> tailPositions = new();
tailPositions.Add(current.Tail);
foreach (var direction in moves)
{
  current = current.Move(direction);
  tailPositions.Add(current.Tail);
}

Console.WriteLine($"Part 1 Result = {tailPositions.Count}");

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

  public override string ToString() => $"({X}, {Y})";
}

record struct Rope(Point Head = default, Point Tail = default)
{
  public bool IsTailAdjacent
    => Math.Abs(Head.X - Tail.X) <= 1 && Math.Abs(Head.Y - Tail.Y) <= 1;

  public Rope Move(char direction)
    => (this with { Head = Head.Move(direction) }).FixTail();

  public Rope FixTail()
  {
    var (head, tail) = this;

    while (true)
    {
      var xdist = tail.X - head.X;
      var xabs = Math.Abs(xdist);

      var ydist = tail.Y - head.Y;
      var yabs = Math.Abs(ydist);

      if (xabs <= 1 && yabs <= 1)
        break;

      if (xdist == 0 && Math.Abs(ydist) > 1)
      {
        tail = tail with { Y = tail.Y - Math.Sign(ydist) };
      }
      else if (ydist == 0 && Math.Abs(xdist) > 1)
      {
        tail = tail with { X = tail.X - Math.Sign(xdist) };
      }
      else
      {
        tail = new(tail.X - Math.Sign(xdist), tail.Y - Math.Sign(ydist));
      }
    }

    return new(head, tail);
  }

  public override string ToString() => $"[{Head}, {Tail}]";
}