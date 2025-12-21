// CPU ve RAM kparsiteleri belirli bir grup sunu üzerinde 
// mikroservilerin en az maliyetle dağıtılmasını sağlamak için
// bir sunucu atama algoritması uygular. Burada temel amaç, sunucu sayısını ve maliyeti minimize etmektir.
// Algoritma, her mikroservisin CPU ve RAM gereksinimlerini dikkate alarak,
// mevcut sunucuların kapasitelerini aşmadan en uygun sunucuyu seçer.   
// Bu haliyle, optimization problemlerinden Bin Packing Problem'e benzer bir yaklaşım sergiler.

namespace ServerAssigner.Models
{
    public class Microservice
    {
        public string Name { get; set; }
        public int CpuRequirement { get; set; }
        public int RamRequirement { get; set; }

        public Microservice(string v, int cpuRequirement, int ramRequirement)
        {
            this.Name = v;
            this.CpuRequirement = cpuRequirement;
            this.RamRequirement = ramRequirement;
        }
    }

   
}