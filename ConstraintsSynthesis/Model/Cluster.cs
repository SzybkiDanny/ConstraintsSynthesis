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
        public double[][] Covarianve { get; }

        public Cluster(List<Point> points, KMeans kmeans, int index)
        {
            Points = points;
            Centroid = kmeans.Clusters[index].Centroid;
            Covarianve = kmeans.Clusters[index].Covariance;

            var d = points.First().Coordinates.Length;
            K = d*(d + 3)/2;

            var pointsMatrix = points.Select(p => p.Coordinates).ToArray();
            var means = pointsMatrix.Mean(pointsMatrix.Transpose().Select(x => x.Sum()).ToArray());
            var covariance = pointsMatrix.Covariance(means);

            _multivariateNormalDistribution = new MultivariateNormalDistribution(means, covariance);
        }

        public double LogLikelihood =>
            Points.Sum(p =>
                _multivariateNormalDistribution.LogProbabilityDensityFunction(p.Coordinates));

        public double BIC =>
            -2*LogLikelihood + K*Math.Log(Size);

    }
}
