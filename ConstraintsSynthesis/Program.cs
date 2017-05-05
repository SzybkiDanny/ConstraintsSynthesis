using System;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using Accord.Math.Random;
using CommandLine;
using ConstraintsSynthesis.Algorithm;
using ConstraintsSynthesis.Model;
using ConstraintsSynthesis.Visualization;

namespace ConstraintsSynthesis
{
    internal class Program
    {
        public static int Seed;

        private static void Main(string[] args)
        {
            var options = new Options();

            if (!Parser.Default.ParseArguments(args, options))
                return;

            Generator.Seed = Seed = options.Seed ?? DateTime.Now.Millisecond;

            var data = new Data();

            data.Load(options.InputFile, options.Delimiter);

            var xmeans = new XMeans();
            xmeans.Fit(data.Points);

            var clusters = xmeans.Clusters;

            var solutions = clusters.Select(c => new Solution(c)).ToArray();

            foreach (var solution in solutions)
            {
                solution.GenerateInitialSolution()
                    .GenerateImprovedInitialConstraints(5)
                    .GenerateImprovingConstraints()
                    .RemoveRedundantConstraints();
            }

            Application.EnableVisualStyles();
            Application.Run(new VisualizationApplicationContext(solutions, data.Dimensions));
        }
    }
}