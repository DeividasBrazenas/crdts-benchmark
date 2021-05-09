using BenchmarkDotNet.Running;
using Benchmarks.Registers;
using Benchmarks.Sets;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<G_CounterBenchmarks>();
            //BenchmarkRunner.Run<PN_CounterBenchmarks>();
            //BenchmarkRunner.Run<LWW_RegisterBenchmarks>();
            //BenchmarkRunner.Run<LWW_RegisterWithVCBenchmarks>();
            //BenchmarkRunner.Run<G_SetBenchmarks>();
            //BenchmarkRunner.Run<P_SetBenchmarks>();
            //BenchmarkRunner.Run<U_SetBenchmarks>();
            //BenchmarkRunner.Run<LWW_SetBenchmarks>();
            //BenchmarkRunner.Run<LWW_SetWithVCBenchmarks>();
            //BenchmarkRunner.Run<LWW_OptimizedSetBenchmarks>();
            //BenchmarkRunner.Run<LWW_OptimizedSetWithVCBenchmarks>();
            //BenchmarkRunner.Run<OR_SetBenchmarks>();
            //BenchmarkRunner.Run<OR_OptimizedSetBenchmarks>();
            //BenchmarkRunner.Run<OUR_SetBenchmarks>();
            //BenchmarkRunner.Run<OUR_SetWithVCBenchmarks>();
            BenchmarkRunner.Run<OUR_OptimizedSetBenchmarks>();
          //  BenchmarkRunner.Run<OUR_OptimizedSetWithVCBenchmarks>();
        }
    }
}
