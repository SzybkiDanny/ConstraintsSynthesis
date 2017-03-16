using System;
using System.Linq;

namespace ConstraintsSynthesis.Model
{
    public class Point
    {
        public bool Label { get; set; } = true;
        public bool IsMarginal { get; set; }
        public double[] Coordinates { get; }

        public double this[int index]
        {
            get { return Coordinates[index]; }
            set { Coordinates[index] = value; }
        }

        public Point(int dimensions)
        {
            Coordinates = new double[dimensions];
        }

        public Point(double[] coordinates)
        {
            Coordinates = coordinates;
        }

        public static bool operator <(Point p1, Point p2)
        {
            if (p1.Coordinates.Length != p2.Coordinates.Length)
                throw new ArgumentException("Both points need to have the same dimensions count");

            for (int i = 0; i < p1.Coordinates.Length; i++)
                if (p1[i] >= p2[i])
                    return false;

            return true;
        }

        public static bool operator >(Point p1, Point p2)
        {
            if (p1.Coordinates.Length != p2.Coordinates.Length)
                throw new ArgumentException("Both points need to have the same dimensions count");

            for (int i = 0; i < p1.Coordinates.Length; i++)
                if (p1[i] <= p2[i])
                    return false;

            return true;
        }

        public override string ToString()
        {
            return $"{string.Join(", ", Coordinates)} {Label}";
        }
    }
}