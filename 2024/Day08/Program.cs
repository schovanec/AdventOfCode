using System.Collections.Immutable;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var result1 = EnumAllAntiNodes(map).Distinct().Count();
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = EnumAllAntiNodes(map, true).Distinct().Count();
Console.WriteLine($"Part 2 Result = {result2}");

static IEnumerable<Vector> EnumAllAntiNodes(Map map, bool useResonantHarmonics = false)
{
  var frequencies = map.Antennas.GroupBy(
    x => x.Value,
    (k, g) => (key: k, items: g.Select(x => x.Key).ToList()));

  var startOffset = useResonantHarmonics ? 0 : 1;
  var limit = useResonantHarmonics ? int.MaxValue : 1;
  foreach (var g in frequencies.Where(x => x.items.Count > 1))
  {
    foreach ((var a, var b) in EnumPairs(g.items))
    {
      var diff = b.Subtract(a);

      foreach (var next in a.EnumOffset(diff.Scale(-1), startOffset).TakeWhile(map.Contains).Take(limit))
        yield return next;

      foreach (var next in b.EnumOffset(diff, startOffset).TakeWhile(map.Contains).Take(limit))
        yield return next;
    }
  }
}

static IEnumerable<(T, T)> EnumPairs<T>(IReadOnlyList<T> list)
  => from i in Enumerable.Range(0, list.Count)
     from j in Enumerable.Range(i + 1, list.Count - i - 1)
     select (list[i], list[j]);

record struct Vector(int X, int Y)
{
  public Vector Scale(int scale) => new(X * scale, Y * scale);

  public Vector Add(Vector other) => new(X + other.X, Y + other.Y);

  public Vector Subtract(Vector other) => Add(other.Scale(-1));

  public IEnumerable<Vector> EnumOffset(Vector diff, int start = 0)
  {
    var scale = start;
    while (true)
    {
      yield return Add(diff.Scale(scale));
      ++scale;
    }
  }
}

record Map(int Width, int Height, ImmutableDictionary<Vector, char> Antennas)
{
  public bool Contains(Vector pt)
    => pt.X >= 0
    && pt.Y >= 0
    && pt.X < Width
    && pt.Y < Height;

  public static Map Parse(IList<string> input)
  {
    var width = input.First().Length;
    var height = input.Count;
    var antennas = ImmutableDictionary.CreateBuilder<Vector, char>();
    for (var y = 0; y < height; ++y)
    {
      var line = input[y];
      for (var x = 0; x < width; ++x)
      {
        var ch = line[x];
        if (ch != '.')
          antennas[new(x, y)] = ch;
      }
    }

    return new(width, height, antennas.ToImmutable());
  }
}