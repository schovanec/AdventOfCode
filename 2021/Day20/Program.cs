using System.Collections.Immutable;

var (algo, input) = Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var result1 = Enhance(input, algo, 2);
Console.WriteLine($"Part 1 Result = {result1.PixelCount}");

var result2 = Enhance(input, algo, 50);
Console.WriteLine($"Part 1 Result = {result2.PixelCount}");

Image Enhance(Image image, ImmutableArray<bool> algo, int count)
{
  var result = image;
  for (var i = 0; i < count; ++i)
    result = EnhanceOnce(result, algo);

  return result;
}

Image EnhanceOnce(Image image, ImmutableArray<bool> algo)
{
  var bounds = image.Bounds.Grow();

  var result = ImmutableHashSet.CreateBuilder<Point>();
  foreach (var pt in bounds.Points)
  {
    var n = image.GetEnhanceInput(pt);
    if (algo[n])
      result.Add(pt);
  }

  var infinite = image.InfiniteValue ? algo.Last() : algo.First();
  return new Image(bounds, result.ToImmutable(), infinite);
}

(ImmutableArray<bool> algo, Image image) Parse(IEnumerable<string> input)
{
  var algo = input.First().Select(ch => ch == '#').ToImmutableArray();

  var image = ImmutableHashSet.CreateBuilder<Point>();
  foreach (var (line, y) in input.Skip(2).Select((a, i) => (a, i)))
  {
    image.UnionWith(line.Select((ch, i) => (x: i, value: ch == '#'))
                        .Where(p => p.value)
                        .Select(p => new Point(p.x, y)));
  }

  var bounds = new Rect(
    image.Min(p => p.X),
    image.Min(p => p.Y),
    image.Max(p => p.X),
    image.Max(p => p.Y));

  return (algo, new Image(bounds, image.ToImmutable()));
}

record struct Point(int X, int Y);

record Rect(int MinX, int MinY, int MaxX, int MaxY)
{
  public bool Contains(Point pt) => pt.X >= MinX && pt.X <= MaxX && pt.Y >= MinY && pt.Y <= MaxY;

  public Rect Grow()
    => new Rect(MinX - 1, MinY - 1, MaxX + 1, MaxY + 1);

  public IEnumerable<Point> Points
    => from y in Enumerable.Range(MinY, MaxY - MinY + 1)
       from x in Enumerable.Range(MinX, MaxX - MinX + 1)
       select new Point(x, y);
}

record Image(Rect Bounds, ImmutableHashSet<Point> Pixels, bool InfiniteValue = false)
{
  public bool this[Point pt]
    => Bounds.Contains(pt) ? Pixels.Contains(pt) : InfiniteValue;

  public int GetEnhanceInput(Point pt)
  {
    var values = from yi in Enumerable.Range(pt.Y - 1, 3)
                 from xi in Enumerable.Range(pt.X - 1, 3)
                 select this[new Point(xi, yi)] ? 1 : 0;

    return values.Aggregate(0, (p, n) => (p << 1) | n);
  }

  public int PixelCount
    => Pixels.Count(Bounds.Contains);
}