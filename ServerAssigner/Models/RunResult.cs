namespace ServerAssigner.Models
{
    public class RunResult
    {
        public int RunId { get; set; }
        public double BestFitness { get; set; }
        public long ElapsedMilliseconds { get; set; }
    }

}
