using System.Collections.Immutable;

var (moduleList, connectionList) = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? @"..\..\..\sample1.txt"));

#if false
foreach (var m in modules)
  Console.WriteLine(m);

Console.WriteLine();

foreach (var c in connections)
  Console.WriteLine(c);
#endif

var modules = moduleList.ToDictionary(m => m.Name);
var outputs = connectionList.ToLookup(c => c.From, c => c.To);
var inputs = connectionList.ToLookup(c => c.To, c => c.From);

var current = State.Empty;
for (var i = 0; i < 1000; ++i)
  current = Run(modules, outputs, inputs, current);

var result1 = current.LowCount * current.HighCount;
Console.WriteLine($"Part 1 Result = {result1}");

static State Run(
  Dictionary<string, Module> modules,
  ILookup<string, string> outputs,
  ILookup<string, string> inputs,
  State initialState)
{
  var state = initialState.ModuleStates.ToBuilder();
  var lowCount = 0L;
  var highCount = 0L;

  Queue<(string, string, Level)> queue = new([("broadcaster", "", Level.Low)]);

  while (queue.Count > 0)
  {
    var (name, src, level) = queue.Dequeue();

    if (level is Level.Low)
      ++lowCount;
    else
      ++highCount;

    if (modules.TryGetValue(name, out var module))
    {
      switch (module.Type)
      {
        case ModuleType.Broadcaster:
          Send(name, level);
          break;

        case ModuleType.FlipFlop when level is Level.Low:
          Send(name, state[(name, "")] = Flip(state.GetValueOrDefault((name, ""), Level.Low)));
          break;

        case ModuleType.Conjunction:
          state[(name, src)] = level;
          var allInputs = inputs[name].Select(n => state.GetValueOrDefault((name, n), Level.Low));
          var msg = allInputs.All(lvl => lvl == Level.High) ? Level.Low : Level.High;
          Send(name, msg);
          break;
      }
    }
  }

  return new(
    initialState.LowCount + lowCount,
    initialState.HighCount + highCount,
    state.ToImmutable());

  void Send(string from, Level level)
  {
    foreach (var dest in outputs[from])
      queue.Enqueue((dest, from, level));
  }
}

static Level Flip(Level input) => input is Level.Low ? Level.High : Level.Low;

static (List<Module> modules, List<Connection> connections) ParseInput(string[] input)
{
  List<Module> modules = new();
  List<Connection> connections = new();

  foreach (var line in input)
  {
    var parts = line.Split("->", 2, StringSplitOptions.TrimEntries);

    var module = parts[0][0] switch
    {
      '%' => new Module(ModuleType.FlipFlop, parts[0][1..]),
      '&' => new Module(ModuleType.Conjunction, parts[0][1..]),
      _ => new Module(ModuleType.Broadcaster, parts[0])
    };

    modules.Add(module);

    connections.AddRange(parts[1].Split(',', StringSplitOptions.TrimEntries)
                                 .Select(x => new Connection(module.Name, x)));
  }

  return (modules, connections);
}

enum ModuleType { Broadcaster, FlipFlop, Conjunction }

enum Level { Low, High }

record Module(ModuleType Type, string Name);

record Connection(string From, string To);

record State(long LowCount, long HighCount, ImmutableDictionary<(string, string), Level> ModuleStates)
{
  public static State Empty = new State(0L, 0L, ImmutableDictionary<(string, string), Level>.Empty);
}