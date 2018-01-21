using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Random;
using MethodTimer;

namespace ConstraintsSynthesis.Algorithm
{
    // klasa służy do "przetwarzania" pojedynczego ograniczenia
    public class ConstraintLocalOptimization
    {
        private const int ProbingCoefficient = 10000;
        private static readonly MersenneTwister Random = new MersenneTwister(Program.Seed);
        private List<Point> PositivePoints { get; }
        private List<Point> NotSatisfiedPoints =>
            PositivePoints.Where(p => !Constraint.IsSatisfying(p)).ToList();

        private Dictionary<Term, ChangeDirection> _coefficientsOptimizationDirection = new Dictionary<Term, ChangeDirection>();
        private Dictionary<Term, double> _coefficientsStepFactor = new Dictionary<Term, double>();

        public Constraint Constraint { get; }
        public List<Point> Points { get; }

        public int SatisfiedPointsCount =>
            PositivePoints.Count(Constraint.IsSatisfying);

        // parametry konstruktora: przetwarzane ograniczenie i zbiór uczący
        public ConstraintLocalOptimization(Constraint constraint, Cluster cluster)
        {
            Constraint = constraint;
            Points = cluster.Points;
            PositivePoints = Points.Where(p => p.Label).ToList();
            ProbeCoefficientChangeDirection();
        }

        // sprawdzenie czy ograniczenie jest spełnione przez co najmniej połowę przykładów zbioru uczącego, jeżeli nie, to zmieniany jest jego znak
        // metoda OptimizeSign jest wywoływana przed metodą OptimizeCoefficients
        [Time("Optimizing sign")]
        public ConstraintLocalOptimization OptimizeSign()
        {
            if (SatisfiedPointsCount * 2 < PositivePoints.Count)
                Constraint.InvertInequalitySign();

            return this;
        }

        // po zakończeniu wykonywania tej metody ograniczenie jest spełnione przez wszystkie przykłady ze zbioru uczącego
        [Time("Optimizing coefficients")]
        public ConstraintLocalOptimization OptimizeCoefficients()
        {
            var terms = Constraint.Terms.Keys.ToList();
            var notSatisfied = NotSatisfiedPoints.Count;

            while (notSatisfied > 0) // dopóki istnieją przykłady, które są niespełnione
            {
                var coefficientAvg = Constraint.Terms.Values.Average(c => Math.Abs(c)); // średnia wartość bezwzględna wag termów
                var step = CalculateCoefficientOptimizationStep(); // wstępna wartość przesunięcia

                foreach (var term in terms)
                {
                    // obliczenie nowej wartości dla wszystkich wag termów w metodzie CalculateNewCoefficientValue
                    // stepSign (kierunek): przechowywany dla każdego termu w słowniku _coefficientsOptimizationDirection - wartość początkowa dla każdego termu (użyta w pierwszej iteracji) jest obliczona w metodzie ProbeCoefficientChangeDirection wywołanej w konstruktorze
                    // step: zależny od współczynnika przechowywanego dla każdego termu w słowniku _coefficientsStepFactor, wartości absolutnej dotychczasowej wagi termu (większe przesunięcie dla większych wartości wag), średniej wartości bezwzględnej wag termów (aby wartości wag termów nie rosły zbyt szybko)
                    // oldValue: dotychczasowa wartość wagi termu
                    Constraint[term] = CalculateNewCoefficientValue((int)_coefficientsOptimizationDirection[term],
                        _coefficientsStepFactor[term] * Math.Abs(Constraint[term]) / coefficientAvg, Constraint[term]);
                }

                // po obliczeniu nowych wartości wag termów obliczana jest nowa wartość wyrazu wolnego
                // nowa wartość wyrazu wolnego zależy od wstępnej wartości przesunięcia i średniej wartości bezwzględnej wag termów (aby zmiany (przesunięcia ograniczenia) powodowane przez zmienienie wartości wag termów były proporcjonalne do zmian powodowanych przez zmienienie wyrazu wolnego)
                // znak zmiany wyrazu wolnego zależy od znaku ograniczenia - wyraz wolny zmieniany jest tak, aby zwiększyć liczbę spełnionych przykładów
                Constraint.AbsoluteTerm += step * coefficientAvg * (Constraint.Sign == Inequality.GreaterThanOrEqual ? -1 : 1);

                // obliczanie nowych wartości dla _coefficientsStepFactor (współczynniki zmiany poszczególnych termów) i _coefficientsOptimizationDirection (kierunki zmiany poszczególnych termów)
                ProbeCoefficientChangeDirection(step);
                notSatisfied = NotSatisfiedPoints.Count;

                var notSatisfiedAfterChange = NotSatisfiedPoints.Count;

                if (notSatisfiedAfterChange == 0)
                    return this;

                if (notSatisfiedAfterChange < notSatisfied)
                    notSatisfied = notSatisfiedAfterChange;
            }

            return this;
        }


        // sprawdzenie jak dana zmiana wagi termu wpłynie na liczbę spełnionych przykładów zbioru uczącego
        private int TestCoefficientChange(Term term, double step, int stepSign)
        {
            var oldValue = Constraint[term];
            Constraint[term] = CalculateNewCoefficientValue(stepSign, step, Constraint[term]);
            var result = NotSatisfiedPoints.Count;
            Constraint[term] = oldValue;
            return result;
        }

        // obliczanie wartośći przesunięcia wagi termu
        // im bardziej ograniczenie jest zbliżone do "krawędzi" zbioru uczącego, tym wolniej jest przesuwane
        private double CalculateCoefficientOptimizationStep() =>
            1 + (double)NotSatisfiedPoints.Count / PositivePoints.Count;

        // obliczanie nowej wartości wagi
        // parameters:
        // stepSign: kierunek przesunięcia (dodatni/ujemny)
        // step: wielkość przesunięcia
        // oldValue: stara wartość wagi
        private double CalculateNewCoefficientValue(int stepSign, double step, double oldValue) =>
            oldValue + stepSign * step;

        // metoda wywoływana po każdej iteracji pętli w metodzie OptimizeCoefficients w celu obliczenia nowych wartości współczynników zmiany poszczególnych termów i kierunków zmiany poszczególnych termów
        // parameters:
        // step: testowa wielkość zmiany termu wykorzystywana do sprawdzenia wpływu zmiany wagi termu na spełnialność ograniczenia przez zbiór uczący
        private void ProbeCoefficientChangeDirection(double step = ProbingCoefficient)
        {
            var notSatisfiedCount = NotSatisfiedPoints.Count;

            foreach (var term in Constraint.Terms.Keys.ToList()) // dla każdego termu
            {
                // przypisywanie domyślnego kierunku zmiany wartości wagi termu na zmniejszanie
                if (!_coefficientsOptimizationDirection.ContainsKey(term))
                    _coefficientsOptimizationDirection[term] = ChangeDirection.Decreasing;

                // przypisywanie domyślnego współczynnika zmiany wartości wagi termu na neutralny (1.0)
                if (!_coefficientsStepFactor.ContainsKey(term))
                    _coefficientsStepFactor[term] = 1.0;

                // sprawdzenie jak zmiana wagi termu w tym samym kierunku, co dotychczas wpływa na spełnialność zbioru uczącego
                var noCoefficientChange = TestCoefficientChange(term, step, (int)_coefficientsOptimizationDirection[term]);

                if (noCoefficientChange < notSatisfiedCount) // jeżeli zmiana w tym samym kierunku poprawia spełnialność
                {
                    // przypisz nowy współczynnik zmiany wagi termu (jeżeli testowana zmiana wagi termu powoduje dużą zmianę w spełnialności, to zmniejsz współczynnik, w przypadku małej zmiany zwiększ współczynnik)
                    _coefficientsStepFactor[term] = 1 + noCoefficientChange / notSatisfiedCount;
                    continue; // sprawdź następny term 
                }

                // jeżeli dotychczasowy kierunek zmiany wartości wagi termu nie poprawia spełnialności, to zmień kierunek na przeciwny
                _coefficientsOptimizationDirection[term] = (ChangeDirection)((int)_coefficientsOptimizationDirection[term] * -1);

                // sprawdzenie jak zmiana wagi termu w nowym kierunku wpływa na spełnialność zbioru uczącego
                var coefficientChange = TestCoefficientChange(term, step, (int)_coefficientsOptimizationDirection[term]);

                if (coefficientChange < notSatisfiedCount) // jeżeli zmiana w nowym kierunku poprawia spełnialność
                {
                    // przypisz nowy współczynnik zmiany wagi termu (jeżeli testowana zmiana wagi termu powoduje dużą zmianę w spełnialności, to zmniejsz współczynnik, w przypadku małej zmiany zwiększ współczynnik)
                    _coefficientsStepFactor[term] = 1 + coefficientChange / notSatisfiedCount;
                    continue;
                }

                // jeżeli zmiana w żadnym kierunku nie poprawia spełnialności, to wybierz kierunek, który najmniej pogarsza spełnialność
                if (coefficientChange > noCoefficientChange)
                    _coefficientsOptimizationDirection[term] = (ChangeDirection)((int)_coefficientsOptimizationDirection[term] * -1);

                // ... i zmień współczynnik zmiany wagi termu na neutralny
                _coefficientsStepFactor[term] = 1;
            }
        }

        enum ChangeDirection
        {
            Decreasing = -1,
            Increasing = 1,
        }
    }
}
