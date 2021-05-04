using BenchmarkDotNet.Running;
using Benchmarks.Registers;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<G_CounterBenchmarks>();
            //BenchmarkRunner.Run<PN_CounterBenchmarks>();
            BenchmarkRunner.Run<LWW_RegisterBenchmarks>();
            BenchmarkRunner.Run<LWW_RegisterWithVCBenchmarks>();
        }
    }
}
