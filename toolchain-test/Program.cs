using System;

namespace toolchain_test
{
    class Program
    {
        static void Main(string[] args)
        {
            var logName = Environment.GetEnvironmentVariable("LOGNAME");
            Console.WriteLine($"Hello World! from {logName}");
        }
    }
}
