using BenchmarkDotNet.Running;
using Benchmarks.Counters;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<G_CounterBenchmarks>();
        }
    }
}
