# Genetic Algorithm Based Server Assignment

This project presents a **Genetic Algorithm (GA)**–based solution for a **server assignment problem**, where a set of microservices must be allocated to a limited number of heterogeneous servers under **CPU and RAM capacity constraints**.

The main objective is to **minimize total server cost** while satisfying resource constraints. Constraint violations are handled using a **penalty-based fitness function**, making the problem suitable for metaheuristic optimization techniques.

---

## Problem Definition

Given:
- A set of **microservices**, each with specific CPU and RAM requirements
- A set of **servers**, each with limited CPU and RAM capacities and an associated cost

Objective:
- Assign each microservice to exactly one server
- Minimize the total cost of the servers used
- Penalize any violations of CPU or RAM capacity constraints

This problem is closely related to the **Bin Packing Problem** and **resource-constrained assignment problems**, which are known to be **NP-hard**.

---

## Solution Approach

A **Genetic Algorithm** is employed to efficiently explore the solution space and approximate near-optimal solutions.

### Chromosome Representation

Each solution (individual) is represented as an integer array:

a = [a₁, a₂, ..., aₙ]


Here, each gene `aᵢ` denotes the **index of the server** to which microservice `i` is assigned.

---

## Fitness Function

The fitness function is defined as:

f(a) = TotalServerCost + λ · (O_CPU(a) + O_RAM(a))


Where:
- `TotalServerCost`: Sum of the costs of all servers utilized in the solution
- `O_CPU(a)`: Total CPU capacity violation
- `O_RAM(a)`: Total RAM capacity violation
- `λ`: Penalty coefficient controlling the severity of constraint violations

This formulation allows infeasible solutions during the search while strongly penalizing them, guiding the algorithm toward feasible and cost-effective solutions.

---

## Genetic Algorithm Components

### 1. Initial Population
- A population of random candidate solutions is generated.
- Each microservice is initially assigned to a randomly selected server.

---

### 2. Parent Selection (Roulette Wheel Selection)

Selection probabilities are computed as:

p(i) = F / f(i)


where `F` is the sum of fitness values in the population.

- Parents are selected using **roulette wheel selection**
- Selected parents are grouped into **pairs** for crossover

---

### 3. Crossover (Uniform Crossover)
- For each parent pair:
  - A random binary mask is generated
  - Genes are exchanged according to the mask
- Each parent pair produces **two offspring**

---

### 4. Mutation
- Mutation is applied to the **first gene** of an offspring
- Mutation occurs with a predefined probability
- The selected gene is reassigned to a different server

---

### 5. Elitist Replacement
- The best-performing individuals (elites) are preserved
- Newly generated offspring replace weaker individuals
- This guarantees that the best solution is never lost across generations

---

### 6. Termination Criteria

The algorithm terminates when:
- The maximum number of generations is reached, **or**
- No improvement is observed for a predefined number of consecutive generations (**early stopping**)

---

## Experimental Results

Due to the stochastic nature of Genetic Algorithms, the program was executed multiple times.

| Run | Best Initial Fitness | Best Final Fitness | Generations |
|----:|---------------------:|-------------------:|------------:|
| 1   | 65,350               | 23,350             | 20          |
| 2   | 88,500               | 22,350             | 20          |
| 3   | 55,290               | 42,340             | 13          |

These results indicate that:
- The quality of the initial population varies between runs
- The Genetic Algorithm consistently improves solution quality
- Early stopping effectively reduces unnecessary iterations

---

## Demo Application

The solution is implemented as a **C# .NET Console Application**.

### Requirements
- .NET 6.0 or later (modern, Core-based .NET platform)

### How to Run

```bash
dotnet restore
dotnet run
