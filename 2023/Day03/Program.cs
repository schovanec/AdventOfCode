using System.Collections.Immutable;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var result1 = map.FindPartNumbers().Sum();
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = map.FindGears().Sum(x => x.ratio);
Console.WriteLine($"Part 2 Result = {result2}");

record struct Point(int X, int Y)
{
  public readonly IEnumerable<Point> EnumAdjacent(int width = 1)
  {
    var (x, y) = (X, Y);
    return from dx in Enumerable.Range(-1, width + 2)
           from dy in Enumerable.Range(-1, 3)
           where dy != 0 || dx < 0 || dx >= width
           select new Point(x + dx, y + dy);
  }
}

record Number(Point Location, int Value, int Size)
{
  public IEnumerable<Point> EnumAdjacent()
    => Location.EnumAdjacent(Size);

  public IEnumerable<Point> EnumPoints()
    => from i in Enumerable.Range(0, Size)
       select new Point(Location.X + i, Location.Y);
}

record Map(ImmutableHashSet<Number> Numbers, ImmutableDictionary<Point, char> Symbols)
{
  public IEnumerable<int> FindPartNumbers()
    => from num in Numbers
       where num.EnumAdjacent().Any(Symbols.ContainsKey)
       select num.Value;

  public IEnumerable<(Point location, int ratio)> FindGears()
  {
    var lookup = Numbers.SelectMany(n => n.EnumPoints(), (n, p) => (num: n, pt: p))
                        .ToLookup(x => x.pt, x => x.num);

    return from sym in Symbols
           where sym.Value == '*'
           let nums = sym.Key.EnumAdjacent().SelectMany(pt => lookup[pt]).Distinct().ToArray()
           where nums.Count() == 2
           select (sym.Key, nums.Aggregate(1, (a, n) => a * n.Value));
  }

  public static Map Parse(IList<string> input)
  {
    var numbers = ImmutableHashSet.CreateBuilder<Number>();
    var symbols = ImmutableDictionary.CreateBuilder<Point, char>();

    for (var y = 0; y < input.Count; ++y)
    {
      var line = input[y];

      int? numberStart = default;
      for (var x = 0; x <= line.Length; ++x)
      {
        var ch = x < line.Length ? line[x] : default;

        if (char.IsDigit(ch))
        {
          if (!numberStart.HasValue)
            numberStart = x;
        }
        else
        {
          if (numberStart is int start)
          {
            var size = x - start;
            var value = int.Parse(line.AsSpan(start, size));
            numbers.Add(new(new(start, y), value, size));
            numberStart = default;
          }

          if (ch != '.' && x < line.Length)
            symbols[new Point(x, y)] = ch;
        }
      }
    }

    return new(numbers.ToImmutable(), symbols.ToImmutable());
  }
}