using System.Collections;

var (rules, parts) = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var acceptedParts = parts.Where(p => EvaluatePart(p, rules) == "A");
var result1 = acceptedParts.Sum(GetPartTotalRating);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = CountAcceptedCombinations(rules);
Console.WriteLine($"Part 2 Result = {result2}");

static string EvaluatePart(Part<int> part, Dictionary<string, RuleSet> rules, string startRule = "in")
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

static int GetPartTotalRating(Part<int> part)
  => part.X + part.M + part.A + part.S;

static long CountAcceptedCombinations(Dictionary<string, RuleSet> allRuleSets, string startRule = "in", int min = 1, int max = 4000)
{
  var rb = new Bounds(min, max);
  var bounds = new Part<Bounds>(rb, rb, rb, rb);
  return SearchRulesByName(startRule, bounds);

  long SearchRulesByName(string name, Part<Bounds> bounds)
    => name switch
    {
      "A" => GetProductOfSizes(bounds),
      "R" => 0L,
      _ => SearchRules(allRuleSets[name].Rules, bounds)
    };

  long SearchRules(ReadOnlySpan<Rule> rules, Part<Bounds> bounds)
  {
    if (rules.IsEmpty)
      return 0L;

    switch (rules[0])
    {
      case AlwaysRule(var target):
        return SearchRulesByName(target, bounds);

      case ConditionRule(var key, _, _, var target) rule:
        var trueTotal = SearchRulesByName(
          target,
          bounds.With(key, rule.LimitBoundsForTrue(bounds[key])));

        var falseTotal = SearchRules(
          rules[1..],
          bounds.With(key, rule.LimitBoundsForFalse(bounds[key])));

        return trueTotal + falseTotal;

      default:
        return 0L;
    }
  }
}

static long GetProductOfSizes(Part<Bounds> bounds) => bounds.Aggregate(1L, (a, b) => a * b.Size);

static (Dictionary<string, RuleSet> rules, Part<int>[] parts) ParseInput(string[] input)
{
  var splitPos = Array.FindIndex(input, string.IsNullOrWhiteSpace);

  var rules = input[..splitPos].Select(RuleSet.Parse)
                               .ToDictionary(rs => rs.Name);

  var parts = input[(splitPos + 1)..].Select(ParsePart).ToArray();

  return (rules, parts);
}

static Part<int> ParsePart(string part)
{
  var ratings = part.Trim('{', '}')
                    .Split(',')
                    .Select(p => p.Split('=', 2))
                    .Select(p => (name: p[0], value: int.Parse(p[1])));

  return ratings.Aggregate(
    default(Part<int>),
    (a, p) => a.With(p.name[0], p.value));
}


record RuleSet(string Name, Rule[] Rules)
{
  public string Evaluate(Part<int> part)
    => Rules.First(r => r.IsTrueFor(part)).Target;

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
  public abstract bool IsTrueFor(Part<int> part);

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
  public override bool IsTrueFor(Part<int> part) => true;
}

record ConditionRule(char Key, char Op, int Value, string Target) : Rule(Target)
{
  public override bool IsTrueFor(Part<int> part)
    => Op switch
    {
      '<' => part[Key] < Value,
      '>' => part[Key] > Value,
      _ => false
    };

  public Bounds LimitBoundsForTrue(Bounds bounds)
    => Op switch
    {
      '<' => bounds.LimitMax(Value - 1),
      '>' => bounds.LimitMin(Value + 1),
      _ => bounds
    };

  public Bounds LimitBoundsForFalse(Bounds bounds)
    => Op switch
    {
      '<' => bounds.LimitMin(Value),
      '>' => bounds.LimitMax(Value),
      _ => bounds
    };

  public static ConditionRule Parse(string condition, string target)
  {
    var key = condition[0];
    var op = condition[1];
    var value = int.Parse(condition[2..]);
    return new(key, op, value, target);
  }
}

record struct Part<T>(T X, T M, T A, T S) : IEnumerable<T>
  where T : struct
{
  public readonly T this[char key] => key switch
  {
    'x' => X,
    'm' => M,
    'a' => A,
    's' => S,
    _ => default
  };

  public Part<T> With(char key, T value)
  => key switch
  {
    'x' => this with { X = value },
    'm' => this with { M = value },
    'a' => this with { A = value },
    's' => this with { S = value },
    _ => this
  };

  public readonly IEnumerator<T> GetEnumerator()
  {
    yield return X;
    yield return M;
    yield return A;
    yield return S;
  }

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

record struct Bounds(int Min, int Max)
{
  public readonly int Size => Max - Min + 1;

  public Bounds LimitMin(int limit)
    => this with { Min = Math.Max(limit, Min) };

  public Bounds LimitMax(int limit)
    => this with { Max = Math.Min(limit, Max) };
}
