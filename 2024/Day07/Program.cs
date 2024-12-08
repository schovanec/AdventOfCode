using System.Collections.Immutable;

var input = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")
                .Select(ParseEquation)
                .ToList();

var result1 = input.Where(x => CanMakeTrue(x.operands, x.result))
                   .Sum(x => x.result);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = input.Where(x => CanMakeTrue(x.operands, x.result, true))
                   .Sum(x => x.result);
Console.WriteLine($"Part 2 Result = {result2}");

static bool CanMakeTrue(ImmutableArray<long> operands, long result, bool allowConcat = false)
{
  if (operands.Length == 0)
    return false;
  else if (operands.Length == 1)
    return result == operands[0];

  var op1 = operands[0];
  var op2 = operands[1];
  var tail = operands[2..];
  return CanMakeTrue([op1 + op2, .. tail], result, allowConcat)
      || CanMakeTrue([op1 * op2, .. tail], result, allowConcat)
      || (allowConcat && CanMakeTrue([long.Parse($"{op1}{op2}"), .. tail], result, allowConcat));
}

static (long result, ImmutableArray<long> operands) ParseEquation(string input)
{
  var parts = input.Split(':', 2, StringSplitOptions.TrimEntries);

  var result = long.Parse(parts[0]);
  var operands = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                         .Select(long.Parse)
                         .ToImmutableArray();

  return (result, operands);
}