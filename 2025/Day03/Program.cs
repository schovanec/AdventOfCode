var banks = File.ReadAllLines(args.FirstOrDefault("input.txt"))
                .Select(x => x.Select(ch => (long)(ch - '0')).ToArray())
                .ToList();

var result1 = banks.Sum(b => FindBest(b));
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = banks.Sum(b => FindBest(b, 12));
Console.WriteLine($"Part 1 Result = {result2}");

static long FindBest(ReadOnlySpan<long> bank, int count = 2)
{
  var min = 0;
  var result = 0L;
  for (var i = 0; i < count; ++i)
  {
    var max = bank.Length - (count - i - 1);
    var bestIdx = MaxIndex(bank[min..max]) + min;
    result = (result * 10) + bank[bestIdx];
    min = bestIdx + 1;
  }

  return result;
}

static int MaxIndex(ReadOnlySpan<long> span)
{
  var max = 0;
  for (var i = 1; i < span.Length; ++i)
  {
    if (span[i] > span[max])
      max = i;
  }

  return max;
}