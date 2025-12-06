var (ranges, values) = ParseInput(File.ReadAllLines(args.FirstOrDefault("input.txt")));

var result1 = values.Count(n => ranges.Any(r => r.Contains(n)));
Console.WriteLine($"Part 1 Result = {result1}");

var compacted = CompactRanges(ranges);
var result2 = compacted.Sum(r => r.Size);
Console.WriteLine($"Part 2 Result = {result2}");

static (Range[] ranges, long[] values) ParseInput(string[] input)
{
  var split = input.IndexOf("");
  var rangeLines = input[..split];
  var valueLines = input[(split+1)..];

  var ranges = rangeLines.Select(Range.Parse).ToArray();
  var values = valueLines.Select(long.Parse).ToArray();

  return (ranges, values);
}

static Range[] CompactRanges(Range[] ranges)
{
  var result = new List<Range>();
  var used = new HashSet<int>();

  var sorted = ranges.OrderBy(r => r.Start).ThenBy(r => r.End).ToArray();
  for (var i = 0; i < sorted.Length; ++i)
  {
    if (used.Contains(i))
      continue;

    var current = sorted[i];

    for (var j = i + 1; j < sorted.Length; ++j)
    {
      if (used.Contains(j))
        continue;

      if (current.Union(sorted[j]) is Range union)
      {
        current = union;
        used.Add(j);
      }
    }

    result.Add(current);
  }

  return result.ToArray();
}

record struct Range(long Start, long End)
{
  public long Size => End - Start + 1;

  public bool Contains(long value)
    => value >= Start && value <= End;

  public bool Intersects(Range other)
    => other.End >= Start && other.Start <= End;

  public Range? Union(Range other)
    => Intersects(other) 
     ? new (Math.Min(Start, other.Start), Math.Max(End, other.End)) 
     : null;

  public static Range Parse(string input)
  {
    var parts = input.Split('-', 2, StringSplitOptions.TrimEntries);
    return new (long.Parse(parts[0]), long.Parse(parts[1]));
  }
}