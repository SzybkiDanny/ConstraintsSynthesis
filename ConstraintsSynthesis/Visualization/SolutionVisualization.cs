﻿using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Random;
using OxyPlot;
using OxyPlot.Series;

namespace ConstraintsSynthesis.Visualization
{
    public class SolutionVisualization
    {
        public Solution Solution { get; }
        public int XIndex { get; }
        public int YIndex { get; }
        public double MinX { get; }
        public double MaxX { get; }
        public double MinY { get; }
        public double MaxY { get; }
        public OxyColor Color { get; }

        public SolutionVisualization(Solution solution, int xIndex, int yIndex)
        {
            Solution = solution;
            XIndex = xIndex;
            YIndex = yIndex;
            MinX = Solution.Cluster.Points.Min(p => p[XIndex]);
            MaxX = Solution.Cluster.Points.Max(p => p[XIndex]);
            MinY = Solution.Cluster.Points.Min(p => p[YIndex]);
            MaxY = Solution.Cluster.Points.Max(p => p[YIndex]);
            Color = GetRandomColor();
        }

        public ScatterSeries GetPointsSeriers()
        {
            var clusterPoints = new ScatterSeries {MarkerFill = Color};

            clusterPoints.Points.AddRange(
                Solution.Cluster.Points.Select(
                    p => new ScatterPoint(p[XIndex], p[YIndex], 2)));

            return clusterPoints;
        }

        public IEnumerable<Series> GetConstraintsSeries()
        {
            foreach (var constraint in Solution.Constraints)
            {
                if (Math.Abs(constraint[YIndex]) > double.Epsilon)
                {
                    var minX = MinX;
                    var maxX = MaxX;
                    var deltaX = 0.0;

                    var leftY = ConstraintFunctionValue(constraint, minX);
                    var rightY = ConstraintFunctionValue(constraint, maxX);

                    if ((leftY > MaxY || leftY < MinY) && (rightY > MaxY || rightY < MinY))
                        continue;

                    while (leftY > MaxY || leftY < MinY)
                    {
                        deltaX = (maxX - minX)/10;
                        minX += deltaX;

                        if (deltaX < 0.1)
                            break;

                        leftY = ConstraintFunctionValue(constraint, minX);
                    }

                    minX -= deltaX;
                    deltaX = 0;

                    while (rightY > MaxY || rightY < MinY)
                    {
                        deltaX = (maxX - minX)/10;
                        maxX -= deltaX;

                        if (deltaX < 0.1)
                            break;

                        rightY = ConstraintFunctionValue(constraint, maxX);
                    }

                    maxX += deltaX;

                    var constraintLine =
                        new FunctionSeries(
                            x => (-constraint[XIndex] * x + constraint.AbsoluteTerm) / constraint[YIndex],
                            minX, maxX, 2) { Color = Color};

                    yield return constraintLine;
                }
                else if (Math.Abs(constraint[XIndex]) > double.Epsilon)
                {
                    var lineSeriers = new LineSeries() {Color = Color};

                    lineSeriers.Points.AddRange(new []
                    {
                        new DataPoint(constraint.AbsoluteTerm, MinY),
                        new DataPoint(constraint.AbsoluteTerm, MaxY)
                    });

                    yield return lineSeriers;
                }
            }
        }

        private double ConstraintFunctionValue(LinearConstraint constraint, double x) =>
            (-constraint[XIndex]*x + constraint.AbsoluteTerm)/constraint[YIndex];

        private OxyColor GetRandomColor()
        {
            var randomGen = new Random(Solution.Cluster.GetHashCode());

            return OxyColor.FromRgb(randomGen.NextBytes(1).First(),
                randomGen.NextBytes(1).First(),
                randomGen.NextBytes(1).First());
        }
    }
}