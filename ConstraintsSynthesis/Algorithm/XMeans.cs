using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Filters;
using ConstraintsSynthesis.Model;
using MethodTimer;

namespace ConstraintsSynthesis.Algorithm
{
    public class XMeans
    {
        private static readonly Normalization Normalization = new Normalization();
        public int MinK { get; }
        public bool Normalize { get; }
        public bool EnforceSingleCluster { get; }
        public IList<Cluster> Clusters { get; } = new List<Cluster>();

        public XMeans(int minK = 1, bool normalize = true, bool enforceSingleCluster = false)
        {   
            MinK = minK;
            Normalize = normalize;
            EnforceSingleCluster = enforceSingleCluster;
        }

        [Time("Clustering points")]
        public void Fit(IList<Point> points)
        {
            var clusters = SplitPointsIntoClusters(points, EnforceSingleCluster ? 1 : MinK, Normalize);

            if (!EnforceSingleCluster)
                RecursivelySplit(clusters);
            else
                Clusters.Add(clusters.First());
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

                var splitClusters = SplitPointsIntoClusters(cluster.Points, 2, Normalize);
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

        private static IList<Cluster> SplitPointsIntoClusters(IList<Point> points, int clustersCount, bool normalize)
        {
            var observations = points.Select(p => p.Coordinates).ToArray();
            var kmeans = new KMeans(clustersCount) {ParallelOptions = {MaxDegreeOfParallelism = 1}};

            if (normalize)
            {
                Normalization.Detect(observations);
                observations = Normalization.Apply(observations);
            }

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
