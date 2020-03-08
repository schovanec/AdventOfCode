using System;
using System.Collections.Generic;
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

            var image = MergeLayers(layers);

            Console.WriteLine();
            Console.WriteLine("Image:");
            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                    Console.Write(image[y * width + x] ? 'X' : ' ');

                Console.WriteLine();
            }
        }

        private static bool[] MergeLayers(IEnumerable<ReadOnlyMemory<char>> layers)
        {
            bool[] image = null;
            bool[] mask = null;

            const char black = '0';
            const char white = '1';

            foreach (var layer in layers)
            {
                var data = layer.Span;
                image ??= new bool[layer.Length];
                mask ??= new bool[layer.Length];

                for (var i = 0; i < image.Length; ++i)
                {
                    if (!mask[i] && (data[i] == black || data[i] == white))
                    {
                        mask[i] = true;
                        image[i] = (data[i] == white);
                    }
                }
            }

            return image;
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
