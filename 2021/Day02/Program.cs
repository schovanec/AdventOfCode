var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(x => x.Split(' '))
                .Select(x => (action: x[0], value: int.Parse(x[1])))
                .ToList();

var (h1, d1) = input.Aggregate(
  (h: 0, d: 0),
  (pos, cmd) => cmd switch
  {
    ("forward", var x) => (pos.h + x, pos.d),
    ("down", var x) => (pos.h, pos.d + x),
    ("up", var x) => (pos.h, pos.d - x),
    _ => pos
  });

Console.WriteLine($"Part 1 Result = {h1 * d1}");

var (h2, d2, _) = input.Aggregate(
  (h: 0, d: 0, aim: 0),
  (pos, cmd) => cmd switch
  {
    ("down", var x) => (pos.h, pos.d, pos.aim + x),
    ("up", var x) => (pos.h, pos.d, pos.aim - x),
    ("forward", var x) => (pos.h + x, pos.d + (pos.aim * x), pos.aim),
    _ => pos
  }
);

Console.WriteLine($"Part 2 Result = {h2 * d2}");