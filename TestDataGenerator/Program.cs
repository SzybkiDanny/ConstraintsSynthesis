using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using TestDataGenerator.Generators;
using TestDataGenerator.Model;

namespace TestDataGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var options = new Options();

            if (!Parser.Default.ParseArguments(args, options))
                return;

            if (options.Center != null && options.Dimensions != options.Center.Length)
                throw new ArgumentException("Center elements count must be equal to Dimensions option value");

            if (options.Center == null)
                options.Center = new double[options.Dimensions];

            IShapeGenerator generator;
            switch (options.Shape)
            {
                case Shapes.Sphere:
                    generator = new SphereGenerator();
                    break;
                case Shapes.SphereFromUniform:
                    generator = new SphereFromUniformGenerator();
                    break;
                case Shapes.MultipleClouds:
                    generator = new MultipleCloudsGenerator();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(args));
            }

            var data = generator.Generate(options.Dimensions, options.Radius, options.Center,
                options.Positives, options.Negatives, options.Multiplicity);

            WriteToFile(data, options.OutputFile, options.Delimiter);
        }

        private static void WriteToFile(IList<Point> data, string fileName, string delimiter)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            using (var file = new StreamWriter(fileName))
            {
                foreach (var point in data)
                {
                    file.WriteLine(
                        $"{string.Join(delimiter, point.Coordinates)}{delimiter}{(point.Label ? 1 : 0)}");
                }
            }
        }
    }
}