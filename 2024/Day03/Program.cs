using System.Text.RegularExpressions;

var memory = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt");

var result1 = ParseMultiplyPairs(memory).Sum(p => p.a * p.b);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = ParseMultiplyPairs(memory, true).Sum(p => p.a * p.b);
Console.WriteLine($"Part 1 Result = {result2}");

static IEnumerable<(int a, int b)> ParseMultiplyPairs(IEnumerable<string> memory, bool conditionals = false)
{
  var pattern = new Regex(@"(?<op>do)\(\)|(?<op>don't)\(\)|(?<op>mul)\((?<a>\d{1,3}),(?<b>\d{1,3})\)", RegexOptions.Singleline);
  var matches = memory.SelectMany(x => pattern.Matches(x))
                      .Cast<Match>();

  bool enabled = true;
  foreach (var m in matches)
  {
    Console.WriteLine(m.Groups["op"].Value);
    switch (m.Groups["op"].Value)
    {
      case "do":
        enabled = true;
        break;

      case "don't":
        enabled = false;
        break;

      case "mul":
        if (!conditionals || enabled)
          yield return (int.Parse(m.Groups["a"].ValueSpan), int.Parse(m.Groups["b"].ValueSpan));
        break;
    }
  }
}