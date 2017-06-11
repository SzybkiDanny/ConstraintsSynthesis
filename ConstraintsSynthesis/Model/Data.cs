using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Numerics.Random;
using MethodTimer;

namespace ConstraintsSynthesis.Model
{
    public class Data
    {
        private readonly MersenneTwister _random = new MersenneTwister(Program.Seed);

        public List<Point> Points { get; } = new List<Point>();
        public int Dimensions { get; private set; }

        [Time("Loading input data")]
        public void Load(string filename, string delimiter = "")
        {
            var data = new List<Point>();

            using (var file = new StreamReader(filename))
            {
                while (!file.EndOfStream)
                {
                    var values =
                        file.ReadLine()?.Split(new[] {delimiter}, StringSplitOptions.None).Select(
                            double.Parse);
                    var point = new Point(values.Take(values.Count() - 1).ToArray())
                    {
                        Label = Math.Abs(values.Last() - 1.0) < double.Epsilon
                    };

                    data.Add(point);
                }
            }

            Points.AddRange(data);
            Dimensions = data.First().Coordinates.Length;
        }


        public IEnumerable<Point> FindMarginalPoints()
        {
            var consideredPoints = new List<Point>(Points);

            for (var i = 0; i < Points.Count; i++)
            {
                var randomPoint = GetRandomPoint();
                var maxDistance = 0.0;
                Point marginalPoint = null;

                foreach (var point in consideredPoints.Where(p => p.Label))
                {
                    var distance = CalculateDistanceBetweenPoints(randomPoint, point);
                    if (distance <= maxDistance)
                        continue;

                    maxDistance = distance;
                    marginalPoint = point;
                }
 
                consideredPoints.Remove(marginalPoint);

                yield return marginalPoint;
            }
        }

        private Point GetRandomPoint() => 
            Points[_random.Next(Points.Count)];

        private double CalculateDistanceBetweenPoints(Point p1, Point p2) =>
            Math.Sqrt(p1.Coordinates.Zip(p2.Coordinates, (d1, d2) => Math.Pow(d1 - d2, 2)).Sum());
    }
}