using System.Collections.Immutable;

var input = Universe.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var expanded1 = input.Expand();
var result1 = expanded1.EnumDistances().Sum();
Console.WriteLine($"Part 1 Result = {result1}");

var expanded2 = input.Expand(1000000);
var result2 = expanded2.EnumDistances().Sum();
Console.WriteLine($"Part 2 Result = {result2}");

record struct Point(long X, long Y);

record Universe(ImmutableList<Point> Galaxies, long Width, long Height)
{
  public Universe Expand(int factor = 2)
  {
    var emptyRows = LongRange(0, Height).Except(Galaxies.Select(p => p.Y)).ToArray();

    var emptyCols = LongRange(0, Width).Except(Galaxies.Select(p => p.X)).ToArray();

    var expanded = from p in Galaxies
                   let offsetX = emptyCols.Count(x => x < p.X) * (factor - 1)
                   let offsetY = emptyRows.Count(y => y < p.Y) * (factor - 1)
                   select new Point(p.X + offsetX, p.Y + offsetY);

    return new(
      expanded.ToImmutableList(),
      Width + emptyCols.Length,
      Height + emptyRows.Length);
  }

  public IEnumerable<long> EnumDistances()
    => from i in Enumerable.Range(0, Galaxies.Count)
       from j in Enumerable.Range(i + 1, Galaxies.Count - i - 1)
       select ManhattanDistance(Galaxies[i], Galaxies[j]);

  private static long ManhattanDistance(Point a, Point b)
    => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

  static IEnumerable<long> LongRange(long start, long count)
  {
    for (var i = start; i < count; ++i)
      yield return i;
  }

  public static Universe Parse(string[] input)
  {
    var galaxies = (from y in Enumerable.Range(0, input.Length)
                    let line = input[y]
                    from x in Enumerable.Range(0, line.Length)
                    where line[x] == '#'
                    select new Point(x, y)).ToImmutableList();

    var width = galaxies.Max(p => p.X) + 1;
    var height = galaxies.Max(p => p.Y) + 1;

    return new(galaxies, width, height);
  }
}