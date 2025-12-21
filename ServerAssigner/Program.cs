namespace ServerAssigner
{
    public class Program
    {
        // =======================
        // GA PARAMETERS
        // =======================
        const int populationSize = 15;
        const int generations = 50;
        const int eliteCount = 2;
        const int parentCount = 2;      // 2 -> 1 pair, 4 -> 2 pair
        const double mutationRate = 0.2;
        const double lambda = 1000.0;
        const int maxNoImprovement = 10;

        static readonly Random rand = new Random();

        static void Main(string[] args)
        {
            int noImprovementCount = 0;
            double lastBestFitness = double.MaxValue;

            // 1) Initial population
            var population = GenerateInitialPopulation(populationSize);

            Console.WriteLine("INITIAL POPULATION");
            PrintPopulation(population);

            // 2) GA MAIN LOOP
            for (int gen = 1; gen <= generations; gen++)
            {
                Console.WriteLine($"\n========== GENERATION {gen} ==========");

                // Parent selection
                var parentPairs = SelectParentPairsRoulette(
                    population,
                    lambda,
                    parentCount
                );
                PrintParentPairs(parentPairs);

                // Crossover
                var offspring = UniformCrossoverWithLogging(parentPairs);

                // Mutation
                MutateFirstGene(offspring);
                PrintOffspringAfterMutation(offspring);

                // Replacement
                population = ElitistReplacement(
                    population,
                    offspring,
                    lambda,
                    eliteCount,
                    populationSize
                );

                // Best of generation
                var bestOfGen = population
                    .OrderBy(ind => Fitness(ind, lambda))
                    .First();

                double bestFitness = Fitness(bestOfGen, lambda);

                Console.WriteLine("\n-- Best of Generation --");
                Console.WriteLine(
                    $"Best @ Gen {gen}: [{string.Join(", ", bestOfGen)}] Fitness = {bestFitness}"
                );

                // Early stopping
                if (bestFitness < lastBestFitness)
                {
                    lastBestFitness = bestFitness;
                    noImprovementCount = 0;
                }
                else
                {
                    noImprovementCount++;
                }

                if (noImprovementCount >= maxNoImprovement)
                {
                    Console.WriteLine(
                        $"Early stopping: no improvement for {maxNoImprovement} generations."
                    );
                    break;
                }
            }

            Console.WriteLine("\n=========== FINAL BEST SOLUTION ===========");
            PrintBestIndividual(population);
        }

        // =======================
        // INITIAL POPULATION
        // =======================
        static List<int[]> GenerateInitialPopulation(int populationSize)
        {
            var population = new List<int[]>();

            for (int i = 0; i < populationSize; i++)
            {
                int[] chromosome = new int[GAEnvironment.Microservices.Length];

                for (int j = 0; j < chromosome.Length; j++)
                    chromosome[j] = rand.Next(GAEnvironment.Servers.Length);

                population.Add(chromosome);
            }

            return population;
        }

        // =======================
        // FITNESS & PENALTIES
        // =======================
        static double ComputeOCPU(int[] a)
        {
            double oCpu = 0.0;

            for (int j = 0; j < GAEnvironment.Servers.Length; j++)
            {
                double usedCpu = 0.0;

                for (int i = 0; i < a.Length; i++)
                    if (a[i] == j)
                        usedCpu += GAEnvironment.Microservices[i].CpuRequirement;

                oCpu += Math.Max(0, usedCpu - GAEnvironment.Servers[j].CpuCapacity);
            }

            return oCpu;
        }

        static double ComputeORAM(int[] a)
        {
            double oRam = 0.0;

            for (int j = 0; j < GAEnvironment.Servers.Length; j++)
            {
                double usedRam = 0.0;

                for (int i = 0; i < a.Length; i++)
                    if (a[i] == j)
                        usedRam += GAEnvironment.Microservices[i].RamRequirement;

                oRam += Math.Max(0, usedRam - GAEnvironment.Servers[j].RamCapacity);
            }

            return oRam;
        }

        static double Fitness(int[] a, double lambda)
        {
            bool[] used = new bool[GAEnvironment.Servers.Length];
            double cost = 0.0;

            for (int i = 0; i < a.Length; i++)
                used[a[i]] = true;

            for (int j = 0; j < used.Length; j++)
                if (used[j])
                    cost += GAEnvironment.Servers[j].Cost;

            return cost + lambda * (ComputeOCPU(a) + ComputeORAM(a));
        }

        // =======================
        // ROULETTE WHEEL SELECTION
        // =======================
        static List<(int[] P1, int[] P2)> SelectParentPairsRoulette(
            List<int[]> population,
            double lambda,
            int parentCount)
        {
            if (parentCount < 2 || parentCount % 2 != 0)
                throw new ArgumentException("parentCount must be even and >= 2");

            double[] fitness = population.Select(p => Fitness(p, lambda)).ToArray();
            double F = fitness.Sum();
            double[] p = fitness.Select(f => F / f).ToArray();

            double sumP = p.Sum();
            double acc = 0;
            double[] cumulative = p.Select(pi =>
            {
                acc += pi / sumP;
                return acc;
            }).ToArray();

            var selected = new List<int[]>();

            for (int k = 0; k < parentCount; k++)
            {
                double u = rand.NextDouble();
                for (int i = 0; i < cumulative.Length; i++)
                    if (u <= cumulative[i])
                    {
                        selected.Add(population[i]);
                        break;
                    }
            }

            var pairs = new List<(int[], int[])>();
            for (int i = 0; i < selected.Count; i += 2)
                pairs.Add((selected[i], selected[i + 1]));

            return pairs;
        }

        // =======================
        // UNIFORM CROSSOVER (LOG)
        // =======================
        static List<int[]> UniformCrossoverWithLogging(
            List<(int[] P1, int[] P2)> parentPairs)
        {
            var offspring = new List<int[]>();

            Console.WriteLine("\n-- Uniform Crossover --");

            foreach (var (P1, P2) in parentPairs)
            {
                int len = P1.Length;
                int[] mask = new int[len];
                int[] c1 = new int[len];
                int[] c2 = new int[len];

                for (int i = 0; i < len; i++)
                    mask[i] = rand.Next(2);

                for (int i = 0; i < len; i++)
                {
                    if (mask[i] == 1)
                    {
                        c1[i] = P1[i];
                        c2[i] = P2[i];
                    }
                    else
                    {
                        c1[i] = P2[i];
                        c2[i] = P1[i];
                    }
                }

                Console.WriteLine($"Mask  : [{string.Join(", ", mask)}]");
                Console.WriteLine($"Child1: [{string.Join(", ", c1)}]");
                Console.WriteLine($"Child2: [{string.Join(", ", c2)}]");

                offspring.Add(c1);
                offspring.Add(c2);
            }

            return offspring;
        }

        // =======================
        // MUTATION
        // =======================
        static void MutateFirstGene(List<int[]> offspring)
        {
            foreach (var child in offspring)
            {
                if (rand.NextDouble() < mutationRate)
                {
                    int oldVal = child[0];
                    int newVal;
                    do
                    {
                        newVal = rand.Next(GAEnvironment.Servers.Length);
                    }
                    while (newVal == oldVal);

                    child[0] = newVal;
                }
            }
        }

        // =======================
        // ELITISM + REPLACEMENT
        // =======================
        static List<int[]> ElitistReplacement(
            List<int[]> current,
            List<int[]> offspring,
            double lambda,
            int eliteCount,
            int populationSize)
        {
            var elites = current
                .OrderBy(ind => Fitness(ind, lambda))
                .Take(eliteCount)
                .Select(ind => (int[])ind.Clone())
                .ToList();

            var next = new List<int[]>();
            next.AddRange(elites);

            foreach (var c in offspring)
            {
                if (next.Count >= populationSize)
                    break;
                next.Add(c);
            }

            if (next.Count < populationSize)
            {
                var remaining = current
                    .Except(elites)
                    .OrderBy(ind => Fitness(ind, lambda))
                    .Take(populationSize - next.Count)
                    .Select(ind => (int[])ind.Clone());

                next.AddRange(remaining);
            }

            return next;
        }

        // =======================
        // PRINT HELPERS
        // =======================
        static void PrintPopulation(List<int[]> population)
        {
            foreach (var ind in population)
                Console.WriteLine(
                    $"[{string.Join(", ", ind)}] -> Fitness = {Fitness(ind, lambda)}"
                );
        }

        static void PrintParentPairs(List<(int[] P1, int[] P2)> pairs)
        {
            Console.WriteLine("\n-- Selected Parent Pairs --");

            int i = 1;
            foreach (var p in pairs)
            {
                Console.WriteLine($"Pair {i++}:");
                Console.WriteLine($"  P1 = [{string.Join(", ", p.P1)}]");
                Console.WriteLine($"  P2 = [{string.Join(", ", p.P2)}]");
            }
        }

        static void PrintOffspringAfterMutation(List<int[]> offspring)
        {
            Console.WriteLine("\n-- After Mutation --");
            int i = 1;
            foreach (var c in offspring)
                Console.WriteLine($"Child {i++}: [{string.Join(", ", c)}]");
        }

        static void PrintBestIndividual(List<int[]> population)
        {
            var best = population.OrderBy(ind => Fitness(ind, lambda)).First();
            Console.WriteLine($"Best Solution: [{string.Join(", ", best)}]");
            Console.WriteLine($"Best Fitness : {Fitness(best, lambda)}");
        }
    }
}
