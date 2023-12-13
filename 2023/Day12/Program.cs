const char StateOperational = '.';
const char StateDamaged = '#';
const char StateUnknown = '?';

var records = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                  .Select(ParseRecord)
                  .ToArray();

var result1 = records.Sum(r => CountArrangements(r.states, r.check));
Console.WriteLine($"Part 1 Result = {result1}");

var unfolded = records.Select(r => Unfold(r)).ToArray();
var result2 = unfolded.Sum(r => CountArrangements(r.states, r.check));
Console.WriteLine($"Part 2 Result = {result2}");

long CountArrangements(char[] initialStates, int[] initialCheck)
{
  var cache = new Dictionary<(int, int), long>();
  return Search(0, 0);

  long Search(int stateOffset, int checkOffset)
  {
    var key = (stateOffset, checkOffset);
    if (cache.TryGetValue((stateOffset, checkOffset), out var cached))
      return cached;

    var state = initialStates.AsSpan(stateOffset..);
    var check = initialCheck.AsSpan(checkOffset..);

    if (state.IsEmpty)
      return cache[key] = check.IsEmpty ? 1 : 0;

    if (check.IsEmpty)
      return cache[key] = state.Contains(StateDamaged) ? 0 : 1;

    var operationalCount = state.IndexOfAnyExcept(StateOperational);
    if (operationalCount > 0)
      return cache[key] = Search(stateOffset + operationalCount, checkOffset);

    var span = check[0];
    if (state.Length < span)
      return cache[key] = 0;

    if (state[0] == StateDamaged)
    {
      if (state[..span].Contains(StateOperational))
        return cache[key] = 0;

      var skip = Math.Min(span + 1, state.Length);
      if (skip > span && !CanBeOperational(state[span]))
        return cache[key] = 0;

      return cache[key] = Search(stateOffset + skip, checkOffset + 1);
    }
    else
    {
      var countIfWorking = Search(stateOffset + 1, checkOffset);

      if (state[..span].Contains(StateOperational))
        return cache[key] = countIfWorking;

      var skip = Math.Min(span + 1, state.Length);
      if (skip > span && !CanBeOperational(state[span]))
        return cache[key] = countIfWorking;

      return cache[key] = Search(stateOffset + skip, checkOffset + 1) + countIfWorking;
    }
  }
}

static bool CanBeOperational(char ch) => ch == StateOperational || ch == StateUnknown;

static (char[] states, int[] check) Unfold((char[] states, int[] check) record, int count = 5)
{
  var unfoldedStates = Enumerable.Repeat(record.states, count)
                                 .Aggregate((a, b) => [.. a, StateUnknown, .. b]);

  var unfoldedCheck = Enumerable.Repeat(record.check, count)
                                .SelectMany(x => x)
                                .ToArray();

  return (unfoldedStates, unfoldedCheck);
}

static (char[] states, int[] check) ParseRecord(string input)
{
  var split = input.Split(' ', 2);
  return (
    split[0].ToArray(),
    split[1].Split(',').Select(int.Parse).ToArray());
}