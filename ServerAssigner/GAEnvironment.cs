using ServerAssigner.Models;

namespace ServerAssigner
{
    public static class GAEnvironment
    {
        // Sunucular (Problem sabiti)
        public static readonly Server[] Servers =
        {
            new Server("Server1", cpuCapacity: 16, ramCapacity: 64, cost: 100),
            new Server("Server2", cpuCapacity: 32, ramCapacity: 128, cost: 180),
            new Server("Server3", cpuCapacity: 8,  ramCapacity: 32, cost: 60),
            new Server("Server4", cpuCapacity: 2,  ramCapacity: 4,  cost: 10)
        };

        // Mikroservisler (Problem sabiti)
        public static readonly Microservice[] Microservices =
        {
            new Microservice("S1",  cpuRequirement: 4, ramRequirement: 16),
            new Microservice("S2",  cpuRequirement: 8, ramRequirement: 32),
            new Microservice("S3",  cpuRequirement: 2, ramRequirement: 8),
            new Microservice("S4",  cpuRequirement: 4, ramRequirement: 64),
            new Microservice("S5",  cpuRequirement: 8, ramRequirement: 8),
            new Microservice("S6",  cpuRequirement: 2, ramRequirement: 48),
            new Microservice("S7",  cpuRequirement: 2, ramRequirement: 2),
            new Microservice("S8",  cpuRequirement: 1, ramRequirement: 8),
            new Microservice("S9",  cpuRequirement: 1, ramRequirement: 16),
            new Microservice("S10", cpuRequirement: 4, ramRequirement: 48)
        };
    }
}
