using System;
using System.Diagnostics;

namespace GZiper.Core {
    internal static class PcPerformance {
        private static readonly PerformanceCounter RamCounter =
            new PerformanceCounter("Memory", "Available MBytes");

        private static readonly PerformanceCounter CpuCounter =
            new PerformanceCounter("Processor", "% Processor Time", "_Total");

        public static float GetRamValue() {
            return RamCounter.NextValue();
        }

        public static float GetCpuValue() {
            return CpuCounter.NextValue();
        }
    }
}