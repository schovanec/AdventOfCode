// See https://aka.ms/new-console-template for more information
using System.Collections.Immutable;

var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(Instruction.Parse)
                .ToList()
                .AsReadOnly();

var machine = new Machine(input, new MachineState());

#if false
//var result1 = Execute(machine, EnumDigits(11111111564138));
var result2 = Execute(machine, EnumDigits(11111111564139));
//var result3 = Execute(machine, EnumDigits(11111111564141));

//Console.WriteLine(result1.State?.GetValueOrDefault("z"));
Console.WriteLine("..");
#else
var digitValues = new long[] { 9, 8, 7, 6, 5, 4, 3, 2, 1 };
var maxModel = FindMaximumModelNumber(machine, digitValues);
Console.WriteLine($"Part 1 Result = {maxModel}");

digitValues = digitValues.Reverse().ToArray();
var minModel = FindMaximumModelNumber(machine, digitValues);
Console.WriteLine($"Part 2 Result = {minModel}");
#endif
//foreach (var d in EnumDigits(12345678912345))
//  Console.WriteLine(d);

long? FindMaximumModelNumber(Machine machine,
                             IEnumerable<long> digitInputs,
                             int digits = 14,
                             long value = 0,
                             Dictionary<(MachineState state, long input), MachineState>? cache = default)
{
  if (digits == 0)
  {
    //Console.WriteLine($"{value} => {machine.State?.GetValueOrDefault("z")}");
    return machine.State["z"] == 0 ? value : null;
  }

  cache ??= new();

#if false
  var options = (from i in Enumerable.Range(1, 9)
                 let m = Execute(machine, new long[] { i })
                 let z = m.State!.GetValueOrDefault("z")
                 orderby z
                 select (machine: m, z: z, digit: i)).ToList();

  var max = default(long?);
  foreach (var item in options)
  {
    var cur = (value * 10) + item.digit;
    var result = FindMaximumModelNumber(item.machine, digits - 1, cur);
    if (result.HasValue && result > max)
      max = result.Value;
  }

  return max;
#else
  foreach (var i in digitInputs)
  {
    var tail = ExecuteWithCache(machine, i, cache);
    var current = (value * 10) + i;
    //Console.WriteLine($"Current = {current}, w={tail.State?["w"]}, x={tail.State?["x"]}, y={tail.State?["y"]}, z={tail.State?["z"]}");
    var result = FindMaximumModelNumber(tail, digitInputs, digits - 1, current, cache);
    if (result.HasValue)
      return result;

    //break;
  }
#endif

  return null;
}

#if false
IEnumerable<long> EnumDigits(long value)
{
  var digit = 10000000000000;
  while (digit > 0)
  {
    yield return value / digit;
    value = value % digit;
    digit /= 10;
  }
}
#endif

Machine ExecuteWithCache(Machine machine, long nextInput, Dictionary<(MachineState state, long input), MachineState> cache)
{
  var key = (machine.State, nextInput);
  if (cache.TryGetValue(key, out var result))
    return machine with { State = result };

  var after = Execute(machine, new[] { nextInput });
  cache[key] = after.State;

  return after;
}

Machine Execute(Machine machine, IEnumerable<long> input)
{
  using var inputEnum = input.GetEnumerator();

  var program = machine.Program;
  var state = machine.State;

  while (state.ProgramCounter < machine.Program.Count)
  {
    var instruction = machine.Program[state.ProgramCounter];
    var result = Step(
      instruction,
      input: () => inputEnum.MoveNext() ? inputEnum.Current : default(long?),
      read: p => p switch
      {
        VariableParameter(var name) => state[name],
        LiteralParameter(var value) => value,
        _ => 0
      },
      write: (p, v) => state = state.Write(p.Name, v)
    );

    if (!result)
      break;

    state = state.MoveNext();
  }

  return machine with { State = state };
}

bool Step(Instruction instruction, Func<long?> input, Func<Parameter, long> read, Action<VariableParameter, long> write)
{
  switch (instruction)
  {
    case UnaryInstruction(Operation.Input, var output):
      var amount = input();
      if (!amount.HasValue)
        return false;

      write(output, amount.Value);
      return true;

    case BinaryInstruction(var op, var arg1, var arg2):
      write(arg1, Calculate(op, read(arg1), read(arg2)));
      return true;

    default:
      throw new InvalidOperationException();
  }
}

long Calculate(Operation operation, long arg1, long arg2)
  => operation switch
  {
    Operation.Add => arg1 + arg2,
    Operation.Multiply => arg1 * arg2,
    Operation.Divide => arg1 / arg2,
    Operation.Modulus => arg1 % arg2,
    Operation.Equals => arg1 == arg2 ? 1 : 0,
    _ => throw new InvalidOperationException()
  };

record struct MachineState(int ProgramCounter = 0, long W = 0, long X = 0, long Y = 0, long Z = 0)
{
  public long this[string variable]
    => variable switch { "w" => W, "x" => X, "y" => Y, "z" => Z, _ => 0 };

  public MachineState Write(string variable, long value)
    => variable switch
    {
      "w" => this with { W = value },
      "x" => this with { X = value },
      "y" => this with { Y = value },
      "z" => this with { Z = value },
      _ => this
    };

  public MachineState MoveNext()
    => this with { ProgramCounter = ProgramCounter + 1 };
}

record Machine(IReadOnlyList<Instruction> Program, MachineState State);

abstract record Parameter
{
  public static Parameter Parse(string text)
    => char.IsLetter(text[0])
     ? new VariableParameter(text)
     : new LiteralParameter(long.Parse(text));
}

record VariableParameter(string Name) : Parameter { }

record LiteralParameter(long Value) : Parameter { }

abstract record Instruction(Operation Operation)
{
  //public abstract void Execute(Func<long> input, Func<Parameter, long> read, Action<VariableParameter, long> write);

  public static Instruction Parse(string text)
  {
    var split = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var op = GetOperation(split[0]);
    if (op == Operation.Input)
      return new UnaryInstruction(op, new VariableParameter(split[1]));
    else
      return new BinaryInstruction(op, new VariableParameter(split[1]), Parameter.Parse(split[2]));
  }

  private static Operation GetOperation(string text)
    => text switch
    {
      "inp" => Operation.Input,
      "add" => Operation.Add,
      "mul" => Operation.Multiply,
      "div" => Operation.Divide,
      "mod" => Operation.Modulus,
      "eql" => Operation.Equals,
      _ => throw new FormatException()
    };
}

record UnaryInstruction(Operation Operation, VariableParameter Param1) : Instruction(Operation)
{

}

record BinaryInstruction(Operation Operation, VariableParameter Param1, Parameter Param2) : Instruction(Operation)
{
}

enum Operation
{
  Input,
  Add,
  Multiply,
  Divide,
  Modulus,
  Equals
}