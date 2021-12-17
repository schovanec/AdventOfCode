using System.Collections.Immutable;
using System.Globalization;
using System.Text;

var inputs = File.ReadLines(args.FirstOrDefault() ?? "input.txt").Select(ParseHex);

foreach (var input in inputs)
{
  var packet = Parse(new BitStream(input));
  Console.WriteLine($"Part 1 Result = {packet.SumVersions()}");
  Console.WriteLine($"Part 2 Result = {packet.Evaluate()}");
}

Packet Parse(BitStream stream)
{
  var version = stream.Take(3);
  var type = (PacketType)(int)stream.Take(3);

  return type switch
  {
    PacketType.Literal => ParseLiteral(version, stream),
    _ => ParseOperator(version, type, stream)
  };
}

LiteralPacket ParseLiteral(long version, BitStream stream)
{
  var value = 0L;
  var stop = false;
  while (!stop)
  {
    stop = stream.Take(1) == 0;
    value = (value << 4) | stream.Take(4);
  }

  return new LiteralPacket(version, value);
}

OperatorPacket ParseOperator(long version, PacketType type, BitStream stream)
{
  if (stream.Take(1) == 0)
  {
    var size = stream.Take(15);
    return new OperatorPacket(
      type,
      version,
      ParseList(stream, (bits, _) => bits < size));
  }
  else
  {
    var count = stream.Take(11);
    return new OperatorPacket(
      type,
      version,
      ParseList(stream, (_, n) => n < count));
  }
}

ImmutableList<Packet> ParseList(BitStream stream, Func<int, int, bool> shouldContinue)
{
  var result = ImmutableList.CreateBuilder<Packet>();

  var start = stream.Offset;
  while (shouldContinue(stream.Offset - start, result.Count))
    result.Add(Parse(stream));

  return result.ToImmutable();
}

byte[] ParseHex(string input)
{
  using var buffer = new MemoryStream();
  using var writer = new BinaryWriter(buffer, Encoding.Default, true);

  var span = input.AsSpan();
  for (var i = 0; i < input.Length; i += 2)
    writer.Write((byte)int.Parse(span.Slice(i, 2), NumberStyles.HexNumber));

  return buffer.ToArray();
}

class BitStream
{
  public BitStream(ReadOnlyMemory<byte> buffer, int offset = 0)
  {
    Buffer = buffer;
    Length = Buffer.Length * 8;
    Offset = offset;
  }

  private ReadOnlyMemory<byte> Buffer { get; }

  public int Length { get; }

  public int Offset { get; private set; }

  public long Take(int bits)
  {
    var span = Buffer.Span.Slice(Offset / 8);
    var shift = Offset % 8;
    var index = 0;
    long result = 0;
    var size = bits;
    while (size > 0)
    {
      var chunk = span[index];
      var chunkSize = 8;

      if (shift > 0)
      {
        chunk = (byte)((byte)(chunk << shift) >> shift);
        chunkSize -= shift;
      }

      if (size < (8 - shift))
      {
        chunk >>= (8 - shift - size);
        chunkSize = size;
      }

      result = (result << chunkSize) | chunk;
      size -= chunkSize;
      shift = 0;
      ++index;
    }

    Offset += bits;
    return result;
  }
}

abstract record Packet(PacketType Type, long Version)
{
  public virtual long SumVersions() => Version;

  public abstract long Evaluate();
}

record LiteralPacket(long Version, long Value) : Packet(PacketType.Literal, Version)
{
  public override long Evaluate() => Value;
}

record OperatorPacket(PacketType Type, long Version, ImmutableList<Packet> Operands) : Packet(Type, Version)
{
  public override long SumVersions()
    => Version + Operands.Sum(op => op.SumVersions());

  public override long Evaluate()
    => Type switch
    {
      PacketType.Sum => Operands.Sum(op => op.Evaluate()),
      PacketType.Product => Operands.Aggregate(1L, (prev, op) => prev * op.Evaluate()),
      PacketType.Min => Operands.Min(op => op.Evaluate()),
      PacketType.Max => Operands.Max(op => op.Evaluate()),
      PacketType.GreaterThan => Operands[0].Evaluate() > Operands[1].Evaluate() ? 1 : 0,
      PacketType.LessThan => Operands[0].Evaluate() < Operands[1].Evaluate() ? 1 : 0,
      PacketType.Equal => Operands[0].Evaluate() == Operands[1].Evaluate() ? 1 : 0,
      _ => throw new InvalidOperationException()
    };
}

enum PacketType { Sum, Product, Min, Max, Literal, GreaterThan, LessThan, Equal }