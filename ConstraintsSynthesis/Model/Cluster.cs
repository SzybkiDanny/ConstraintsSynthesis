using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Statistics;
using Accord.Statistics.Distributions.Multivariate;
using Accord.Statistics.Distributions.Univariate;

namespace ConstraintsSynthesis.Model
{
    public class Cluster
    {
        private readonly MultivariateNormalDistribution _multivariateNormalDistribution;
        public readonly int K;
        private Cluster _centralizedCluster;

        public int Size => Points.Count;
        public List<Point> Points { get; }
        public double[] Centroid { get; }
        public double[][] Covariance { get; }
        public double[] Minimums { get; }
        public double[] Maximums { get; }
        public int Dimensions { get; }
        public double[] Means { get; }

        public Cluster(List<Point> points, KMeans kmeans = null, int index = 0)
        {
            Dimensions = points.First().Coordinates.Length;

            K = Dimensions * (Dimensions + 3) / 2;
            Points = points;
            Centroid = kmeans?.Clusters[index].Centroid;
            Covariance = kmeans?.Clusters[index].Covariance;
            Minimums = new double[Dimensions];
            Maximums = new double[Dimensions];

            var pointsMatrix = points.Select(p => p.Coordinates).ToArray();

            Means = pointsMatrix.Mean(pointsMatrix.Transpose().Select(x => x.Sum()).ToArray());
            var covariance = pointsMatrix.Covariance(Means);

            for (var i = 0; i < Dimensions; i++)
            {
                Minimums[i] = points.Min(p => p[i]);
                Maximums[i] = points.Max(p => p[i]);
            }

            _multivariateNormalDistribution = new MultivariateNormalDistribution(Means, covariance);
        }


        public Cluster GetCentralizedCluster()
        {
            if (_centralizedCluster != null)
                return _centralizedCluster;

            var centralizedPoints =
                Points.Select(p => new Point(p.Coordinates.Subtract(Means)) {Label = p.Label}).ToList();

            return _centralizedCluster = new Cluster(centralizedPoints);
        }

        public double LogLikelihood =>
            Points.Sum(p =>
                _multivariateNormalDistribution.LogProbabilityDensityFunction(p.Coordinates));

        public double BIC =>
            -2*LogLikelihood + K*Math.Log(Size);

        public IEnumerable<Point> GenerateNegativePoints(int count = 100)
        {
            for (var i = 0; i < count; i++)
            {
                double[] point;

                do
                {
                    point = _multivariateNormalDistribution.Generate();
                } while (_multivariateNormalDistribution.Mahalanobis(point) < CalculateThreshold());

                yield return new Point(point) {Label = false};
            }
        }

        private double CalculateThreshold(double probability = 0.999) =>
            Math.Sqrt(ChiSquareDistribution.Inverse(probability, Dimensions));
    }
}
