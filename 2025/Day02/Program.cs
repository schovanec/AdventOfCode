var input = File.ReadAllLines(args.FirstOrDefault("input.txt"))
                .First()
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ParseRange)
                .ToList();

var result1 = input.SelectMany(r => FindInvalids(r, IsInvalid1)).Sum();
Console.WriteLine($"Part 1 Result = {result1}");

// foreach (var item in input.SelectMany(r => FindInvalids(r, IsInvalid2)))
//   Console.WriteLine(item);

var result2 = input.SelectMany(r => FindInvalids(r, IsInvalid2)).Sum();
Console.WriteLine($"Part 2 Result = {result2}");

static IEnumerable<long> FindInvalids((long start, long end) range, Func<long, bool> test)
  => EnumRange(range.start, range.end).Where(test);

static IEnumerable<long> EnumRange(long start, long end)
{
  for (var i = start; i <= end; ++i)  
    yield return i;
}

static bool IsInvalid1(long id) => IsRepeated(id.ToString(), 2);

static bool IsInvalid2(long id)
{
  var text = id.ToString().AsSpan();
  var len = text.Length;
  for (int n = 2; n <= len; ++n)
  {
    if (IsRepeated(text, n))
      return true;
  }

  return false;
}

static bool IsRepeated(ReadOnlySpan<char> text, int count)
{
  var len = text.Length;
  if (len == 0 || len % count != 0)
    return false;

  var n = len / count;
  var first = text.Slice(0, n);
  for (var i = n; i < len; i += n)
  {
    if (!text.Slice(i, n).SequenceEqual(first))
      return false;
  }

  return true;
}

static (long start, long end) ParseRange(string range)
{
  var parts = range.Split('-', 2);
  return (long.Parse(parts[0]), long.Parse(parts[1]));
}