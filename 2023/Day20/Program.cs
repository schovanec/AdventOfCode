using System.Collections.Immutable;
using StateKey = (string module, string input);

var modules = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

Part1();
Part2();

void Part1()
{
  Dictionary<StateKey, Level> state = new();
  var lowCount = 0L;
  var highCount = 0L;
  for (var i = 0; i < 1000; ++i)
  {
    var history = Run(modules, state);
    lowCount += history.Count(m => m.Level == Level.Low);
    highCount += history.Count(m => m.Level == Level.High);
  }

  var result1 = lowCount * highCount;
  Console.WriteLine($"Part 1 Result = {result1}");
}

void Part2()
{
  var criticalInputs = modules.Values
                              .Where(m => m.Outputs.Contains("rx"))
                              .SelectMany(m => m.Inputs)
                              .Distinct()
                              .ToImmutableList();

  List<long> cycleSizes = new();
  Dictionary<StateKey, Level> state = new();
  var count = 0L;
  while (criticalInputs.Count > 0)
  {
    ++count;
    var history = Run(modules, state);

    foreach (var item in criticalInputs)
    {
      if (history.Any(m => m.Source == item && m.Level == Level.High))
      {
        cycleSizes.Add(count);
        criticalInputs = criticalInputs.Remove(item);
      }
    }
  }

  var result2 = cycleSizes.Aggregate(LCM);
  Console.WriteLine($"Part 2 Result = {result2}");
}

static ImmutableList<Message> Run(
  IDictionary<string, Module> modules,
  Dictionary<StateKey, Level> state)
{
  var history = ImmutableList.CreateBuilder<Message>();
  Queue<Message> queue = new([new("", "broadcaster", Level.Low)]);

  while (queue.TryDequeue(out var msg))
  {
    history.Add(msg);

    if (modules.TryGetValue(msg.Dest, out var module))
    {
      switch (module.Type)
      {
        case ModuleType.Broadcaster:
          QueueMessage(module, msg.Level);
          break;

        case ModuleType.FlipFlop when msg.Level is Level.Low:
          {
            var key = module.GetStateKey();
            var newValue = Flip(GetState(module));
            SetState(module, newValue);
            QueueMessage(module, newValue);
          }
          break;

        case ModuleType.Conjunction:
          SetStateItem(module, msg.Source, msg.Level);
          var allInputs = module.Inputs.Select(n => GetState(module, n));
          var outValue = allInputs.All(lvl => lvl == Level.High) ? Level.Low : Level.High;
          QueueMessage(module, outValue);
          break;
      }
    }
  }

  return history.ToImmutable();

  void QueueMessage(Module from, Level level)
  {
    foreach (var dest in from.Outputs)
      queue.Enqueue(new(from.Name, dest, level));
  }

  Level Flip(Level input) => input is Level.Low ? Level.High : Level.Low;

  Level GetState(Module m, string key = "")
    => state.GetValueOrDefault((m.Name, key), Level.Low);

  void SetState(Module m, Level value)
    => SetStateItem(m, "", value);

  void SetStateItem(Module m, string key, Level value)
    => state[(m.Name, key)] = value;
}

static long LCM(long a, long b) => (a * b) / GCD(a, b);

static long GCD(long a, long b) => a % b == 0 ? b : GCD(b, a % b);

static ImmutableDictionary<string, Module> ParseInput(string[] input)
{
  List<(ModuleType type, string name)> modules = new();
  List<(string from, string to)> connections = new();

  foreach (var line in input)
  {
    var parts = line.Split("->", 2, StringSplitOptions.TrimEntries);

    var (type, name) = parts[0][0] switch
    {
      '%' => (ModuleType.FlipFlop, parts[0][1..]),
      '&' => (ModuleType.Conjunction, parts[0][1..]),
      _ => (ModuleType.Broadcaster, parts[0])
    };

    modules.Add((type, name));

    connections.AddRange(parts[1].Split(',', StringSplitOptions.TrimEntries)
                                 .Select(x => (name, x)));
  }

  var outputs = connections.ToLookup(x => x.from, x => x.to);
  var inputs = connections.ToLookup(x => x.to, x => x.from);

  return modules.Select(m => new Module(m.type,
                                        m.name,
                                        inputs[m.name].ToImmutableArray(),
                                        outputs[m.name].ToImmutableArray()))
                .ToImmutableDictionary(m => m.Name);
}

enum ModuleType { Broadcaster, FlipFlop, Conjunction }

enum Level { Low, High }

record Module(ModuleType Type, string Name, ImmutableArray<string> Inputs, ImmutableArray<string> Outputs)
{
  public (string module, string key) GetStateKey(string input = "") => (Name, input);
}

record struct Message(string Source, string Dest, Level Level);
