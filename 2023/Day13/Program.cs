var input = ParseInput(File.ReadLines(args.FirstOrDefault() ?? "input.txt")).ToArray();

var result1 = input.Sum(x => (FindHorizontalReflection(x, false) * 100) ?? FindVerticalReflection(x, false));
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = input.Sum(x => (FindHorizontalReflection(x, true) * 100) ?? FindVerticalReflection(x, true));
Console.WriteLine($"Part 2 Result = {result2}");

static int? FindHorizontalReflection(List<string> pattern, bool allowSmudge)
{
  return (from pos in Enumerable.Range(1, pattern.Count - 1)
          where IsReflection(pos, pattern, allowSmudge)
          select (int?)pos).FirstOrDefault();
}

static int? FindVerticalReflection(List<string> pattern, bool allowSmudge)
  => FindHorizontalReflection(Transpose(pattern), allowSmudge);

static bool IsReflection(int pos, List<string> pattern, bool allowSmudge)
{
  var badPairs = from i in Enumerable.Range(0, Math.Min(pos, pattern.Count - pos))
                 let top = pattern[pos - i - 1]
                 let bot = pattern[pos + i]
                 where top != bot
                 select (top, bot);

  if (!allowSmudge)
    return !badPairs.Any();

  if (badPairs.ToArray() is [(var badTop, var badBot)])
    return badTop.Zip(badBot).Count(x => x.First != x.Second) == 1;
  else
    return false;
}

static List<string> Transpose(List<string> pattern)
{
  var w = pattern[0].Length;
  return Enumerable.Range(0, w)
                   .Select(i => string.Concat(pattern.Select(x => x[i])))
                   .ToList();
}

IEnumerable<List<string>> ParseInput(IEnumerable<string> lines)
{
  List<string>? current = null;

  foreach (var line in lines)
  {
    if (string.IsNullOrEmpty(line))
    {
      if (current != null)
      {
        yield return current;
        current = null;
      }
    }
    else
    {
      if (current == null)
        current = new();

      current.Add(line);
    }
  }

  if (current != null)
    yield return current;
}