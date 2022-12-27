var (map, steps) = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var pos1 = map.GetStart();
foreach (var item in steps)
{
  switch (item)
  {
    case Move(var n):
      pos1 = map.Move(pos1, n);
      break;

    case Turn(var dir):
      pos1 = pos1.Turn(dir);
      break;
  }
}

var result1 = CalculatePassword(pos1);
Console.WriteLine($"Part 1 Result = {result1}");

var size = int.Parse(args.Skip(1).FirstOrDefault() ?? "50");
var map2 = CubeMap.Create(map, size);

var pos2 = map2.GetStart();
foreach (var item in steps)
{
  switch (item)
  {
    case Move(var n):
      pos2 = map2.Move(pos2, n);
      break;

    case Turn(var dir):
      pos2 = pos2.Turn(dir);
      break;
  }
}

var result2 = CalculatePassword(pos2);
Console.WriteLine($"Part 2 Result = {result2}");

static int CalculatePassword(Position pos)
  => (1000 * (pos.Row + 1)) + (4 * (pos.Column + 1)) + (int)pos.Facing;

static (Map map, IReadOnlyList<Instruction>) ParseInput(string[] input)
{
  List<string> rows = new();

  int i;
  for (i = 0; i < input.Length; ++i)
  {
    if (string.IsNullOrEmpty(input[i]))
      break;

    rows.Add(input[i]);
  }

  var actions = ParseActions(input[i + 1]).ToList();

  return (new Map(rows.AsReadOnly()), actions.AsReadOnly());
}

static IEnumerable<Instruction> ParseActions(string actions)
{
  var i = 0;
  while (i < actions.Length)
  {
    var end = actions.IndexOfAny(new[] { 'L', 'R' }, i);
    if (end < 0)
      end = actions.Length;

    if (end > i)
      yield return new Move(int.Parse(actions[i..end]));

    if (end < actions.Length)
      yield return new Turn(actions[end]);

    i = end + 1;
  }
}

record struct Position(int Row, int Column, Direction Facing)
{
  public Position MoveNext()
    => Facing switch
    {
      Direction.Right => this with { Column = Column + 1 },
      Direction.Down => this with { Row = Row + 1 },
      Direction.Left => this with { Column = Column - 1 },
      Direction.Up => this with { Row = Row - 1 },
      _ => this
    };

  public Position Turn(char direction)
    => direction switch
    {
      'R' => this with { Facing = (Direction)((((int)Facing) + 1) % 4) },
      'L' => this with { Facing = (Direction)((((int)Facing) + 3) % 4) },
      _ => this
    };
}

record Map(IReadOnlyList<string> Rows)
{
  const char OpenTile = '.';
  const char WallTile = '#';

  public int Width => Rows.Max(r => r.Length);

  public int Height = Rows.Count;

  public Position GetStart()
    => new(0, Rows[0].IndexOf('.'), Direction.Right);

  public Tile GetTile(int row, int col)
  {
    if (!IsInGrid(row, col))
      return Tile.None;

    return Rows[row][col] switch
    {
      OpenTile => Tile.Open,
      WallTile => Tile.Wall,
      _ => Tile.None
    };
  }

  public Position Move(Position current, int count)
  {
    var result = current;
    while (count > 0)
    {
      --count;
      var next = Step(result);
      if (result == next)
        break;

      result = next;
    }

    return result;
  }

  private Position Step(Position pos)
  {
    var next = pos.MoveNext();
    var tile = GetTile(next.Row, next.Column);
    if (tile == Tile.None)
    {
      next = Wrap(next);
      tile = GetTile(next.Row, next.Column);
    }

    return tile == Tile.Wall ? pos : next;
  }

  private Position Wrap(Position pos)
  {
    var result = pos.Facing switch
    {
      Direction.Right => pos with { Column = 0 },
      Direction.Down => pos with { Row = 0 },
      Direction.Left => pos with { Column = Rows[pos.Row].Length - 1 },
      Direction.Up => pos with { Row = Rows.Count - 1 },
      _ => throw new InvalidOperationException()
    };

    while (GetTile(result.Row, result.Column) == Tile.None)
      result = result.MoveNext();

    return result;
  }

  private bool IsInGrid(int row, int col)
    => row >= 0 && row < Rows.Count
    && col >= 0 && col < Rows[row].Length;
}

record CubeMap(Map BaseMap, Dictionary<Face, CubeFace> Layout)
{
  public Position GetStart() => BaseMap.GetStart();

  public Face GetFace(int row, int col)
    => Layout.Where(f => f.Value.Contains(row, col))
             .Select(f => f.Key)
             .FirstOrDefault(Face.None);

  public Position Move(Position current, int count)
  {
    var result = current;
    while (count > 0)
    {
      --count;
      var next = Step(result);
      if (result == next)
        break;

      result = next;
    }

    return result;
  }

  private Position Step(Position pos)
  {
    var next = pos.MoveNext();
    var face = GetFace(next.Row, next.Column);
    if (face == Face.None)
    {
      var fromFace = GetFace(pos.Row, pos.Column);
      next = Wrap(next, fromFace);
    }

    var tile = BaseMap.GetTile(next.Row, next.Column);
    return tile == Tile.Wall ? pos : next;
  }

  private Position Wrap(Position pos, Face from)
  {
    var size = Layout[from].Width;
    var max = size - 1;
    var (row, col, facing) = MapToFace(pos, from);
    return (size, from, facing) switch
    {
      // small (example)
      (4, Face.Front, Direction.Right) => FaceToMap(new(max - row, max - (col - size), Direction.Left), Face.Right),
      (4, Face.Front, Direction.Up) => FaceToMap(new(-(row + 1), max - col, Direction.Down), Face.Top),
      (4, Face.Front, Direction.Left) => FaceToMap(new(-(col + 1), row, Direction.Down), Face.Left),
      (4, Face.Left, Direction.Up) => FaceToMap(new(col, -(row + 1), Direction.Right), Face.Front),
      (4, Face.Left, Direction.Down) => FaceToMap(new(max - col, row - max, Direction.Right), Face.Rear),
      (4, Face.Top, Direction.Up) => FaceToMap(new(-(row + 1), max - col, Direction.Down), Face.Front),
      (4, Face.Top, Direction.Left) => FaceToMap(new(size + col, row, Direction.Up), Face.Right),
      (4, Face.Top, Direction.Down) => FaceToMap(new(max - (row - size), max - col, Direction.Up), Face.Rear),
      (4, Face.Bottom, Direction.Right) => FaceToMap(new(col - size, max - row, Direction.Down), Face.Right),
      (4, Face.Rear, Direction.Left) => FaceToMap(new(size + col, max - row, Direction.Up), Face.Left),
      (4, Face.Rear, Direction.Down) => FaceToMap(new(max - (row - size), max - col, Direction.Up), Face.Top),
      (4, Face.Right, Direction.Up) => FaceToMap(new(max - col, size + row, Direction.Left), Face.Bottom),
      (4, Face.Right, Direction.Right) => FaceToMap(new(max - row, max - (col - size), Direction.Left), Face.Front),
      (4, Face.Right, Direction.Down) => FaceToMap(new(max - col, row - size, Direction.Right), Face.Top),

      // full (input)
      (50, Face.Top, Direction.Up) => FaceToMap(new(col, -(row + 1), Direction.Right), Face.Rear),
      (50, Face.Top, Direction.Left) => FaceToMap(new(max - row, -(col + 1), Direction.Right), Face.Left),
      (50, Face.Right, Direction.Up) => FaceToMap(new(size + row, col, Direction.Up), Face.Rear),
      (50, Face.Right, Direction.Right) => FaceToMap(new(max - row, max - (col - size), Direction.Left), Face.Bottom),
      (50, Face.Right, Direction.Down) => FaceToMap(new(col, max - (row - size), Direction.Left), Face.Front),
      (50, Face.Front, Direction.Left) => FaceToMap(new(-(col + 1), row, Direction.Down), Face.Left),
      (50, Face.Front, Direction.Right) => FaceToMap(new(max - (col - size), row, Direction.Up), Face.Right),
      (50, Face.Left, Direction.Up) => FaceToMap(new(col, -(row + 1), Direction.Right), Face.Front),
      (50, Face.Left, Direction.Left) => FaceToMap(new(max - row, -(col + 1), Direction.Right), Face.Top),
      (50, Face.Bottom, Direction.Right) => FaceToMap(new(max - row, max - (col - size), Direction.Left), Face.Right),
      (50, Face.Bottom, Direction.Down) => FaceToMap(new(col, max - (row - size), Direction.Left), Face.Rear),
      (50, Face.Rear, Direction.Left) => FaceToMap(new(-(col + 1), row, Direction.Down), Face.Top),
      (50, Face.Rear, Direction.Right) => FaceToMap(new(max - (col - size), row, Direction.Up), Face.Bottom),
      (50, Face.Rear, Direction.Down) => FaceToMap(new(row - size, col, Direction.Down), Face.Right),
      _ => throw new InvalidOperationException()
    };
  }

  private Position MapToFace(Position pos, Face face)
  {
    var (startRow, startCol, _, _) = Layout[face];
    return pos with { Row = pos.Row - startRow, Column = pos.Column - startCol };
  }

  private Position FaceToMap(Position pos, Face face)
  {
    var (startRow, startCol, _, _) = Layout[face];
    return pos with { Row = pos.Row + startRow, Column = pos.Column + startCol };
  }

  public static CubeMap Create(Map map, int size)
  {
    Dictionary<Face, CubeFace> layout = new();
    foreach (var (face, row, col) in layouts[size])
    {
      var startRow = (row * size);
      var startCol = (col * size);
      layout[face] = new(startRow, startCol, startRow + size, startCol + size);
    }

    return new(map, layout);
  }

  private static readonly Dictionary<int, List<(Face face, int row, int col)>> layouts = new()
  {
    [4] = new()
    {
      (Face.Front, 0, 2),
      (Face.Bottom, 1, 2),
      (Face.Left, 1, 1),
      (Face.Top, 1, 0),
      (Face.Rear, 2, 2),
      (Face.Right, 2, 3)
    },
    [50] = new()
    {
      (Face.Top, 0, 1),
      (Face.Right, 0, 2),
      (Face.Front, 1, 1),
      (Face.Left, 2, 0),
      (Face.Bottom, 2, 1),
      (Face.Rear, 3, 0)
    }
  };
}

record CubeFace(int StartRow, int StartCol, int EndRow, int EndCol)
{
  public bool Contains(int row, int col)
    => row >= StartRow
    && row < EndRow
    && col >= StartCol
    && col < EndCol;

  public int Width => EndCol - StartCol;

  public int Height => EndRow - StartRow;
}

abstract record Instruction();

record Move(int Steps) : Instruction;

record Turn(char Direction) : Instruction;

enum Direction { Right, Down, Left, Up }

enum Tile { None, Open, Wall }

enum Face { None, Front, Rear, Top, Bottom, Left, Right }