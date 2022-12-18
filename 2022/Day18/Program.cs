var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(Point.Parse)
                .ToHashSet();

var surfacePoints = input.SelectMany(p => p.GetAdjacentToFaces())
                         .Where(p => !input.Contains(p));
var area = surfacePoints.Count();
Console.WriteLine($"Part 1 Result = {area}");

var air = FindAir(input);
var exteriorArea = surfacePoints.Count(p => air.Contains(p));
Console.WriteLine($"Part 2 Result = {exteriorArea}");

static HashSet<Point> FindAir(HashSet<Point> shape)
{
  Box bounds = Box.FromPoints(shape).Grow();
  HashSet<Point> visited = new() { bounds.Min };
  Queue<Point> queue = new(visited);
  while (queue.Count > 0)
  {
    var current = queue.Dequeue();
    foreach (var pt in current.GetAdjacentToFaces().Where(pt => bounds.Contains(pt)))
    {
      if (!visited.Contains(pt) && !shape.Contains(pt))
      {
        visited.Add(pt);
        queue.Enqueue(pt);
      }
    }
  }

  return visited;
}

record struct Point(int X, int Y, int Z)
{
  public IEnumerable<Point> GetAdjacentToFaces()
  {
    for (var i = -1; i <= 1; i += 2)
    {
      yield return this with { X = X + i };
      yield return this with { Y = Y + i };
      yield return this with { Z = Z + i };
    }
  }

  public static Point Parse(string input)
  {
    var parts = input.Split(',', 3);
    return new(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
  }
}

record struct Box(Point Min, Point Max)
{
  private static readonly Box Empty = new(
    new Point(int.MaxValue, int.MaxValue, int.MaxValue),
    new Point(int.MinValue, int.MinValue, int.MinValue));

  public Box Grow(int amount = 1)
    => new(new(Min.X - amount, Min.Y - amount, Min.Z - amount),
           new(Max.X + amount, Max.Y + amount, Max.Z + amount));

  public bool Contains(Point pt)
    => pt.X >= Min.X && pt.X <= Max.X
    && pt.Y >= Min.Y && pt.Y <= Max.Y
    && pt.Z >= Min.Z && pt.Z <= Max.Z;

  public static Box FromPoints(IEnumerable<Point> points)
    => points.Aggregate(Empty,
      (b, p) => new Box(
        new Point(
          Math.Min(b.Min.X, p.X),
          Math.Min(b.Min.Y, p.Y),
          Math.Min(b.Min.Z, p.Z)),
        new Point(
          Math.Max(b.Max.X, p.X),
          Math.Max(b.Max.Y, p.Y),
          Math.Max(b.Max.Z, p.Z))));
}