var map = new Map(File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                      .Select(line => line.Select(ch => ch - '0').ToArray())
                      .ToArray());

var result1 = FindBestPath(map);
Console.WriteLine($"Part 1 Result = {result1}");

static int FindBestPath(Map map)
{
  Dictionary<Node, int> dist = new() { [new(0, 0, Direction.South, 0)] = 0 };
  Dictionary<Node, Node> prev = new();
  PriorityQueue<Node, int> queue = new([(new(0, 0, Direction.South, 0), 0)]);

  while (queue.TryDequeue(out var u, out var uCost))
  {
    //if (uCost > dist[u])
    //  continue;

    if (u.X == map.Width - 1 && u.Y == map.Height - 1)
    {
      // List<(int X, int Y)> path = [(u.X, u.Y)];
      // var current = u;
      // while (prev.TryGetValue(current, out var p))
      // {
      //   path.Insert(0, (p.X, p.Y));
      //   current = p;
      // }
      // Console.WriteLine(string.Join(" => ", path));
      return dist[u];
    }

    foreach (var next in map.EnumMovesFrom(u))
    {
      var cost = dist[u] + next.cost;
      if (cost < dist.GetValueOrDefault(next.node, int.MaxValue))
      {
        dist[next.node] = cost;
        prev[next.node] = u;
        queue.Enqueue(next.node, cost);
      }
    }
  }

  return 0;
}

enum Direction { North, East, South, West }

record struct Node(int X, int Y, Direction Entered, int Steps)
{
  public Node Move(Direction direction)
  {
    var (newX, newY) = direction switch
    {
      Direction.North => (X, Y - 1),
      Direction.East => (X + 1, Y),
      Direction.South => (X, Y + 1),
      Direction.West => (X - 1, Y),
      _ => (X, Y)
    };

    var newSteps = (direction == Entered) ? Steps + 1 : 1;

    return new(newX, newY, direction, newSteps);
  }
}

record Map(int[][] HeatLoss)
{
  public int Width = HeatLoss?.FirstOrDefault()?.Length ?? 0;

  public int Height = HeatLoss?.Length ?? 0;

  public IEnumerable<(Node node, int cost)> EnumMovesFrom(Node current, int directionLimit = 3)
    => from dir in EnumDirections()
       where dir != GetReverse(current.Entered)
       let n = current.Move(dir)
       where n.X >= 0
          && n.Y >= 0
          && n.X < Width
          && n.Y < Height
          && n.Steps <= directionLimit
       let cost = HeatLoss[n.Y][n.X]
       select (n, cost);

  private static IEnumerable<Direction> EnumDirections()
    => [Direction.North, Direction.East, Direction.South, Direction.West];

  private static Direction GetReverse(Direction dir)
    => dir switch
    {
      Direction.North => Direction.South,
      Direction.East => Direction.West,
      Direction.South => Direction.North,
      Direction.West => Direction.East,
      _ => throw new ArgumentOutOfRangeException(nameof(dir))
    };
}