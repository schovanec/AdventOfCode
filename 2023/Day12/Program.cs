using System.Collections.Immutable;
using System.Runtime.CompilerServices;

var records = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                  .Select(SpringStates.Parse)
                  .ToArray();

var result1 = records.Sum(r => CountArrangements(r.States, r.Check.AsSpan()));
Console.WriteLine($"Part 1 Result = {result1}");

int CountArrangements(string state, ReadOnlySpan<int> check)
{
  if (state.Length == 0)
    return check.IsEmpty ? 1 : 0;

  if (check.IsEmpty)
    return state.Contains('#') ? 0 : 1;

  if (IsOperational(state[0]))
    return CountArrangements(state[1..], check);

  var span = check[0];
  if (state.Length < span)
    return 0;

  if (IsDamaged(state[0]))
  {
    if (state.Take(span).Any(IsOperational))
      return 0;

    var skip = Math.Min(span + 1, state.Length);
    if (skip > span && !CanBeOperational(state[span]))
      return 0;

    return CountArrangements(state[skip..], check[1..]);
  }
  else
  {
    var countIfWorking = CountArrangements(state[1..], check);

    if (state.Take(span).Any(IsOperational))
      return countIfWorking;

    var skip = Math.Min(span + 1, state.Length);
    if (skip > span && !CanBeOperational(state[span]))
      return countIfWorking;

    return CountArrangements(state[skip..], check[1..]) + countIfWorking;
  }
}

bool IsOperational(char ch) => ch == '.';

bool IsDamaged(char ch) => ch == '#';

bool CanBeDamaged(char ch) => ch == '#' || ch == '?';

bool CanBeOperational(char ch) => ch == '.' || ch == '?';

record SpringStates(string States, ImmutableArray<int> Check)
{
  public static SpringStates Parse(string input)
  {
    var split = input.Split(' ', 2);
    return new(
      split[0],
      split[1].Split(',').Select(int.Parse).ToImmutableArray());
  }
}