var input = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")
                .SelectMany(line => line.Split(','))
                .ToArray();

var result1 = input.Sum(HASH);
Console.WriteLine($"Part 1 Result = {result1}");

var boxes = PlaceLenses(input.Select(Step.Parse));
var powers = from i in Enumerable.Range(0, boxes.Count)
             let box = boxes[i]
             from j in Enumerable.Range(0, box.Count)
             let fp = box[j].FocalLength
             select (i + 1) * (j + 1) * fp;

var result2 = powers.Sum();
Console.WriteLine($"Part 2 Result = {result2}");

static int HASH(string input)
{
  return input.Aggregate(0, Step);

  int Step(int current, char ch)
    => ((current + (int)ch) * 17) % 256;
}

static List<List<Lens>> PlaceLenses(IEnumerable<Step> steps)
{
  var boxes = Enumerable.Range(0, 265)
                        .Select(_ => new List<Lens>())
                        .ToList();

  foreach (var step in steps)
  {
    var label = step.Label;
    var box = boxes[HASH(label)];

    switch (step)
    {
      case RemoveStep:
        box.RemoveAll(x => x.Label == label);
        break;

      case InsertStep(_, var focalLength):
        var lens = new Lens(label, focalLength);
        var pos = box.FindIndex(x => x.Label == lens.Label);
        if (pos < 0)
          box.Add(lens);
        else
          box[pos] = lens;
        break;
    }
  }

  return boxes;
}

record Lens(string Label, int FocalLength);

abstract record Step(string Label)
{
  public static Step Parse(string input)
  {
    int pos = input.IndexOfAny(['=', '-']);

    var label = input[..pos];
    return input[pos] switch
    {
      '-' => new RemoveStep(label),
      '=' => new InsertStep(label, int.Parse(input[(pos + 1)..])),
      _ => throw new FormatException()
    };
  }
}

record RemoveStep(string Label) : Step(Label);

record InsertStep(string Label, int FocalLength) : Step(Label);