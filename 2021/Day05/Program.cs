var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(Line.Parse)
                .ToList();

var overlapCount1 = CountOverlappingPoints(input.Where(x => !x.IsDiagonal));
Console.WriteLine($"Part 1 Result = {overlapCount1}");

var overlapCount2 = CountOverlappingPoints(input);
Console.WriteLine($"Part 2 Result = {overlapCount2}");

int CountOverlappingPoints(IEnumerable<Line> lines)
  => lines.SelectMany(x => x.EnumAllPoints())
          .GroupBy(pt => pt)
          .Count(g => g.Count() > 1);

record struct Point(int X, int Y)
{
  public static Point Parse(string input)
  {
    var coords = input.Split(',');
    return new Point(int.Parse(coords[0]), int.Parse(coords[1]));
  }
}

record Line(Point Start, Point End)
{
  public IEnumerable<Point> EnumAllPoints()
  {
    var deltaX = End.X - Start.X;
    var deltaY = End.Y - Start.Y;
    var stepX = Math.Sign(deltaX);
    var stepY = Math.Sign(deltaY);
    var length = Math.Max(Math.Abs(deltaX), Math.Abs(deltaY)) + 1;

    return Enumerable.Range(0, length)
                     .Select(i => new Point(Start.X + (stepX * i), Start.Y + (stepY * i)));
  }

  public bool IsDiagonal => Start.X != End.X && Start.Y != End.Y;

  public static Line Parse(string input)
  {
    var points = input.Split(" -> ");
    return new Line(Point.Parse(points[0]), Point.Parse(points[1]));
  }
}