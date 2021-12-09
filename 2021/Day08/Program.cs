using System.Collections.Immutable;

var input = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")
                .Select(x => x.Split(" | "))
                .Select(x => (signals: x[0].Split(' '), outputs: x[1].Split(' ')))
                .ToList();

var uniqueSegmentCounts = new[] { 2, 3, 4, 7 };
var count = input.SelectMany(x => x.outputs)
                 .GroupBy(x => x.Length)
                 .Where(g => uniqueSegmentCounts.Contains(g.Key))
                 .Sum(g => g.Count());

Console.WriteLine($"Part 1 Result = {count}");

var digits = new List<string>
{
  "abcefg",   // 0
  "cf",       // 1
  "acdeg",    // 2
  "acdfg",    // 3
  "bcdf",     // 4
  "abdfg",    // 5
  "abdefg",   // 6
  "acf",      // 7
  "abcdefg",  // 8
  "abcdfg"    // 9
};

var digitsBySize = digits.Select((x, i) => (
                            value: i,
                            pattern: x,
                            indexes: x.Select(a => a - 'a').ToImmutableArray()
                          ))
                         .ToLookup(x => x.pattern.Length);

var allConfigurations = GetPermutations(digits[8]).ToImmutableList();

var sum = input.Select(x => DecodeInput(x.signals, x.outputs)).Sum();
Console.WriteLine($"Part 2 Result = {sum}");

int DecodeInput(string[] signals, string[] outputs)
{
  var config = signals.Select(x => allConfigurations.Where(c => IsMatch(c, x)).ToImmutableHashSet())
                      .Aggregate((a, b) => a.Intersect(b))
                      .Single();

  return outputs.Select(x => GetDigitValue(config, x))
                 .Aggregate((a, b) => (a * 10) + b);
}

ImmutableList<string> GetPermutations(string input)
{
  Span<char> buffer = stackalloc char[input.Length];
  input.CopyTo(buffer);

  var results = ImmutableList.CreateBuilder<string>();
  Permute(buffer, 0, results);

  return results.ToImmutable();
}

void Permute(Span<char> input, int startIndex, ImmutableList<string>.Builder output)
{
  if (startIndex == input.Length - 1)
  {
    output.Add(input.ToString());
  }
  else
  {
    for (int i = startIndex; i < input.Length; ++i)
    {
      (input[startIndex], input[i]) = (input[i], input[startIndex]);
      Permute(input, startIndex + 1, output);
      (input[startIndex], input[i]) = (input[i], input[startIndex]);
    }
  }
}

bool IsMatch(string config, string input)
{
  return digitsBySize[input.Length].Any(c => c.indexes.All(x => input.Contains(config[x])));
}

int GetDigitValue(string config, string digit)
{
  Span<char> key = stackalloc char[digit.Length];

  for (var i = 0; i < digit.Length; ++i)
    key[i] = (char)('a' + config.IndexOf(digit[i]));

  key.Sort();

  return digits.IndexOf(key.ToString());
}