using System.Collections.Immutable;

ImmutableArray<string> DigitNames = ["zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"];

var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt");

var result1 = input.Select(x => GetCalibrationValue(x, ParseDigit)).Sum();
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = input.Select(x => GetCalibrationValue(x, ParseDigitOrName)).Sum();
Console.WriteLine($"Part 2 Result = {result2}");

int GetCalibrationValue(string line, Func<string, int, int?> parser)
{
  var first = FindFirstNumber(line, parser);
  var last = FindLastNumber(line, parser);
  return (first * 10) + last;
}

int FindFirstNumber(string line, Func<string, int, int?> parser)
  => (from i in Enumerable.Range(0, line.Length)
      let n = parser(line, i)
      where n.HasValue
      select n.Value).FirstOrDefault(0);

int FindLastNumber(string line, Func<string, int, int?> parser)
  => FindFirstNumber(line, ReverseParser(parser));

Func<string, int, int?> ReverseParser(Func<string, int, int?> parser)
  => (input, offset) => parser(input, input.Length - offset - 1);

int? ParseDigit(string input, int offset)
  => offset < input.Length && char.IsDigit(input[offset])
      ? input[offset] - '0'
      : null;

int? ParseName(string input, int offset)
{
  var span = input.AsSpan(offset);
  for (var i = 0; i < DigitNames.Length; ++i)
  {
    if (span.StartsWith(DigitNames[i], StringComparison.Ordinal))
      return i;
  }

  return null;
}

int? ParseDigitOrName(string input, int offset)
  => ParseDigit(input, offset) ?? ParseName(input, offset);
