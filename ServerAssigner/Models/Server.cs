// CPU ve RAM kparsiteleri belirli bir grup sunu üzerinde 
// mikroservilerin en az maliyetle dağıtılmasını sağlamak için
// bir sunucu atama algoritması uygular. Burada temel amaç, sunucu sayısını ve maliyeti minimize etmektir.
// Algoritma, her mikroservisin CPU ve RAM gereksinimlerini dikkate alarak,
// mevcut sunucuların kapasitelerini aşmadan en uygun sunucuyu seçer.   
// Bu haliyle, optimization problemlerinden Bin Packing Problem'e benzer bir yaklaşım sergiler.

namespace ServerAssigner.Models
{

    public class Server
    {
        public string Name { get; set; }
        public int CpuCapacity { get; set; }
        public int RamCapacity { get; set; }
        public double Cost { get; set; }

        public Server(string name, int cpuCapacity, int ramCapacity, int cost)
        {
            Name = name;
            CpuCapacity = cpuCapacity;
            RamCapacity = ramCapacity;
            Cost = cost;
        }
    }


    
}