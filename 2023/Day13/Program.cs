var input = ParseInput(File.ReadLines(args.FirstOrDefault() ?? "input.txt")).ToArray();

var result1 = input.Sum(x => (FindHorizontalReflection(x) * 100) ?? FindVerticalReflection(x));
Console.WriteLine($"Part 1 Result = {result1}");

static int? FindHorizontalReflection(List<string> pattern)
{
  return (from pos in Enumerable.Range(1, pattern.Count - 1)
          where IsReflection(pos, pattern)
          select (int?)pos).FirstOrDefault();
}

static int? FindVerticalReflection(List<string> pattern)
  => FindHorizontalReflection(Transpose(pattern));

static bool IsReflection(int pos, List<string> pattern)
  => !(from i in Enumerable.Range(0, Math.Min(pos, pattern.Count - pos))
       let top = pos - i - 1
       let bot = pos + i
       where pattern[top] != pattern[bot]
       select i).Any();

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