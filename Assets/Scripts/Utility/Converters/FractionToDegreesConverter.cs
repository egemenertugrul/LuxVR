using System;
using System.Globalization;

namespace Lux.Converters
{
    public class FractionToDegreesConverter : IConverter
    {
        public static FractionToDegreesConverter Instance { get; } = new FractionToDegreesConverter();

        public double Convert(double value) =>
            value is double doubleValue
                ? doubleValue * 360.0
                : value;

        public double ConvertBack(double value) =>
            value is double doubleValue
                ? doubleValue / 360.0
                : value;
    }
}