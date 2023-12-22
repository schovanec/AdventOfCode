// using Bounds = (int min, int max);

var (rules, parts) = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var acceptedParts = parts.Where(p => EvaluatePart(p, rules) == "A");
var result1 = acceptedParts.Sum(p => p.TotalRating);
Console.WriteLine($"Part 1 Result = {result1}");

static string EvaluatePart(Part part, Dictionary<string, RuleSet> rules, string startRule = "in")
{
  var current = rules[startRule];
  while (current != null)
  {
    var nextRule = current.Evaluate(part);
    if (nextRule is "R" or "A")
      return nextRule;

    current = rules[nextRule];
  }

  return "";
}

// static long CountAcceptedCombinations(Dictionary<string, RuleSet> rules, string startRule = "in")
// {

// }

static (Dictionary<string, RuleSet> rules, Part[] parts) ParseInput(string[] input)
{
  var splitPos = Array.FindIndex(input, string.IsNullOrWhiteSpace);

  var rules = input[..splitPos].Select(RuleSet.Parse)
                               .ToDictionary(rs => rs.Name);

  var parts = input[(splitPos + 1)..].Select(Part.Parse).ToArray();

  return (rules, parts);
}

record RuleSet(string Name, Rule[] Rules)
{
  public string Evaluate(Part part)
    => Rules.First(r => r.IsValidFor(part)).Target;

  public override string ToString()
    => $"{Name}: [{string.Join(", ", Rules.AsEnumerable())}]";

  public static RuleSet Parse(string input)
  {
    var parts = input.Split('{', 2);
    var rules = parts[1].TrimEnd('}')
                        .Split(',')
                        .Select(Rule.Parse)
                        .ToArray();

    return new(parts[0], rules);
  }
}

abstract record Rule(string Target)
{
  public abstract bool IsValidFor(Part part);

  public static Rule Parse(string input)
  {
    var parts = input.Split(':', 2);
    return parts switch
    {
    [var condition, var target] => ConditionRule.Parse(condition, target),
    [var target] => new AlwaysRule(target),
      _ => throw new FormatException()
    };
  }
}

record AlwaysRule(string Target) : Rule(Target)
{
  public override bool IsValidFor(Part part) => true;
}

record ConditionRule(char Key, char Op, int Value, string Target) : Rule(Target)
{
  public override bool IsValidFor(Part part)
    => Op switch
    {
      '<' => part[Key] < Value,
      '>' => part[Key] > Value,
      _ => false
    };

  public static ConditionRule Parse(string condition, string target)
  {
    var key = condition[0];
    var op = condition[1];
    var value = int.Parse(condition[2..]);
    return new(key, op, value, target);
  }
}

record struct Part(int X, int M, int A, int S)
{
  public readonly int this[char key] => key switch
  {
    'x' => X,
    'm' => M,
    'a' => A,
    's' => S,
    _ => default
  };

  public readonly int TotalRating => X + M + A + S;

  public static Part Parse(string part)
  {
    var ratings = part.Trim('{', '}')
                      .Split(',')
                      .Select(p => p.Split('=', 2))
                      .Select(p => (name: p[0], value: int.Parse(p[1])));

    return ratings.Aggregate(
      default(Part),
      (a, p) => p.name switch
      {
        "x" => a with { X = p.value },
        "m" => a with { M = p.value },
        "a" => a with { A = p.value },
        "s" => a with { S = p.value },
        _ => a
      });
  }
}