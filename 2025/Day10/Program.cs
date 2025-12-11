using System.Collections.Immutable;
using System.Text;
using Microsoft.Z3;

var input = File.ReadLines(args.FirstOrDefault("input.txt"))
                .Select(MachineSpec.Parse)
                .ToImmutableArray();

var result1 = input.Select(FindMinIndicatorPresses)
                   .AsParallel()
                   .Sum();
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = input.Select(FindMinJoltagePresses)
                   .AsParallel()
                   .Sum();
Console.WriteLine($"Part 2 Result = {result2}");

static int FindMinIndicatorPresses(MachineSpec spec)
{
  var start = new string('.', spec.Indicators.Length);
  Queue<string> queue = new([start]);
  HashSet<string> visited = [start];
  Dictionary<string,int> dist = new() {[start] = 0};
  while (queue.Count > 0)
  {
    var current = queue.Dequeue();
    var currentDist = dist[current];
    if (current == spec.Indicators)
      return currentDist;

    foreach (var next in spec.EnumIndicatorButtonStep(current).Where(n => !visited.Contains(n)))
    {
      var nextDist = currentDist + 1;
      if (nextDist < dist.GetValueOrDefault(next, int.MaxValue))
      {
        visited.Add(next);
        dist[next] = nextDist;
        queue.Enqueue(next);
      }
    }
  }

  return int.MaxValue;
}

static int FindMinJoltagePresses(MachineSpec m)
{
  using var ctx = new Context();

  var buttons = Enumerable.Range(0, m.ButtonCount)
                          .Select(i => $"b{i}")
                          .Select(n => (IntExpr)ctx.MkConst(n, ctx.IntSort))
                          .ToArray();

  var opt = ctx.MkOptimize();

  // minimize the sum of button presses
  opt.MkMinimize(ctx.MkAdd(buttons));

  // button presses cannot be negative
  var zero = ctx.MkInt(0);
  foreach (var b in buttons)
    opt.Assert(ctx.MkGe(b, zero));

  var buttonMap = m.Buttons
                   .SelectMany((b, i) => b.Select(j => (index: i, joltage: j)))
                   .ToLookup(x => x.joltage, x => x.index);

  // add expression for each joltage value
  for (var i = 0; i < m.JoltageRequirements.Length; ++i)
  {
    var sum = ctx.MkAdd(from j in buttonMap[i]
                        select buttons[j]);

    opt.Assert(ctx.MkEq(ctx.MkInt(m.JoltageRequirements[i]), sum));
  }

  if (opt.Check() != Status.SATISFIABLE)
  {
    Console.WriteLine($"{opt.Check()}: {m}");
    return 0;
  }

  var result = buttons.Select(exp => (IntNum)opt.Model.Evaluate(exp)).ToArray();
  return result.Sum(n => n.Int);
}

record MachineSpec(
  string Indicators,
  ImmutableArray<ImmutableArray<int>> Buttons,
  ImmutableArray<int> JoltageRequirements)
{
  public int ButtonCount => Buttons.Length;

  public string PressIndicatorButton(string current, int button)
  {
    var newState = new StringBuilder(current);
    foreach (var item in Buttons[button])
      newState[item] = newState[item] == '#' ? '.' : '#';

    return newState.ToString();
  }

  public IEnumerable<string> EnumIndicatorButtonStep(string current)
    => Enumerable.Range(0, ButtonCount)
                 .Select(i => PressIndicatorButton(current, i));

  public static MachineSpec Parse(string input)
  {
    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    
    string indicators = string.Empty;
    var buttons = ImmutableArray.CreateBuilder<ImmutableArray<int>>();
    var joltage = ImmutableArray<int>.Empty;
    foreach (var item in parts)
    {
      switch (item[0])
      {
        case '[':
          indicators = item[1..^1];
          break;

        case '(':
          buttons.Add(item[1..^1].Split(',')
                                    .Select(int.Parse)
                                    .ToImmutableArray());
          break;

        case '{':
          joltage = item[1..^1].Split(',')
                               .Select(int.Parse)
                               .ToImmutableArray();
          break;
      }
    }

    return new (indicators, buttons.ToImmutable(), joltage);
  }
}