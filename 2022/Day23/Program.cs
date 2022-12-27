var input = ParseInput(File.ReadLines(args.FirstOrDefault() ?? "input.txt")).ToList();

var stateAfter10 = DoRounds(input).Skip(9).First();
var occupied1 = stateAfter10.Select(e => e.Position).ToHashSet();
var result1 = Rectangle.FromPoints(occupied1)
                       .EnumAllPoints()
                       .Except(occupied1)
                       .Count();
Console.WriteLine($"Part 1 Result = {result1}");

var roundsUntilStopped = DoRounds(stateAfter10, 10).Count() + 11;
Console.WriteLine($"Part 2 Result = {roundsUntilStopped}");

static IEnumerable<List<Elf>> DoRounds(List<Elf> start, int firstRound = 0)
{
  var current = start;
  var count = firstRound;
  while (TryDoOneRound(current, count, out current))
  {
    yield return current;
    ++count;
  }
}

static bool TryDoOneRound(List<Elf> elves, int round, out List<Elf> after)
{
  var occupied = elves.Select(e => e.Position).ToHashSet();

  var moves = (from e in elves
               where e.Position.EnumAdjacent().Any(occupied.Contains)
               let move = e.EnumMoves(round)
                           .Where(m => !m.EnumProtectedLocations().Any(occupied.Contains))
                           .Select(x => (Move?)x)
                           .FirstOrDefault()
               where move.HasValue
               group (e, move) by move.Value.Destination into g
               where g.Count() == 1
               from x in g
               select x).ToDictionary(x => x.e.Id, x => x.move);

  if (moves.Count == 0)
  {
    after = elves;
    return false;
  }

  after = elves.Select(e => e.ApplyMove(moves.GetValueOrDefault(e.Id)))
               .ToList();
  return true;
}

static IEnumerable<Elf> ParseInput(IEnumerable<string> input)
{
  var id = 0;
  var y = 0;
  foreach (var line in input)
  {
    var last = -1;
    while (true)
    {
      var pos = line.IndexOf('#', last + 1);
      if (pos < 0)
        break;

      yield return new Elf(id++, new(pos, y));
      last = pos;
    }

    ++y;
  }
}

record struct Point(int X, int Y)
{
  public Point Add(Point other) => new(X + other.X, Y + other.Y);

  public IEnumerable<Point> EnumAdjacent()
  {
    var (x, y) = this;
    return from i in Enumerable.Range(-1, 3)
           from j in Enumerable.Range(-1, 3)
           where i != 0 || j != 0
           select new Point(x + i, y + j);
  }
}

record struct Rectangle(Point Min, Point Max)
{
  public static readonly Rectangle Empty = new(
    new(int.MaxValue, int.MaxValue),
    new(int.MinValue, int.MinValue));

  public bool IsEmpty => Min.X > Max.X || Min.Y > Max.Y;

  public int Width => IsEmpty ? 0 : Max.X - Min.X + 1;

  public int Height => IsEmpty ? 0 : Max.Y - Min.Y + 1;

  public IEnumerable<Point> EnumAllPoints()
  {
    var minY = Min.Y;
    var height = Height;
    return from x in Enumerable.Range(Min.X, Width)
           from y in Enumerable.Range(minY, height)
           select new Point(x, y);
  }

  public static Rectangle FromPoints(IEnumerable<Point> points)
    => points.Aggregate(Empty,
      (b, p) => new Rectangle(
        new Point(
          Math.Min(b.Min.X, p.X),
          Math.Min(b.Min.Y, p.Y)),
        new Point(
          Math.Max(b.Max.X, p.X),
          Math.Max(b.Max.Y, p.Y))));
}

record Elf(int Id, Point Position)
{
  public IEnumerable<Move> EnumMoves(int round)
  {
    var offset = round % moves.Count;
    return from i in Enumerable.Range(0, 4)
           let direction = (Direction)((i + offset) % moves.Count)
           select new Move(direction, Position);
  }

  public Elf ApplyMove(Move? move)
    => move is Move dest ? this with { Position = dest.Destination } : this;

  private static readonly IReadOnlyList<Direction> moves = Enum.GetValues<Direction>();
}

record struct Move(Direction Direction, Point Start)
{
  public Point Offset
    => Direction switch
    {
      Direction.North => new(0, -1),
      Direction.South => new(0, 1),
      Direction.West => new(-1, 0),
      Direction.East => new(1, 0),
      _ => default
    };

  public Point Destination => Start.Add(Offset);

  public IEnumerable<Point> EnumProtectedLocations()
  {
    var (x, y) = Offset;
    var offsets = y == 0
      ? Enumerable.Range(-1, 3).Select(i => new Point(x, i))
      : Enumerable.Range(-1, 3).Select(i => new Point(i, y));

    var start = Start;
    return offsets.Select(p => start.Add(p));
  }
}

enum Direction { North, South, West, East }