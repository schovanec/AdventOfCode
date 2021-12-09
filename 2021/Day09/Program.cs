var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .SelectMany((line, y) => line.Select((ch, x) => (x: x, y: y, v: ch - '0')))
                .ToDictionary(pt => new Point(pt.x, pt.y), pt => pt.v);

var lowest = input.Where(x => EnumAdjacentPoints(x.Key, input).All(a => a.height > x.Value))
                  .Select(x => (location: x.Key, height: x.Value));

var riskLevelSum = lowest.Sum(p => p.height + 1);

Console.WriteLine($"Part 1 Result = {riskLevelSum}");

var basinSizes = lowest.Select(p => FindBasinSize(p.location, input))
                       .OrderByDescending(n => n);

var topThreeProduct = basinSizes.Take(3)
                                .Aggregate((a, b) => a * b);

Console.WriteLine($"Part 2 Result = {topThreeProduct}");

int FindBasinSize(Point lowPoint, IDictionary<Point, int> heights)
{
  var seen = new HashSet<Point> { lowPoint };
  var queue = new Queue<Point>();
  queue.Enqueue(lowPoint);

  var count = 0;
  while (queue.Count > 0)
  {
    var current = queue.Dequeue();
    ++count;

    var adjacent = EnumAdjacentPoints(current, heights)
      .Where(a => a.height < 9 && !seen.Contains(a.location))
      .Select(a => a.location);

    foreach (var pt in adjacent)
    {
      seen.Add(pt);
      queue.Enqueue(pt);
    }
  }

  return count;
}

IEnumerable<(Point location, int height)> EnumAdjacentPoints(Point pt, IDictionary<Point, int> heights)
  => pt.EnumAdjacent()
       .Where(a => heights.ContainsKey(a))
       .Select(a => (a, heights[a]));

record struct Point(int X, int Y)
{
  public IEnumerable<Point> EnumAdjacent()
  {
    yield return new Point(X - 1, Y);
    yield return new Point(X + 1, Y);
    yield return new Point(X, Y - 1);
    yield return new Point(X, Y + 1);
  }
}