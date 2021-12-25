using System.Collections.Immutable;

var map = Map.Parse(File.ReadLines(args.FirstOrDefault() ?? "input.txt"));

var count = 0;
var current = map;
while (true)
{
  ++count;
  var next = Step(current);

  if (next.Data.SequenceEqual(current.Data))
    break;

  current = next;
}

Console.WriteLine($"Part 1 Result = {count}");

Map Step(Map current)
{
  Map result;

  result = StepType(current, MapEntry.East);
  result = StepType(result, MapEntry.South);

  return result;
}

Map StepType(Map current, MapEntry type)
{
  var result = current.Data.ToBuilder();

  foreach (var pt in current.EnumCucumbers(type))
  {
    var next = current.GetNext(pt);
    if (current[next] == MapEntry.Empty)
    {
      var ptIndex = current.GetIndex(pt);
      var nextIndex = current.GetIndex(next);
      (result[ptIndex], result[nextIndex]) = (result[nextIndex], result[ptIndex]);
    }
  }

  return current with { Data = result.ToImmutable() };
}

enum MapEntry { Empty, East, South }

record Map(ImmutableArray<MapEntry> Data, int Width)
{
  public int Height => Data.Length / Width;

  public MapEntry this[(int x, int y) pt] => Data[GetIndex(pt)];

  public int GetIndex((int x, int y) pt)
    => IsValid(pt) ? (pt.y * Width) + pt.x : throw new ArgumentOutOfRangeException();

  public (int x, int y) GetNext((int x, int y) pt)
    => this[pt] switch
    {
      MapEntry.East => ((pt.x + 1) % Width, pt.y),
      MapEntry.South => (pt.x, (pt.y + 1) % Height),
      _ => throw new InvalidOperationException()
    };

  public IEnumerable<(int x, int y)> EnumCucumbers(MapEntry type)
    => from y in Enumerable.Range(0, Height)
       from x in Enumerable.Range(0, Width)
       let pt = (x, y)
       where this[pt] == type
       select (x, y);

  private bool IsValid((int x, int y) pt)
    => pt.x >= 0 && pt.x < Width && pt.y >= 0 && pt.y < Height;

  public static Map Parse(IEnumerable<string> input)
  {
    var result = ImmutableArray.CreateBuilder<MapEntry>();

    int? width = null;
    foreach (var line in input)
    {
      width = line.Length;
      result.AddRange(line.Select(ch => ch switch { '>' => MapEntry.East, 'v' => MapEntry.South, _ => MapEntry.Empty }));
    }

    if (!width.HasValue)
      throw new FormatException();

    return new Map(result.ToImmutable(), width.Value);
  }
}
