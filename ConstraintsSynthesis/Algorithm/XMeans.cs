using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Statistics.Distributions.Univariate;
using ConstraintsSynthesis.Model;

namespace ConstraintsSynthesis.Algorithm
{
    public class XMeans
    {
        public readonly int MinK;
        public IList<Cluster> Clusters { get; } = new List<Cluster>();

        public XMeans(int minK = 2)
        {
            MinK = minK;
        }

        public void Fit(IList<Point> points)
        {
            var clusters = SplitPointsIntoClusters(points, MinK);

            RecursivelySplit(clusters);
        }

        private void RecursivelySplit(IEnumerable<Cluster> clusters)
        {
            foreach (var cluster in clusters)
            {
                if (cluster.Size <= 3)
                {
                    Clusters.Add(cluster);
                    continue;
                }

                var splitClusters = SplitPointsIntoClusters(cluster.Points, 2);
                var c1 = splitClusters[0];
                var c2 = splitClusters[1];

                var beta = c1.Centroid.Subtract(c2.Centroid).Euclidean()/
                           Math.Sqrt(c1.Covariance.PseudoDeterminant() +
                                     c2.Covariance.PseudoDeterminant());
                var alpha = 0.5/NormalDistribution.Standard.DistributionFunction(beta);
                var bic = -2*
                          (cluster.Size*Math.Log(alpha) + c1.LogLikelihood +
                           c2.LogLikelihood) + 2*cluster.K*Math.Log(cluster.Size);

                if (bic < cluster.BIC)
                    RecursivelySplit(splitClusters);
                else
                    Clusters.Add(cluster);
            }
        }

        private static IList<Cluster> SplitPointsIntoClusters(IList<Point> points, int clustersCount)
        {
            var observations = points.Select(p => p.Coordinates).ToArray();
            var kmeans = new KMeans(clustersCount);
            var initialClustersIndices = kmeans.Learn(observations).Decide(observations);
            var distinctIndices = initialClustersIndices.Distinct();
            var clusters =
                distinctIndices.Select(
                    clusterIndex =>
                        points.Where((p, index) => 
                            initialClustersIndices[index] == clusterIndex).ToList())
                    .Select((newClusterPoints, index) => 
                        new Cluster(newClusterPoints, kmeans, index)).ToList();

            return clusters;
        }
    }
}
