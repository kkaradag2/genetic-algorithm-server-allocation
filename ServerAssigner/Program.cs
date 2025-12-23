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
        const int runSystemAtTimes = 5;

        static readonly Random rand = new Random();

        static void Main(string[] args)
        {

            for (int run = 1; run <= runSystemAtTimes; run++) 
            {

                Console.WriteLine($"\n========== Run {run} of the Genetic Algorithm ==========");
                Console.WriteLine();

                int noImprovementCount = 0;
                double lastBestFitness = double.MaxValue;

                // 1) Initial population
                var population = GenerateInitialPopulation(populationSize);

                Console.WriteLine("\nSTEP 1 - INITIAL POPULATION");
                PrintPopulation(population);

                // 2) GA MAIN LOOP
                for (int gen = 1; gen <= generations; gen++)
                {
                    Console.WriteLine($"\nSTEP 2 - PARENT SELECTION Gen:{gen}");

                    // Parent selection
                    var parentPairs = SelectParentPairsRoulette(
                        population,
                        lambda,
                        parentCount
                    );
                    PrintParentPairs(parentPairs);

                    Console.WriteLine($"\nSTEP 3 - CROSSOVER Gen:{gen}");
                    // Crossover
                    var offspring = UniformCrossoverWithLogging(parentPairs);

                    Console.WriteLine($"\nSTEP 4 - MUTATION Gen:{gen}");
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

                    Console.WriteLine("\nSTEP 5 - REPLACEMENT (NEW POPULATION) ");
                    PrintPopulation(population);


                    // Best of generation
                    var bestOfGen = population
                        .OrderBy(ind => Fitness(ind, lambda))
                        .First();

                    double bestFitness = Fitness(bestOfGen, lambda);

                    Console.WriteLine("\n-- Best of Generation --");
                    Console.WriteLine(
                        $"Best Gen {gen}: [{string.Join(", ", bestOfGen)}] Fitness = {bestFitness}"
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
        // ROULETTE WHEEL SELECTION (PAIR-BASED)
        // =======================
        // Amaç:
        // - Rulet tekeri yöntemi ile parent seçimi yapmak
        // - Aynı bireyin kendi kendisiyle eşleşmesini (self-mating) engellemek
        // - Her pair (P1, P2) için P1 ≠ P2 olacak şekilde eşleştirme yapmak
        static List<(int[] P1, int[] P2)> SelectParentPairsRoulette(
            List<int[]> population,
            double lambda,
            int parentCount)
        {
            // parentCount mutlaka çift ve en az 2 olmalı
            if (parentCount < 2 || parentCount % 2 != 0)
                throw new ArgumentException("parentCount must be even and >= 2");

            // 1) Popülasyondaki her birey için fitness hesapla
            double[] fitness = population
                .Select(ind => Fitness(ind, lambda))
                .ToArray();

            // 2) p(i) = F / f(i) olasılıklarını hesapla
            double F = fitness.Sum();
            double[] probabilities = fitness
                .Select(f => F / f)
                .ToArray();

            // 3) Kümülatif rulet tekeri oluştur
            double sumP = probabilities.Sum();
            double acc = 0.0;

            double[] cumulative = probabilities.Select(p =>
            {
                acc += p / sumP;
                return acc;
            }).ToArray();

            // 4) Parent seçimi (aynı birey üst üste seçilmesin)
            var selectedParents = new List<int[]>();

            while (selectedParents.Count < parentCount)
            {
                double u = rand.NextDouble(); // [0,1)

                for (int i = 0; i < cumulative.Length; i++)
                {
                    if (u <= cumulative[i])
                    {
                        // İş kuralı:
                        // Aynı birey kendisiyle eşleşmemeli
                        // (self-mating'i engelle)
                        if (selectedParents.Count == 0 ||
                            !ReferenceEquals(selectedParents.Last(), population[i]))
                        {
                            selectedParents.Add(population[i]);
                        }
                        break;
                    }
                }
            }

            // 5) Parentları pair'lere ayır
            var pairs = new List<(int[] P1, int[] P2)>();

            for (int i = 0; i < selectedParents.Count; i += 2)
            {
                // Güvenlik kontrolü:
                // Aynı bireyler yanlışlıkla yan yana geldiyse,
                // bu pair atlanır (pratikte çok nadir olur)
                if (ReferenceEquals(selectedParents[i], selectedParents[i + 1]))
                    continue;

                pairs.Add((selectedParents[i], selectedParents[i + 1]));
            }

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

                Console.WriteLine($"Mask      : [{string.Join(", ", mask)}]");
                Console.WriteLine($"Offspring1: [{string.Join(", ", c1)}]");
                Console.WriteLine($"Offspring2: [{string.Join(", ", c2)}]");

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

        static List<int[]> ElitistReplacement(List<int[]> current, List<int[]> offspring, double lambda, int eliteCount, int populationSize)
        {
            // 1) Elitleri seç
            var elites = current
                .OrderBy(ind => Fitness(ind, lambda))
                .Take(eliteCount)
                .Select(ind => (int[])ind.Clone())
                .ToList();

            // 2) Offspring’leri fitness’a göre sırala
            var sortedOffspring = offspring
                .OrderBy(ind => Fitness(ind, lambda))
                .Select(ind => (int[])ind.Clone())
                .ToList();

            var next = new List<int[]>();
            next.AddRange(elites);

            // 3) En iyi offspring’leri ekle
            foreach (var child in sortedOffspring)
            {
                if (next.Count >= populationSize)
                    break;

                next.Add(child);
            }

            // 4) Hâlâ eksik varsa, eski popülasyondan doldur
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
