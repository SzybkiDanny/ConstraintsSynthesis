using CommandLine;
using CommandLine.Text;

namespace ConstraintsSynthesis
{
    internal class Options
    {
        [Option('o', "output", Required = false,
            HelpText = "Output file name.")]
        public string OutputFile { get; set; }

        [Option('i', "input", Required = true,
            HelpText = "Input file name.")]
        public string InputFile { get; set; }

        [Option("delimiter", DefaultValue = " ",
            HelpText = "Data delimiter in input file.")]
        public string Delimiter { get; set; }

        [Option("seed", HelpText = "Seed for all random operations.")]
        public int? Seed { get; set; }

        [HelpOption]
        public string GetUsage() => HelpText.AutoBuild(this,
            current => HelpText.DefaultParsingErrorsHandler(this, current));
    }
}