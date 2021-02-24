using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Lux.Extensions
{
    //
    // Summary:
    //     Extensions for numeric types.
    public static class NumericExtensions
    {
        //
        // Summary:
        //     Clamps a value to a given range.
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        }
        //
        // Summary:
        //     Clamps a value to a given range.
        public static T ClampMax<T>(this T value, T max) where T : IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            return result;
        }
        //
        // Summary:
        //     Clamps a value to a given range.
        public static T ClampMin<T>(this T value, T min) where T : IComparable<T>
        {
            T result = value;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        }
    }
}