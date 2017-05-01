using CommandLine;
using CommandLine.Text;
using TestDataGenerator.Model;

namespace TestDataGenerator
{
    internal class Options
    {
        [Option('s', "shape", Required = true,
            HelpText = "Shape of the points to be generated.")]
        public Shapes Shape { get; set; }

        [Option('p', "positives", DefaultValue = 1000,
            HelpText = "Count of the positive examples (inside the shape).")]
        public int Positives { get; set; }

        [Option('n', "negatives", DefaultValue = 0,
            HelpText = "Count of the negative examples (outside the shape).")]
        public int Negatives { get; set; }

        [Option('d', "dimensions", DefaultValue = 2,
            HelpText = "Number of dimensions of the data to be generated.")]
        public int Dimensions { get; set; }

        [Option('o', "output", Required = true,
            HelpText = "Output file name.")]
        public string OutputFile { get; set; }

        [Option("delimiter", DefaultValue = " ",
            HelpText = "Data delimiter in output file.")]
        public string Delimiter { get; set; }

        [Option('r', "radius", DefaultValue = 1.0,
            HelpText = "Radius of the sphere (in case of sphere generation).")]
        public double Radius { get; set; }

        [OptionArray('c', "center",
            HelpText = "Origin of the generated shape.")]
        public double[] Center { get; set; }

        [Option('m', "multiplicity", DefaultValue = 2,
            HelpText = "Number of shapes to generate.")]
        public int Multiplicity { get; set; }

        [HelpOption]
        public string GetUsage() => HelpText.AutoBuild(this,
            current => HelpText.DefaultParsingErrorsHandler(this, current));
    }
}