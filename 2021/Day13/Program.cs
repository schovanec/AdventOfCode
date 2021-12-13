
using System.Collections.Immutable;
using System.Text.RegularExpressions;

var (points, folds) = Parse(File.ReadLines(args.FirstOrDefault() ?? "input.txt"));

var firstFold = Fold(points, folds.First());
Console.WriteLine($"Part 1 Result = {firstFold.Count}");

var final = folds.Aggregate(points, (pts, tx) => Fold(pts, tx));

Console.WriteLine("Part 2 Result:");
Print(final);

ImmutableHashSet<Point> Fold(ImmutableHashSet<Point> points, Func<Point, Point> transform)
  => points.Select(transform).ToImmutableHashSet();

void Print(ImmutableHashSet<Point> points)
{
  var xmax = points.Max(pt => pt.X);
  var ymax = points.Max(pt => pt.Y);

  for (var y = 0; y <= ymax; ++y)
  {
    for (var x = 0; x <= xmax; ++x)
      Console.Write(points.Contains(new Point(x, y)) ? '#' : ' ');

    Console.WriteLine();
  }
}

(ImmutableHashSet<Point> points, ImmutableList<Func<Point, Point>> folds) Parse(IEnumerable<string> lines)
{
  var points = ImmutableHashSet.CreateBuilder<Point>();
  var folds = ImmutableList.CreateBuilder<Func<Point, Point>>();

  foreach (var line in lines)
  {
    var match = Regex.Match(line, @"^fold along (?<axis>[yx])=(?<value>\d+)$");
    if (match.Success)
    {
      var pos = int.Parse(match.Groups["value"].Value);
      if (match.Groups["axis"].Value == "x")
        folds.Add(pt => pt.X <= pos ? pt : pt with { X = (2 * pos) - pt.X });
      else
        folds.Add(pt => pt.Y <= pos ? pt : pt with { Y = (2 * pos) - pt.Y });
    }
    else if (!string.IsNullOrEmpty(line))
    {
      var coords = line.Split(',');
      points.Add(new Point(int.Parse(coords[0]), int.Parse(coords[1])));
    }
  }

  return (points.ToImmutable(), folds.ToImmutable());
}

record struct Point(int X, int Y);