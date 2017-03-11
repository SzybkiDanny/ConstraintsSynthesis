namespace TestDataGenerator.Model
{
    internal class Point
    {
        public bool Label { get; set; } = true;
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
    }
}