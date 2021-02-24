using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

//
// Summary:
//     Provides a way to apply custom logic to a binding.
public interface IConverter
{
    //
    // Summary:
    //     Converts a value.
    //
    // Parameters:
    //   value:
    //     The value produced by the binding source.
    //
    //   targetType:
    //     The type of the binding target property.
    //
    //   parameter:
    //     The converter parameter to use.
    //
    //   culture:
    //     The culture to use in the converter.
    //
    // Returns:
    //     A converted value. If the method returns null, the valid null value is used.
    double Convert(double value);
    //
    // Summary:
    //     Converts a value.
    //
    // Parameters:
    //   value:
    //     The value that is produced by the binding target.
    //
    //   targetType:
    //     The type to convert to.
    //
    //   parameter:
    //     The converter parameter to use.
    //
    //   culture:
    //     The culture to use in the converter.
    //
    // Returns:
    //     A converted value. If the method returns null, the valid null value is used.
    double ConvertBack(double value);
}
