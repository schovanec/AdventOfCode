using System.Collections.Immutable;

var (template, rules) = Parse(File.ReadLines(args.FirstOrDefault() ?? "input.txt"));

var result1 = RunInsertion(10, template, rules);
Console.WriteLine($"Part 1 Result = {result1.Max - result1.Min}");

var result2 = RunInsertion(40, template, rules);
Console.WriteLine($"Part 2 Result = {result2.Max - result2.Min}");

Polymer RunInsertion(int steps, string template, ImmutableDictionary<(char a, char b), char> rules)
{
  var cache = new Dictionary<(int steps, (char a, char b) pair), Polymer>();
  var inserted = template.Zip(template.Skip(1))
                         .Select(pair => RunPairInsertion(steps, pair, rules, cache))
                         .SelectMany(p => p.Elements);

  return Polymer.FromTemplate(template).Add(inserted);
}

Polymer RunPairInsertion(int steps,
                         (char a, char b) pair,
                         ImmutableDictionary<(char a, char b), char> rules,
                         Dictionary<(int steps, (char a, char b) pair), Polymer> cache)
{
  Polymer? result;
  if (!cache.TryGetValue((steps, pair), out result))
  {
    var ch = rules[pair];
    result = Polymer.FromElement(ch);
    if (steps > 1)
    {
      var left = RunPairInsertion(steps - 1, (pair.a, ch), rules, cache);
      var right = RunPairInsertion(steps - 1, (ch, pair.b), rules, cache);
      result = result.Add(left.Elements.Concat(right.Elements));
    }

    cache[(steps, pair)] = result;
  }

  return result;
}

(string template, ImmutableDictionary<(char a, char b), char> rules) Parse(IEnumerable<string> input)
{
  string? template = null;
  var rules = ImmutableDictionary.CreateBuilder<(char a, char b), char>();

  foreach (var item in input)
  {
    if (template == null)
    {
      template = item;
    }
    else
    {
      var split = item.Split(" -> ", StringSplitOptions.RemoveEmptyEntries);
      if (split.Length == 2)
      {
        rules[(split[0][0], split[0][1])] = split[1][0];
      }
    }
  }

  if (template == null)
    throw new InvalidOperationException();

  return (template, rules.ToImmutable());
}

record class Polymer(ImmutableDictionary<char, long> Elements)
{
  public long Min => Elements.Values.Min();

  public long Max => Elements.Values.Max();

  public Polymer Add(IEnumerable<KeyValuePair<char, long>> elements)
    => new Polymer(Elements.Concat(elements)
                           .GroupBy(x => x.Key)
                           .ToImmutableDictionary(g => g.Key, g => g.Sum(x => x.Value)));

  public static Polymer FromTemplate(string template)
    => new Polymer(template.GroupBy(ch => ch).ToImmutableDictionary(g => g.Key, g => g.LongCount()));

  public static Polymer FromElement(char element)
    => new Polymer(ImmutableDictionary<char, long>.Empty.Add(element, 1));
}