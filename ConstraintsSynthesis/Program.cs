using System.Linq;
using System.Windows.Forms;
using CommandLine;
using ConstraintsSynthesis.Algorithm;
using ConstraintsSynthesis.Model;
using ConstraintsSynthesis.Visualization;

namespace ConstraintsSynthesis
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var options = new Options();

            if (!Parser.Default.ParseArguments(args, options))
                return;

            var data = new Data();

            data.Load(options.InputFile, options.Delimiter);

            var xmeans = new XMeans();
            xmeans.Fit(data.Points);

            var clusters = xmeans.Clusters;

            var solutions = clusters.Select(c => new Solution(c)).ToArray();

            foreach (var solution in solutions)
            {
                solution.GenerateInitialSolution()
                    .GenerateImprovedInitialConstraints()
                .GenerateImprovingConstraints(5);
            }

            Application.EnableVisualStyles();
            Application.Run(new VisualizationApplicationContext(solutions, data.Dimensions));
        }
    }
}