using System.Collections.Immutable;

var map = Map.Parse(File.ReadAllLines(args.FirstOrDefault("input.txt")));

var result1 = map.Rolls.Count(map.IsAccessible);
Console.WriteLine($"Part 1 Result = {result1}");

var current = map;
while (true)
{
  var accessible = current.Rolls.Where(current.IsAccessible);
  var remainingRolls = current.Rolls.Except(accessible);
  if (remainingRolls.Count >= current.Rolls.Count)
    break;

  current = current with { Rolls = remainingRolls };
}
var result2 = map.Rolls.Count - current.Rolls.Count;
Console.WriteLine($"Part 2 Result = {result2}");

record struct Point(int X, int Y);

record Map(int Width, int Height, ImmutableHashSet<Point> Rolls)
{
  public bool IsAccessible(Point pt) => CountAdjacentRolls(pt) < 4;

  public int CountAdjacentRolls(Point pt)
    => (from dx in Enumerable.Range(-1, 3)
        from dy in Enumerable.Range(-1, 3)
        let adjacentPt = new Point(pt.X + dx, pt.Y + dy)
        where adjacentPt != pt && Rolls.Contains(adjacentPt)
        select pt).Count();

  public static Map Parse(string[] input)
  {
    var width = input.First().Length;
    var height = input.Length;

    var rolls = ImmutableHashSet.CreateBuilder<Point>();
    for (var y = 0; y < height; ++y)
    {
      for (var x = 0; x < width; ++x)
      {
        if (input[y][x] == '@')
          rolls.Add(new (x, y));
      }
    }

    return new (width, height, rolls.ToImmutable());
  }
}