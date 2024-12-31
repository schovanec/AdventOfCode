using System.Collections.Immutable;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

TryFindShortestPath(map, out var result1, out var parentLookup);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = FindAllVisitedNodes(map.Goal, result1, parentLookup);
Console.WriteLine($"Part 2 Result = {result2}");

static bool TryFindShortestPath(
  Map map,
  out int shortestPathLength,
  out ILookup<(Vector pos, int cost), (Vector prevPos, int prevCost)> parentLookup)
{
  var parents = new HashSet<(Vector pos, int cost, Vector prevPos, int prevCost)>();
  int? result = default;
  var startNode = new Node(map.Start, Direction.East);
  var visited = new HashSet<Node>();

  var queue = new PriorityQueue<Node, int>();
  queue.Enqueue(startNode, 0);

  while (queue.TryDequeue(out var node, out var pathCost))
  {
    if (pathCost > result)
      break;

    if (visited.Contains(node))
      continue;

    visited.Add(node);

    if (node.Position == map.Goal)
    {
      if (!result.HasValue)
        result = pathCost;
    }
    else
    {
      foreach (var (next, weight) in EnumMoves(node, map).Where(m => !visited.Contains(m.node)))
      {
        var n = pathCost + weight;
        parents.Add((next.Position, n, node.Position, pathCost));
        queue.Enqueue(next, n);
      }
    }
  }

  shortestPathLength = result ?? int.MinValue;
  parentLookup = parents.ToLookup(x => (x.pos, x.cost), x => (x.prevPos, x.prevCost));
  return result.HasValue;
}

static int FindAllVisitedNodes(
  Vector goal,
  int goalPathLength,
  ILookup<(Vector pos, int cost), (Vector prevPos, int prevCost)> parentLookup)
{
  var visited = new HashSet<Vector>();
  var queue = new Queue<(Vector pos, int cost)>([(goal, goalPathLength)]);
  while (queue.TryDequeue(out var node))
  {
    visited.Add(node.pos);

    foreach (var parent in parentLookup[node])
      queue.Enqueue(parent);
  }

  return visited.Count;
}

static IEnumerable<(Node node, int cost)> EnumMoves(Node node, Map map)
{
  Node next;

  next = node.Step();
  if (!map.IsWall(next.Position))
    yield return (next, 1);

  next = node.RotateRight().Step();
  if (!map.IsWall(next.Position))
    yield return (next, 1001);

  next = node.RotateLeft().Step();
  if (!map.IsWall(next.Position))
    yield return (next, 1001);

  next = node.RotateLeft().RotateLeft().Step();
  if (!map.IsWall(next.Position))
    yield return (next, 2001);
}

enum Direction { North, East, South, West };

record Node(Vector Position, Direction Direction)
{
  public Node Step()
    => this with { Position = Position.Move(Direction) };

  public Node RotateRight()
    => this with { Direction = RotateDirectionRight(Direction) };

  public Node RotateLeft()
    => this with { Direction = RotateDirectionLeft(Direction) };

  static Direction RotateDirectionRight(Direction direction) => (Direction)(((int)direction + 1) % 4);

  static Direction RotateDirectionLeft(Direction direction) => (Direction)(((int)direction - 1 + 4) % 4);
}

record struct Vector(int X, int Y)
{
  public Vector Move(Direction direction)
    => direction switch
    {
      Direction.North => new(X, Y - 1),
      Direction.East => new(X + 1, Y),
      Direction.South => new(X, Y + 1),
      Direction.West => new(X - 1, Y),
      _ => this
    };
}

record Map(ImmutableHashSet<Vector> Walls, Vector Start, Vector Goal)
{
  public bool IsWall(Vector pos) => Walls.Contains(pos);

  public static Map Parse(IReadOnlyList<string> input)
  {
    Vector start = default;
    Vector goal = default;
    var walls = ImmutableHashSet.CreateBuilder<Vector>();

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

          case 'S':
            start = pt;
            break;

          case 'E':
            goal = pt;
            break;
        }
      }
    }

    return new Map(walls.ToImmutable(), start, goal);
  }
}
