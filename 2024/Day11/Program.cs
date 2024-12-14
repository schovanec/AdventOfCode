using System.Collections.Immutable;

var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .First()
                .Split(' ')
                .ToImmutableList();

var result1 = CountStones(input, 25);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = CountStones(input, 75);
Console.WriteLine($"Part 2 Result = {result2}");

static long CountStones(IEnumerable<string> input, int blinks)
{
  var cache = new Dictionary<(string, int), long>();
  return input.Sum(x => CountSingle(x, blinks));

  long CountSingle(string stone, int blinks)
  {
    if (blinks <= 0)
      return 1;

    if (cache.TryGetValue((stone, blinks), out var cached))
      return cached;

    long result;
    if (stone == "0")
    {
      result = CountSingle("1", blinks - 1);
    }
    else if (stone.Length % 2 == 0)
    {
      var mid = stone.Length / 2;
      result = CountSingle(ulong.Parse(stone[0..mid]).ToString(), blinks - 1);
      result += CountSingle(ulong.Parse(stone[mid..]).ToString(), blinks - 1);
    }
    else
    {
      result = CountSingle((ulong.Parse(stone) * 2024).ToString(), blinks - 1);
    }

    cache[(stone, blinks)] = result;
    return result;
  }
}