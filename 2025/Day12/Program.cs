// See https://aka.ms/new-console-template for more information
using System.Collections.Immutable;

var (presents, regions) = ParseInput(File.ReadAllLines(args.FirstOrDefault("input.txt")));

var result1 = regions.Count(r => CanFill(r, presents));
Console.WriteLine($"Part 1 Result = {result1}");

static bool CanFill(Region region, ImmutableList<Shape> presents)
{
  var totalShapes = region.PresentsNeeded.Sum();
  if (totalShapes <= region.GetMaxShapes())
    return true;

  var totalArea = region.PresentsNeeded.Select((n, i) => n * presents[i].Area).Sum();
  if (totalArea > (region.Width * region.Height))
    return false;

  throw new NotImplementedException("Eeek!");
}

static (ImmutableList<Shape>, ImmutableList<Region>) ParseInput(string[] input)
{
  var presents = ImmutableList.CreateBuilder<Shape>();
  var regions = ImmutableList.CreateBuilder<Region>();
  for (var i = 0; i < input.Length; ++i)
  {
    var line = input[i];
    if (line.Length > 0)
    {
      var parts = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length == 1)
      {
        var start = i + 1;
        var end = start + Shape.Height;
        var area = input[start..end].Sum(x => x.Count(ch => ch =='#'));
        presents.Add(new (area));
        i = end;
      }
      else if (parts.Length > 1)
      {
        var size = parts[0].Split('x');
        var needs = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        regions.Add(new (int.Parse(size[0]), int.Parse(size[1]), needs.Select(int.Parse).ToImmutableArray()));
      }
    }
  }

  return (presents.ToImmutable(), regions.ToImmutable());
}

record Shape(int Area)
{
  public static readonly int Width = 3;

  public static readonly int Height = 3;
}

record Region(int Width, int Height, ImmutableArray<int> PresentsNeeded)
{
  public int GetMaxShapes() => (Width / Shape.Width) * (Height / Shape.Height);
}

