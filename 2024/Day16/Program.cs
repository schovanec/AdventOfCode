using System.Collections.Immutable;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")); //"..\\..\\..\\reddit_sample1.txt"));

var result1 = FindShortestPath(map);
Console.WriteLine($"Part 1 Result = {result1}");

static int FindShortestPath(Map map)
{
  var cost = new Dictionary<Vector, int> { [map.Start] = 0 };
  var visited = new HashSet<Vector>();
  var prev = new Dictionary<Vector, Vector>();

  var queue = new PriorityQueue<Node, int>();
  queue.Enqueue(new Node(map.Start, Direction.East), 0);

  while (queue.Count > 0)
  {
    var node = queue.Dequeue();
    var nodeCost = cost[node.Position];

    if (visited.Contains(node.Position))
      continue;

    visited.Add(node.Position);

    if (node.Position == map.Goal)
      return nodeCost;

    foreach (var (next, weight) in EnumMoves(node, map).Where(m => !visited.Contains(m.node.Position)))
    {
      var n = nodeCost + weight;
      if (n < cost.GetValueOrDefault(next.Position, int.MaxValue))
      {
        cost[next.Position] = n;
        prev[next.Position] = node.Position;
        queue.Enqueue(next, n);
      }
    }
  }

  return int.MaxValue;
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