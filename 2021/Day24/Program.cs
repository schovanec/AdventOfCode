var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(Instruction.Parse)
                .ToList()
                .AsReadOnly();

var machine = new Machine(input, new MachineState());

var digitValues = new long[] { 9, 8, 7, 6, 5, 4, 3, 2, 1 };
var maxModel = FindModelNumber(machine, digitValues);
Console.WriteLine($"Part 1 Result = {maxModel}");

digitValues = digitValues.Reverse().ToArray();
var minModel = FindModelNumberParallel(machine, digitValues);
Console.WriteLine($"Part 2 Result = {minModel}");

long FindModelNumberParallel(Machine machine,
                             IEnumerable<long> digitInputs,
                             int digits = 14,
                             long value = 0)
{
  return digitInputs.AsParallel()
                    .Select(i => FindModelNumber(Execute(machine, EnumSingle(i)), digitInputs, 13, i))
                    .Where(r => r.HasValue)
                    .Min(r => r ?? 0);
}

long? FindModelNumber(Machine machine,
                      IEnumerable<long> digitInputs,
                      int digits = 14,
                      long value = 0,
                      Dictionary<(long z, long input), long?>? cache = default)
{
  if (digits == 0)
    return machine.State["z"] == 0 ? value : null;

  cache ??= new();

  var key = (machine.State.Z, digits);
  if (cache.TryGetValue(key, out var cached))
    return cached;

  var result = default(long?);
  foreach (var i in digitInputs)
  {
    var tail = Execute(machine, EnumSingle(i));
    var current = (value * 10) + i;
    result = FindModelNumber(tail, digitInputs, digits - 1, current, cache);
    if (result.HasValue)
      break;
  }

  cache[key] = result;
  return result;
}

IEnumerable<T> EnumSingle<T>(T value)
{
  yield return value;
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

record UnaryInstruction(Operation Operation, VariableParameter Param1) : Instruction(Operation) { }

record BinaryInstruction(Operation Operation, VariableParameter Param1, Parameter Param2) : Instruction(Operation) { }

enum Operation { Input, Add, Multiply, Divide, Modulus, Equals }
