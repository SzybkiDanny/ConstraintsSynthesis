using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using ConstraintsSynthesis.Model;

namespace ConstraintsSynthesis
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var options = new Options();

            if (!Parser.Default.ParseArguments(args, options))
                return;

            var data = LoadData(options.InputFile, options.Delimiter);
        }

        private static IList<Point> LoadData(string fileName, string delimiter)
        {
            var data = new List<Point>();

            using (var file = new StreamReader(fileName))
            {
                while (!file.EndOfStream)
                {
                    var values =
                        file.ReadLine()?.Split(new[] {delimiter}, StringSplitOptions.None).Select(double.Parse);
                    var point = new Point(values.Take(values.Count() - 1).ToArray())
                    {
                        Label = Math.Abs(values.Last() - 1.0) < double.Epsilon
                    };

                    data.Add(point);
                }
            }

            return data;
        }
    }
}