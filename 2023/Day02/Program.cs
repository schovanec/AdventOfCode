using System.Collections.Immutable;

var games = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")
                .Select(Game.Parse)
                .ToImmutableArray();

var availableCubes = new Cubes(12, 13, 14);
var result1 = games.Where(g => g.Fits(availableCubes)).Sum(g => g.Id);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = games.Sum(g => g.GetMinCubes().Power);
Console.WriteLine($"Part 2 Result = {result2}");

record Game(int Id, ImmutableArray<Cubes> Draws)
{
  public bool Fits(Cubes other)
    => Draws.All(d => d.Fits(other));

  public Cubes GetMinCubes()
    => Draws.Aggregate((a, b) => a.Max(b));

  public static Game Parse(string input)
  {
    var parts = input.Split(':', StringSplitOptions.TrimEntries);
    var id = int.Parse(parts[0].Split(' ')[1]);
    var draws = parts[1].Split(';', StringSplitOptions.TrimEntries)
                        .Select(Cubes.Parse)
                        .ToImmutableArray();

    return new(id, draws);
  }
}

record struct Cubes(int Red = 0, int Green = 0, int Blue = 0)
{
  public static Cubes Empty = default;

  public readonly int Power => Red * Green * Blue;

  public readonly bool Fits(Cubes other)
    => Red <= other.Red
    && Green <= other.Green
    && Blue <= other.Blue;

  public readonly Cubes Max(Cubes other)
    => new(Math.Max(Red, other.Red),
           Math.Max(Green, other.Green),
           Math.Max(Blue, other.Blue));

  public Cubes Add(int count, string color)
    => color switch
    {
      "red" => this with { Red = Red + count },
      "green" => this with { Green = Green + count },
      "blue" => this with { Blue = Blue + count },
      _ => this
    };

  public static Cubes Parse(string input)
    => input.Split(',', StringSplitOptions.TrimEntries)
            .Select(x => x.Split(' ', 2))
            .Aggregate(Cubes.Empty, (a, n) => a.Add(int.Parse(n[0]), n[1]));
}