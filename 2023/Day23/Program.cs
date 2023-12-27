
using System.Collections.Immutable;

var map = new Map(File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                      .ToImmutableArray());

Part1();
Part2();

void Part1()
{
  var result1 = FindPathLengths(
    map.GetStart(),
    map.GetEnd(),
    pt => map.EnumMoves(pt).Select(n => (n, 1))).Max();

  Console.WriteLine($"Part 1 Result = {result1}");
}

void Part2()
{
  var graph = BuildSimplifiedGraph(map);

  var result2 = FindPathLengths(
    map.GetStart(),
    map.GetEnd(),
    pt => graph[pt]).Max();

  Console.WriteLine($"Part 2 Result = {result2}");
}

static IEnumerable<int> FindPathLengths(Point start, Point goal, Func<Point, IEnumerable<(Point pt, int steps)>> edges)
{
  Stack<(Point, ImmutableHashSet<Point>, int)> stack = new([(start, [start], 0)]);

  while (stack.Count > 0)
  {
    var (pt, visited, steps) = stack.Pop();
    foreach (var next in edges(pt).Where(next => !visited.Contains(next.pt)))
    {
      if (next.pt == goal)
        yield return steps + next.steps;
      else
        stack.Push((next.pt, visited.Add(next.pt), steps + next.steps));
    }
  }
}

static ILookup<Point, (Point to, int size)> BuildSimplifiedGraph(Map map)
{
  var start = map.GetStart();
  var goal = map.GetEnd();
  HashSet<(Point from, Point to, int size)> edges = new();
  HashSet<(Point, Point)> visited = [];
  Stack<(Point start, Point next, int steps, ImmutableHashSet<Point>)> stack = new();

  foreach (var m in map.EnumMoves(start, true))
  {
    visited.Add((start, m));
    stack.Push((start, m, 1, [start]));
  }

  while (stack.Count > 0)
  {
    var (from, pt, steps, path) = stack.Pop();

    if (pt == goal)
    {
      edges.Add((from, pt, steps));
      edges.Add((pt, from, steps));
    }
    else
    {
      var moves = map.EnumMoves(pt, true)
                     .Where(n => !path.Contains(n))
                     .ToArray();

      if (moves.Length == 1)
      {
        stack.Push((from, moves[0], steps + 1, path.Add(pt)));
      }
      else if (moves.Length > 0)
      {
        edges.Add((from, pt, steps));
        edges.Add((pt, from, steps));

        foreach (var m in moves)
        {
          if (!visited.Contains((pt, m)))
          {
            visited.Add((pt, m));
            stack.Push((pt, m, 1, [pt]));
          }
        }
      }
    }
  }

  return edges.ToLookup(x => x.from, x => (x.to, x.size));
}


record struct Point(int X, int Y)
{
  public IEnumerable<Point> EnumAdjacent()
    => [new(X - 1, Y), new(X + 1, Y), new(X, Y - 1), new(X, Y + 1)];
}

record Map(ImmutableArray<string> Data)
{
  public int Width = Data.FirstOrDefault()?.Length ?? 0;

  public int Height = Data.Length;

  public char this[Point pt]
    => Contains(pt) ? Data[pt.Y][pt.X] : default;

  public bool Contains(Point pt)
    => pt.X >= 0 && pt.X < Width
    && pt.Y >= 0 && pt.Y < Height;

  public bool IsWalkable(Point pt)
    => this[pt] is '.' or '^' or '>' or 'v' or '<';

  public bool IsBadMove(Point from, Point to)
    => (to.X - from.X, to.Y - from.Y) switch
    {
      (1, 0) => this[to] != '<',
      (-1, 0) => this[to] != '>',
      (0, 1) => this[to] != '<',
      (0, -1) => this[to] != '^',
      _ => false
    };

  public Point GetStart() => new(Data[0].IndexOf('.'), 0);

  public Point GetEnd() => new(Data.Last().IndexOf('.'), Data.Length - 1);

  public IEnumerable<Point> EnumMoves(Point pt, bool slopesAsPaths = false)
  {
    return from to in EnumMovesInternal(pt, slopesAsPaths)
           where IsWalkable(to) && (slopesAsPaths || !IsBadMove(pt, pt))
           select to;
  }

  private IEnumerable<Point> EnumMovesInternal(Point pt, bool slopesAsPaths = false)
    => slopesAsPaths
      ? pt.EnumAdjacent()
      : this[pt] switch
      {
        '.' => pt.EnumAdjacent(),
        '^' => [new(pt.X, pt.Y - 1)],
        '>' => [new(pt.X + 1, pt.Y)],
        'v' => [new(pt.X, pt.Y + 1)],
        '<' => [new(pt.X - 1, pt.Y)],
        _ => []
      };
}