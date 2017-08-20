using System.Collections.Generic;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Random;

namespace ConstraintsSynthesis.Benchmarks
{
    public abstract class BenchmarkGenerator
    {
        protected MersenneTwister RandomSource = new MersenneTwister(Program.Seed);

        public abstract IList<Point> Generate(int dimensions, double d, int positives, int negatives = 0);
    }
}
