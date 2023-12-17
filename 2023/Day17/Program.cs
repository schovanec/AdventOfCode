var map = new Map(File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                      .Select(line => line.Select(ch => ch - '0').ToArray())
                      .ToArray());

var result1 = FindBestPath(map);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = FindBestPath(map, 4, 10);
Console.WriteLine($"Part 2 Result = {result2}");

static int FindBestPath(Map map, int directionMin = 1, int directionMax = 3)
{
  Dictionary<Node, int> dist = new() { [new(0, 0, Direction.South, -1)] = 0 };
  PriorityQueue<Node, int> queue = new([(new(0, 0, Direction.South, -1), 0)]);

  while (queue.TryDequeue(out var u, out var uCost))
  {
    if (u.X == map.Width - 1 && u.Y == map.Height - 1)
      return dist[u];

    foreach (var next in map.EnumMovesFrom(u, directionMin, directionMax))
    {
      var cost = dist[u] + next.cost;
      if (cost < dist.GetValueOrDefault(next.node, int.MaxValue))
      {
        dist[next.node] = cost;
        queue.Enqueue(next.node, cost);
      }
    }
  }

  return 0;
}

enum Direction { North, East, South, West }

record struct Node(int X, int Y, Direction Entered, int Steps)
{
  public Node Move(Direction direction, int steps = 1)
  {
    var (newX, newY) = direction switch
    {
      Direction.North => (X, Y - steps),
      Direction.East => (X + steps, Y),
      Direction.South => (X, Y + steps),
      Direction.West => (X - steps, Y),
      _ => (X, Y)
    };

    var newSteps = (direction == Entered) ? Steps + steps : 1;

    return new(newX, newY, direction, newSteps);
  }
}

record Map(int[][] HeatLoss)
{
  public int Width = HeatLoss?.FirstOrDefault()?.Length ?? 0;

  public int Height = HeatLoss?.Length ?? 0;

  public IEnumerable<(Node node, int cost)> EnumMovesFrom(Node current, int directionMin, int directionMax)
    => from dir in EnumDirections()
       where (current.Steps < 0 || current.Steps >= directionMin || current.Entered == dir)
          && dir != GetReverse(current.Entered)
       let n = current.Move(dir)
       where IsInMap(n) && n.Steps <= directionMax
       let minStepsInDir = Math.Max(0, directionMin - n.Steps)
       where minStepsInDir <= 0 || IsInMap(n.Move(dir, Math.Max(0, directionMin - n.Steps)))
       let cost = HeatLoss[n.Y][n.X]
       select (n, cost);

  private bool IsInMap(Node node)
    => node.X >= 0 && node.X < Width
    && node.Y >= 0 && node.Y < Height;

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