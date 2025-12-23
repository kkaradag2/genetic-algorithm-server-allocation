
# Genetic Algorithm Based Server Assignment

This project presents a **Genetic Algorithm (GA)**–based solution for a **server assignment problem**, where a set of microservices must be allocated to a limited number of heterogeneous servers under **CPU and RAM capacity constraints**.

The main objective is to **minimize total server cost** while satisfying resource constraints. Constraint violations are handled using a **penalty-based fitness function**, making the problem suitable for metaheuristic optimization techniques.


## Problem Definition

This is a sample problem definition that close the real-world senario but not same.

A bank plans to deploy its microservice-based core banking system on a cloud infrastructure.
The system consists of multiple independent microservices such as authentication, account management, money transfer, and notification services.
Each microservice consumes a fractional amount of CPU and RAM depending on its workload and must be deployed on exactly one cloud server. 
Each cloud server has fixed CPU and RAM capacities as well as a fixed operational cost. The total resource consumption of microservices assigned to a server must not exceed its capacity.
The objective is to minimize the total operational cost of the cloud infrastructure while ensuring that all microservices are deployed without resource overload.

This is a 2-dimensional Bin Packing Problem (CPU + RAM).
The problem is NP-hard and is solved using a Genetic Algorithm



Given:
- A set of **microservices**, each with specific CPU and RAM requirements

| Service Instance | Service Name                  | CPU | RAM |
|------------------|-------------------------------|-----|-----|
| S1               | AuthenticationService         | 4   | 16  |
| S2               | AccountManagementService      | 8   | 32  |
| S3               | TransactionService            | 2   | 8   |
| S4               | PaymentProcessingService      | 4   | 64  |
| S5               | CustomerProfileService        | 8   | 8   |
| S6               | NotificationService           | 2   | 48  |
| S7               | FraudDetectionService         | 2   | 2   |
| S8               | ReportingService              | 1   | 8   |
| S9               | AuditLoggingService           | 1   | 16  |
| S10              | ConfigurationService          | 4   | 48  |


- A set of **servers**, each with limited CPU and RAM capacities and an associated cost

| Server   | CPU | RAM | Cost / Month |
|----------|-----|-----|--------------|
| Server 1 | 16  | 64  | 100          |
| Server 2 | 32  | 128 | 180          |
| Server 3 | 8   | 32  | 60           |
| Server 4 | 2   | 4   | 10           |



Objective:
- Assign each microservice to exactly one server
- Minimize the total cost of the servers used
- Penalize any violations of CPU or RAM capacity constraints

This problem is closely related to the **Bin Packing Problem** and **resource-constrained assignment problems**, which are known to be **NP-hard**.


## Solution Approach

In this problem, a Genetic Algorithm (GA) is used to search for near-optimal solutions.
According to the GA workflow, an initial population is first generated randomly.
Then, parent selection is performed using the Roulette Wheel selection method.
Uniform crossover is applied to generate offspring, followed by a swap operation as the mutation mechanism.
Finally, elitist replacement is used to form the next generation of the population.

This procedure is stochastic in nature; therefore, the algorithm is executed five independent times, and statistical measures of the best solutions are reported.

## Chromosome Representation

Each solution (individual) is represented as an integer array:

$$
\mathbf{a} = [a_1, a_2, \ldots, a_n]
$$

where:

$$
a_i \in \{1, 2, \ldots, K\}, \quad \forall i
$$

A sample chromosome is given as:

$$
\mathbf{a} = [1, 1, 2, 2, 1, 3, 3, 1, 2, 3]
$$

This chromosome represents a candidate solution in which each gene corresponds to a microservice, and its value indicates the server to which that microservice is assigned.


## Fitness Function

The fitness function is designed to minimize the total operational cost while penalizing any capacity violations.

$$
f(a) = \sum_{j=1}^{K} (cost_j \cdot y_j(a)) + \lambda (O_{CPU}(a) + O_{RAM}(a))
$$

where the CPU capacity violation is defined as:

$$
O_{\text{CPU}}(a) =
\sum_{j=1}^{K}
\max \left(
0,
\sum_{i \in S_j(a)} \text{CPU}_i - W_{\text{CPU},j}
\right)
$$

and the RAM capacity violation is defined as:

$$
O_{\text{RAM}}(a) =
\sum_{j=1}^{K}
\max \left(
0,
\sum_{i \in S_j(a)} \text{RAM}_i - W_{\text{RAM},j}
\right)
$$



## Genetic Algorithm Components

### 1. Initial Population
- A population of random candidate solutions is generated.
- Each microservice is initially assigned to a randomly selected server.
- Population size is defined as 'populationSize'  limited value.


### 2. Parent Selection (Roulette Wheel Selection)

Selection probabilities are computed as:

p(i) = F / f(i)


where `F` is the sum of fitness values in the population.

- Parents are selected using **roulette wheel selection**
- Selected parents are grouped into **pairs** for crossover


### 3. Crossover (Uniform Crossover)
- For each parent pair:
  - A random binary mask is generated
  - Genes are exchanged according to the mask
- Each parent pair produces **two offspring**



### 4. Mutation

Mutation is performed using a swap operation. With a predefined mutation probability, two gene positions are selected randomly, and their values are exchanged. This operator introduces diversity into the population while maintaining the structural validity of the chromosome.




### 5. Elitist Replacement
- The best-performing individuals (elites) are preserved
- Newly generated offspring replace weaker individuals
- This guarantees that the best solution is never lost across generations


### 6. Termination Criteria

The algorithm terminates when:
- The maximum number of generations is reached, **or**
- No improvement is observed for a predefined number of consecutive generations (**early stopping**)


## Experimental Results

Due to the stochastic nature of Genetic Algorithms, the algorithm was executed multiple times under identical parameter settings.
The performance of the algorithm was evaluated based on the best fitness values and execution time obtained in each run.

### Experimental Results Summary

| Metric                 | Value             |
| ---------------------- | ----------------- |
| Mean Best Fitness      | 36946             |
| Best Fitness Overall   | 30350             |
| Std. Deviation Fitness | 6971.716001100446 |
| Mean Time (ms)         | 26                |
| Best Time (ms)         | 13                |


## Demo Application

The solution is implemented as a **C# .NET Console Application**.

### Requirements
- .NET 6.0 or later (modern, Core-based .NET platform)

### How to Run

```bash
cd ServerAssigner
dotnet restore
dotnet run
