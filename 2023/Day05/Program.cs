using System.Collections.Immutable;

var (seeds, maps) = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var result1 = seeds.Min(s => ApplyMapsToValue(maps, s));
Console.WriteLine($"Part 1 Result = {result1}");

var ranges = from n in Enumerable.Range(0, seeds.Count / 2)
             let i = n * 2
             select Range.FromStartAndSize(seeds[i], seeds[i + 1]);

var result2 = ranges.SelectMany(r => ApplyMapsToRange(maps, r))
                    .Min(r => r.Start);
Console.WriteLine($"Part 2 Result = {result2}");

static long ApplyMapsToValue(IEnumerable<Map> maps, long seed)
  => maps.Aggregate(seed, (a, m) => m.Apply(a));

static Range[] ApplyMapsToRange(IEnumerable<Map> maps, Range seeds)
  => maps.Aggregate(new[] { seeds }, (a, m) => a.SelectMany(x => m.Apply(x)).ToArray());

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
        entries.Add(MapEntry.Create(numbers[0], numbers[1], numbers[2]));

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
    => Entries.Where(x => x.Range.Contains(sourceValue))
              .Select(x => sourceValue + x.Offset)
              .FirstOrDefault(sourceValue);

  public IEnumerable<Range> Apply(Range source)
  {
    var (start, end) = source;
    var index = 0;
    while (start < end)
    {
      if (index < Entries.Count)
      {
        var (current, offset) = Entries[index];
        if (start < current.Start)
        {
          yield return new(start, current.Start - 1);
          start = current.Start;
        }
        else if (start >= current.End)
        {
          ++index;
        }
        else
        {
          var size = Math.Min(end, current.End) - start;
          yield return Range.FromStartAndSize(start + offset, size);
          start += size;
        }
      }
      else
      {
        yield return new(start, end);
        start = end;
      }
    }
  }

  public static Map Create(IEnumerable<MapEntry> entries)
    => new(entries.OrderBy(x => x.Range.Start).ToImmutableList());
}

record MapEntry(Range Range, long Offset)
{
  public static MapEntry Create(long destStart, long sourceStart, long size)
    => new(Range.FromStartAndSize(sourceStart, size), destStart - sourceStart);
}

record struct Range(long Start, long End)
{
  public long Size = End - Start;

  public bool Contains(long value) => Start <= value && End > value;

  public static Range FromStartAndSize(long start, long size)
    => new(start, start + size);
}