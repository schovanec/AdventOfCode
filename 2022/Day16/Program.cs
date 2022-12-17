using System.Collections.Immutable;

var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select((line, i) => Valve.Parse(line) with { Index = i })
                .ToDictionary(x => x.Name);

#if true
var result1 = FindBest(
  "AA",
  x => GetNextStepsSingle(input, x),
  30);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = FindBest(
  ("AA", "AA"),
  x => GetNextStepsDouble(input, x),
  26);
Console.WriteLine($"Part 2 Result = {result2}");

static IEnumerable<Step<string>> GetNextStepsSingle(Dictionary<string, Valve> valves, Step<string> current)
{
  var (pos, state, currentRate) = current;
  var index = valves[pos].Index;
  if (!state.Contains(index))
  {
    var n = valves[pos].Rate;
    if (n > 0)
      yield return new(pos, state.Add(index), currentRate + n);
  }

  foreach (var next in valves[pos].Tunnels)
    yield return new(next, state, currentRate);
}

static IEnumerable<Step<(string a, string b)>> GetNextStepsDouble(Dictionary<string, Valve> valves, Step<(string a, string b)> current)
{
  var ((posA, posB), state, currentRate) = current;

  var (valveA, valveB) = (valves[posA], valves[posB]);
  var (rateA, rateB) = (valveA.Rate, valveB.Rate);
  var (indexA, indexB) = (valveA.Index, valveB.Index);
  var canOpenA = rateA > 0 && !state.Contains(indexA);
  var canOpenB = rateB > 0 && !state.Contains(indexB);

  // both open
  if (posA != posB && canOpenA && canOpenB)
    yield return new((posA, posB), state.Add(indexA).Add(indexB), currentRate + rateA + rateB);

  // A opens
  if (canOpenA)
  {
    var newState = state.Add(indexA);
    var newRate = currentRate + rateA;
    foreach (var next in valves[posB].Tunnels)
      yield return new((posA, next), newState, newRate);
  }

  // B opens
  if (canOpenB)
  {
    var newState = state.Add(indexB);
    var newRate = currentRate + rateB;
    foreach (var next in valves[posA].Tunnels)
      yield return new((next, posB), newState, newRate);
  }

  // both move
  var moves = from nextA in valves[posA].Tunnels
              from nextB in valves[posB].Tunnels
              select (nextA, nextB);
  foreach (var (nextA, nextB) in moves)
    yield return new((nextA, nextB), state, currentRate);
}

int FindBest<T>(T start, Func<Step<T>, IEnumerable<Step<T>>> next, int initialTime)
{
  var cache = new Dictionary<(T, ValveState, int, int), int>();
  return Visit(
    new(start, new(), 0),
    initialTime);

  int Visit(Step<T> current, int timeLeft)
  {
    if (timeLeft == 0)
      return 0;

    var cacheKey = current.CacheKey(timeLeft);
    if (cache.TryGetValue(cacheKey, out var cached))
      return cached;

    var answer = next(current).Max(n => Visit(n, timeLeft - 1));
    return cache[cacheKey] = answer + current.CurrentRate;
  }
}

#else
var (result1, _) = FindBest(input, "AA", 30);
Console.WriteLine($"Part 1 Result = {result1}");

var (result2a, openedValves) = FindBest(input, "AA", 26);
var (result2b, _) = FindBest(input, "AA", 26, openedValves);
Console.WriteLine($"Part 2 Result = {result2a + result2b}");

Result FindBest(
  Dictionary<string, Valve> valves, string start, int initialTime,
  ImmutableSortedSet<string>? openValves = default)
{
  var cache = new Dictionary<(string, string, int, int), Result>();
  return Visit(
    start,
    openValves ?? ImmutableSortedSet<string>.Empty,
    0,
    initialTime);

  Result Visit(string pos, ImmutableSortedSet<string> state, int currentRate, int timeLeft)
  {
    var cacheKey = (pos, string.Concat(state), currentRate, timeLeft);
    if (cache.TryGetValue(cacheKey, out var cached))
      return cached;

    if (timeLeft == 0)
      return new(0, state);

    var next = EnumSteps(pos, state, currentRate, timeLeft);
    var answer = next.OrderByDescending(n => n.Pressure)
                     .ThenBy(n => n.Valves.Count)
                     .First();

    return cache[cacheKey] = answer with { Pressure = answer.Pressure + currentRate };
  }

  IEnumerable<Result> EnumSteps(string pos, ImmutableSortedSet<string> state, int currentRate, int timeLeft)
  {
    if (!state.Contains(pos))
    {
      var n = valves[pos].Rate;
      if (n > 0)
        yield return Visit(pos, state.Add(pos), currentRate + n, timeLeft - 1);
    }

    foreach (var next in valves[pos].Tunnels)
      yield return Visit(next, state, currentRate, timeLeft - 1);
  }
}
#endif

record Valve(string Name, int Rate, ImmutableArray<string> Tunnels, int Index = 0)
{
  public static Valve Parse(string line)
  {
    var parts = line.Split(' ');

    var name = parts[1];
    var rate = int.Parse(parts[4].TrimEnd(';').Split('=', 2)[1]);
    var tunnels = parts.Skip(9)
                       .Select(x => x.TrimEnd(','))
                       .ToImmutableArray();

    return new(name, rate, tunnels);
  }
}

record struct Step<T>(T Position, ValveState State, int CurrentRate)
{
  public (T, ValveState, int, int) CacheKey(int time) => (Position, State, CurrentRate, time);
}

record struct Result(int Pressure, ImmutableSortedSet<string> Valves);

record struct ValveState(ulong Value = 0)
{
  public bool Contains(int i) => (Value & (1UL << i)) != 0;

  public ValveState Add(int i) => new(Value | (1UL << i));
}