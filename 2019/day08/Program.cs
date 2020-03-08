using System;
using System.IO;
using System.Linq;

namespace day08
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.FirstOrDefault() ?? "input.txt";
            var data = File.ReadLines(file).First().AsMemory();
            const int width = 25;
            const int height = 6;
            const int layerSize = width * height;

            var layerCount = data.Length / layerSize;
            var layers = Enumerable.Range(0, layerCount).Select(i => data.Slice(i * layerSize, layerSize)).ToArray();

            Console.WriteLine($"Number of Layers = {layerCount}");

            var targetLayer = layers.OrderBy(x => x.Span.Count(x => x == '0')).First().Span;

            for (var y = 0; y < height; ++y)
                Console.WriteLine(targetLayer.Slice(y * width, width).ToString());

            var countOf1 = targetLayer.Count(ch => ch == '1');
            var countOf2 = targetLayer.Count(ch => ch == '2');
            Console.WriteLine($"Count of 1 = {countOf1}");
            Console.WriteLine($"Count of 2 = {countOf2}");

            var result = countOf1 * countOf2;
            Console.WriteLine($"Result = {result}");
        }

    }

    static class SpanExtensions
    {
        public static int Count(this ReadOnlySpan<char> source, Predicate<char> predicate)
        {
            var result = 0;
            foreach (var ch in source)
            {
                if (predicate(ch))
                    ++result;
            }

            return result;
        }
    }
}
