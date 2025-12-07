using System.Collections.Immutable;

var diagram = Diagram.Parse(File.ReadAllLines(args.FirstOrDefault("input.txt")));

var (result1, result2) = CountSplits(diagram);
Console.WriteLine($"Part 1 Result = {result1}");
Console.WriteLine($"Part 2 Result = {result2}");

static (int splits, long timelines) CountSplits(Diagram diagram)
{
  ImmutableHashSet<int> beams = [diagram.Start.X];
  var timelines = ImmutableDictionary.CreateRange([KeyValuePair.Create(diagram.Start.X, 1L)]);
  var height = diagram.Height;
  var splitCount = 0;
  for (var y = diagram.Start.Y + 1; y < height; ++y)
  {
    var newBeams = beams.ToBuilder();
    var newTimelines = timelines.ToBuilder();
    foreach (var beamX in beams)
    {
      if (diagram.Splitters.Contains(new (beamX, y)))
      {
        ++splitCount;
        newBeams.Remove(beamX);
        newBeams.Add(beamX - 1);
        newBeams.Add(beamX + 1);

        if (!newTimelines.Remove(beamX, out var incomingTimelines))
          incomingTimelines = 0;

        newTimelines[beamX - 1] = newTimelines.GetValueOrDefault(beamX - 1) + incomingTimelines;
        newTimelines[beamX + 1] = newTimelines.GetValueOrDefault(beamX + 1) + incomingTimelines;
      }
    }

    beams = newBeams.ToImmutable();
    timelines = newTimelines.ToImmutable();
  }

  return (splitCount, timelines.Values.Sum());
}

record struct Point(int X, int Y);

record Diagram(Point Start, ImmutableHashSet<Point> Splitters)
{
  public int Height => Splitters.Max(p => p.Y) + 1;

  public int Width => Splitters.Max(p => p.X) + 1;

  public static Diagram Parse(string[] lines)
  {
    var start = default(Point);
    var splitters = ImmutableHashSet.CreateBuilder<Point>();

    for (var y = 0; y < lines.Length; ++y)
    {
      var line = lines[y];
      for (var x = 0; x < line.Length; ++x)
      {
        switch (line[x])
        {
          case 'S':
            start = new (x, y);
            break;

          case '^':
            splitters.Add(new (x, y));
            break;
        }
      }
    }

    return new (start, splitters.ToImmutable());
  }
}