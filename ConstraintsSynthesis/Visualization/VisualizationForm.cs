using System;
using System.Linq;
using System.Windows.Forms;
using OxyPlot;

namespace ConstraintsSynthesis.Visualization
{
    public partial class VisualizationForm : Form
    {
        private readonly SolutionVisualization[] _solutionVisualizations;

        public VisualizationForm(SolutionVisualization[] solutionVisualizations)
        {
            _solutionVisualizations = solutionVisualizations;
            InitializeComponent();
        }

        private void AssignPlot(object sender, EventArgs e)
        {
            plot.Model = CreatePlotModel();
        }

        private PlotModel CreatePlotModel(bool includeNegativePoints = true)
        {
            var plotModel = new PlotModel
            {
                Title =
                    $"Constraints Synthesis x{_solutionVisualizations.First().XIndex}, " +
                    $"x{_solutionVisualizations.First().YIndex}",
                PlotType = PlotType.Cartesian
            };

            foreach (var solutionVisualization in _solutionVisualizations)
            {
                plotModel.Series.Add(solutionVisualization.GetPointsSeriers());

                if (includeNegativePoints)
                    plotModel.Series.Add(solutionVisualization.GetNegativePointsSeriers());
            }

            foreach (
                var constraintsSeries in
                    _solutionVisualizations.SelectMany(
                        solutionVisualization => solutionVisualization.GetConstraintsSeries(trimConstraints:false)))
            {
                plotModel.Series.Add(constraintsSeries);
            }

            return plotModel;
        }
    }
}
