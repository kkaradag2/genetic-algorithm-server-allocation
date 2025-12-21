
Where `F` is the sum of fitness values in the population.

- Parents are selected using **roulette wheel selection**
- Selected parents are grouped into **pairs** for crossover

---

### 3. Crossover (Uniform Crossover)
- For each parent pair:
  - A random binary mask is generated
  - Genes are exchanged based on the mask
- Each parent pair produces **two offspring**

---

### 4. Mutation
- Mutation is applied to the **first gene** of an offspring
- Mutation occurs with a fixed probability
- The gene is reassigned to a different server

---

### 5. Elitist Replacement
- The best individuals from the current population are preserved
- Offspring replace weaker individuals
- This ensures that the best solution is never lost

---

### 6. Termination Criteria
The algorithm terminates when:
- The maximum number of generations is reached, **or**
- No improvement is observed for a predefined number of generations (**early stopping**)

---

## Experimental Results

Due to the stochastic nature of Genetic Algorithms, the program was executed multiple times.

| Run | Best Initial Fitness | Best Final Fitness | Generations |
|----:|---------------------:|-------------------:|------------:|
| 1   | 65,350               | 23,350             | 20          |
| 2   | 88,500               | 22,350             | 20          |
| 3   | 55,290               | 42,340             | 13          |

These results demonstrate that:
- Initial population quality varies between runs
- The Genetic Algorithm consistently improves solution quality
- Early stopping effectively reduces unnecessary iterations

---

## Demo Application

The solution is implemented as a **C# .NET Console Application**.

### Requirements
- .NET 6.0 or later (modern .NET / .NET Core platform)


### How to Run

```bash
dotnet restore
dotnet run
