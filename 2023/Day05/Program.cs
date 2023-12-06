using System.Collections.Immutable;

var (seeds, maps) = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var result1 = seeds.Min(s => ApplyMaps(maps, s));
Console.WriteLine($"Part 1 Result = {result1}");

static long ApplyMaps(IEnumerable<Map> maps, long seed) => maps.Aggregate(seed, (a, m) => m.Apply(a));

(ImmutableList<long> seeds, ImmutableList<Map> maps) ParseInput(string[] input)
{
  var seeds = input[0].Split(':', 2, StringSplitOptions.TrimEntries)[1]
                      .Split(' ')
                      .Select(long.Parse)
                      .ToImmutableList();

  var maps = ImmutableList.CreateBuilder<Map>();
  var index = 1;
  var entries = new List<MapEntry>();
  while (index < input.Length)
  {
    if (input[index].EndsWith(':'))
    {
      ++index;
      while (index < input.Length && input[index].Length > 0)
      {
        var numbers = input[index].Split(' ').Select(long.Parse).ToArray();
        entries.Add(new(numbers[2], numbers[1], numbers[0]));

        ++index;
      }

      maps.Add(Map.Create(entries));
      entries.Clear();
    }
    else
    {
      ++index;
    }
  }

  return (seeds, maps.ToImmutable());
}

record Map(ImmutableList<MapEntry> Entries)
{
  public long Apply(long sourceValue)
    => Entries.FirstOrDefault(x => x.Contains(sourceValue))?.Apply(sourceValue) ?? sourceValue;

  public static Map Create(IEnumerable<MapEntry> entries)
    => new(entries.OrderBy(x => x.SourceStart).ToImmutableList());
}

record MapEntry(long Size, long SourceStart, long DestStart)
{
  public long SourceEnd => SourceStart + Size;

  public bool Contains(long sourceValue) => sourceValue >= SourceStart && sourceValue < SourceEnd;

  public long Apply(long sourceValue) => DestStart + (sourceValue - SourceStart);
}