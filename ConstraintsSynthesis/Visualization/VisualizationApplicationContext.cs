using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ConstraintsSynthesis.Model;

namespace ConstraintsSynthesis.Visualization
{
    internal class VisualizationApplicationContext : ApplicationContext
    {

        public VisualizationApplicationContext(IEnumerable<Solution> solutions, int dimensions)
        {
            var enumerableSolutions = solutions as Solution[] ?? solutions.ToArray();
            var forms = new List<Form>();

            for (var i = 0; i < dimensions; i++)
            {
                for (var j = i + 1; j < dimensions; j++)
                {
                    forms.Add(new VisualizationForm(
                        enumerableSolutions.Select(s => new SolutionVisualization(s, i, j)).ToArray()));
                }
            }

            foreach (var form in forms)
            {
                form.FormClosed += OnFormClosed;
            }

            foreach (var form in forms)
            {
                form.Show();
            }
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            if (Application.OpenForms.Count == 0)
            {
                ExitThread();
            }
        }
    }
}