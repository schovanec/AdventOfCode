using System.Collections;

var blueprints = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                     .Select(ParseInput)
                     .ToList();

var initialInventory = new Inventory(1);

var maxGeodeCounts1 = (from b in blueprints
                       select (id: b.Id, geodes: CalculateGeodes(b, initialInventory, 24))).ToList();
var qualityLevelSum = maxGeodeCounts1.Sum(x => x.id * x.geodes);
Console.WriteLine($"Part 1 Result = {qualityLevelSum}");

var maxGeodeCounts2 = (from b in blueprints.Take(3)
                       select (id: b.Id, geodes: CalculateGeodes(b, initialInventory, 32))).ToList();
var product = maxGeodeCounts2.Select(x => x.geodes).Aggregate((a, b) => a * b);
Console.WriteLine($"Part 2 Result = {product}");

static int CalculateGeodes(Blueprint blueprint, Inventory initialRobots, int time, bool debug = false)
{
  var maxCosts = Inventory.Create(blueprint.Rules.Values
                                           .SelectMany(x => x.EnumValues())
                                           .GroupBy(x => x.type, x => x.amount)
                                           .Select(g => (type: g.Key, amount: g.Max())));

  Dictionary<(Inventory, Inventory, int), int> cache = new();

  return Search(initialRobots, new(), time);

  int Search(Inventory robots, Inventory material, int remaining)
  {
    var cacheKey = CreateKey(robots, material, remaining);
    if (cache.TryGetValue(cacheKey, out var cached))
      return cached;

    var options = EnumBuildOptions(robots, material, remaining);
    var result = options.Select(o => Search(o.robots, o.materials, o.remaining))
                        .DefaultIfEmpty(material.Geode + (robots.Geode * remaining))
                        .Max();

    return cache[cacheKey] = result;
  }

  IEnumerable<(Inventory robots, Inventory materials, int remaining)> EnumBuildOptions(Inventory robots, Inventory material, int remaining)
  {
    foreach (var rule in blueprint.Rules)
    {
      if (ShouldBuild(rule.Key, robots, material, remaining))
      {
        var timeToBuild = (from r in rule.Value.EnumValues()
                           let needed = Math.Max(0, r.amount - material[r.type])
                           let growth = robots[r.type]
                           select needed switch
                           {
                             <= 0 => 0,
                             >= 0 when growth > 0 => (needed / growth) + ((needed % growth == 0) ? 0 : 1),
                             _ => int.MaxValue
                           }).DefaultIfEmpty(0).Max();

        if (timeToBuild < remaining)
        {
          var materialsBeforeBuild = material.Add(robots, timeToBuild + 1);
          var materialsAfterBuild = materialsBeforeBuild.Subtract(rule.Value);
          var robotsAfter = robots.Add(rule.Key, 1);
          var timeAfter = remaining - timeToBuild - 1;
          yield return (robotsAfter, materialsAfterBuild, timeAfter);
        }
      }
    }
  }

  (Inventory, Inventory, int) CreateKey(Inventory robots, Inventory material, int remaining)
  {
    var materialKey = material;
    foreach (var (type, _) in material.EnumValues())
    {
      if (!ShouldBuild(type, robots, material, remaining))
        materialKey = materialKey.Set(type, int.MaxValue);
    }

    return (robots, materialKey, remaining);
  }

  bool ShouldBuild(Resource robotType, Inventory currentRobots, Inventory currentMaterial, int remaining)
  {
    if (robotType == Resource.Geode)
      return true;

    var maxGain = currentRobots[robotType] * Math.Max(0, remaining - 1);
    var maxEndingAmount = currentMaterial[robotType] + maxGain;
    return maxEndingAmount <= (maxCosts[robotType] * remaining);
  }
}

static Blueprint ParseInput(string line)
{
  var split = line.Split(':', 2, StringSplitOptions.TrimEntries);
  var id = int.Parse(split[0].Split(' ', 2)[1]);

  Dictionary<Resource, Inventory> rules = new();
  foreach (var item in split[1].Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
  {
    var parts = item.Split(' ');
    var type = Enum.Parse<Resource>(parts[1], true);
    for (var i = 2; i < parts.Length; ++i)
    {
      if (int.TryParse(parts[i], out var amount))
      {
        ++i;
        rules[type] = rules.GetValueOrDefault(type).Add(Enum.Parse<Resource>(parts[i], true), amount);
      }
    }
  }

  return new Blueprint(id, rules);
}

record Blueprint(int Id, Dictionary<Resource, Inventory> Rules)
{
  public override string ToString()
    => $"{Id}: {string.Join(", ", Rules)}";
}

record struct Inventory(int Ore = 0, int Clay = 0, int Obsidian = 0, int Geode = 0)
//: IEnumerable<(Resource type, int amount)>
{
  public static Inventory Create(IEnumerable<(Resource type, int amount)> source)
    => source.Aggregate(default(Inventory), (i, x) => i.Add(x.type, x.amount));

  public int this[Resource type]
    => type switch
    {
      Resource.Ore => Ore,
      Resource.Clay => Clay,
      Resource.Obsidian => Obsidian,
      Resource.Geode => Geode,
      _ => 0
    };

  public Inventory Set(Resource type, int amount)
    => type switch
    {
      Resource.Ore => this with { Ore = amount },
      Resource.Clay => this with { Clay = amount },
      Resource.Obsidian => this with { Obsidian = amount },
      Resource.Geode => this with { Geode = amount },
      _ => this
    };

  public Inventory Add(Resource type, int amount)
    => Set(type, this[type] + amount);

  public Inventory Add(Inventory other, int factor = 1)
    => new(
      Ore + (other.Ore * factor),
      Clay + (other.Clay * factor),
      Obsidian + (other.Obsidian * factor),
      Geode + (other.Geode * factor));

  public Inventory Subtract(Inventory other) => Add(other, -1);

  public IEnumerable<(Resource type, int amount)> EnumValues()
  {
    if (Ore > 0)
      yield return (Resource.Ore, Ore);

    if (Clay > 0)
      yield return (Resource.Clay, Clay);

    if (Obsidian > 0)
      yield return (Resource.Obsidian, Obsidian);

    if (Geode > 0)
      yield return (Resource.Geode, Geode);
  }

  public override string ToString() => $"{{Or={Ore}, Cl={Clay}, Ob={Obsidian}, Ge={Geode}}}";
}

enum Resource { Ore, Clay, Obsidian, Geode }