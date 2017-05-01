using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Statistics;
using Accord.Statistics.Distributions.Multivariate;

namespace ConstraintsSynthesis.Model
{
    public class Cluster
    {
        private readonly MultivariateNormalDistribution _multivariateNormalDistribution;
        public readonly int K;

        public int Size => Points.Count;
        public List<Point> Points { get; }
        public double[] Centroid { get; }
        public double[][] Covariance { get; }
        public double[] Minimums { get; }
        public double[] Maximums { get; }
        public int Dimensions { get; }

        public Cluster(List<Point> points, KMeans kmeans, int index)
        {
            Dimensions = points.First().Coordinates.Length;

            K = Dimensions * (Dimensions + 3) / 2;
            Points = points;
            Centroid = kmeans.Clusters[index].Centroid;
            Covariance = kmeans.Clusters[index].Covariance;
            Minimums = new double[Dimensions];
            Maximums = new double[Dimensions];

            var pointsMatrix = points.Select(p => p.Coordinates).ToArray();
            var means = pointsMatrix.Mean(pointsMatrix.Transpose().Select(x => x.Sum()).ToArray());
            var covariance = pointsMatrix.Covariance(means);

            for (var i = 0; i < Dimensions; i++)
            {
                Minimums[i] = points.Min(p => p[i]);
                Maximums[i] = points.Max(p => p[i]);
                _multivariateNormalDistribution = new MultivariateNormalDistribution(means, covariance);
            }
        }

        public double LogLikelihood =>
            Points.Sum(p =>
                _multivariateNormalDistribution.LogProbabilityDensityFunction(p.Coordinates));

        public double BIC =>
            -2*LogLikelihood + K*Math.Log(Size);
    }
}
