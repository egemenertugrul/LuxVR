using System;

namespace Lux.Extensions
{
    //
    // Summary:
    //     Extensions for time types.
    public static class TimeExtensions
    {
        /// <summary>
        /// Multiplies a timespan by an integer value
        /// </summary>
        public static TimeSpan Multiply(this TimeSpan multiplicand, int multiplier)
        {
            return TimeSpan.FromTicks(multiplicand.Ticks * multiplier);
        } 

        /// <summary>
        /// Multiplies a timespan by a double value
        /// </summary>
        public static TimeSpan Multiply(this TimeSpan multiplicand, double multiplier)
        {
            return TimeSpan.FromTicks((long)(multiplicand.Ticks * multiplier));
        }

        /// <summary>
        /// Divide a timespan by a timespan
        /// </summary>
        public static double Divide(this TimeSpan ts1, TimeSpan ts2)
        {
            return ts1.Ticks / (double)ts2.Ticks;
        }
    }
}