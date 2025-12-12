
using System.Collections.Immutable;

var input = File.ReadLines(args.FirstOrDefault("input.txt"))
                .Select(Device.Parse)
                .ToDictionary(x => x.Id);

var result1 = CountPaths(input, "you", "out");
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = CountPaths(input, "svr", "out", "fft", "dac");
Console.WriteLine($"Part 2 Result = {result2}");

static long CountPaths(
  IDictionary<string, Device> devices, string start, string goal,
  string? required1 = default, string? required2 = default)
{
  SearchState initialState = new (start, required1, required2);
  Dictionary<SearchState, long> cache = new();
  return Search(initialState);

  long Search(SearchState state)
  {
    if (cache.TryGetValue(state, out var cachedResult))
      return cachedResult;

    long result;
    if (state.Start == goal)
    {
      result = state.HasRequired ? 0L : 1L;
    }
    else if (!devices.TryGetValue(state.Start, out var device))
    {
      result = 0L;
    }
    else
    {
      result = device.Outputs
                    .Sum(x => Search(state.Next(x)));
    }

    return cache[state] = result;
  }
}

record Device(string Id, ImmutableArray<string> Outputs)
{
  public static Device Parse(string input)
  {
    var parts = input.Split(':', StringSplitOptions.TrimEntries);
    var id = parts[0];
    var outputs = parts[1].Split(' ');

    return new(id, outputs.ToImmutableArray());
  }
}

record struct SearchState(string Start, string? Required1 = default, string? Required2 = default)
{
  public bool HasRequired => Required1 != null || Required2 != null;

  public SearchState Next(string nextStart)
  {
    if (Required1 == nextStart)
      return new (nextStart, Required2);
    else if (Required2 == nextStart)
      return new (nextStart, Required1);
    else
      return this with { Start = nextStart };
  }
}