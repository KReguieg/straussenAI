using System;
using UnityEngine;

public static class MathHelper
{
    /// <summary>Remaps values from the input range [inputRangeMin to inputRangeMax] into the output range [outputRangeMin to outputRangeMax] on a per-component basis.
    /// Equivalent to Lerp(oMin, oMax, InverseLerp(iMin, iMax, t))</summary>
    /// <param name="inputRangeMin">The start value of the input range</param>
    /// <param name="inputRangeMax">The end value of the input range</param>
    /// <param name="outputRangeMin">The start value of the output range</param>
    /// <param name="outputRangeMax">The end value of the output range</param>
    /// <param name="value">The value used to interpolate</param>
    public static float Remap(float inputRangeMin, float inputRangeMax, float outputRangeMin, float outputRangeMax,
        float value)
    {
        return Mathf.Lerp(outputRangeMin, outputRangeMax, Mathf.InverseLerp(inputRangeMin, inputRangeMax, value));
    }

    /// <summary> Reflects a given rotation against a normal vector.</summary>
    /// <param name="source">The rotation that shall be mirrored</param>
    /// <param name="normal">The normal direction that is used for the reflection</param>
    public static Quaternion ReflectRotation(Quaternion source, Vector3 normal)
    {
        return Quaternion.LookRotation(Vector3.Reflect(source * Vector3.forward, normal),
            Vector3.Reflect(source * Vector3.up, normal));
    }

    /// <summary>
    /// Checks if two floating-point numbers, a and b, are similar up to a specified number of decimal places.
    /// </summary>
    /// <param name="a">The first number to compare</param>
    /// <param name="b">The second number to compare</param>
    /// <param name="decimalPlaces">The number of decimal places to consider for the comparison</param>
    /// <returns>Returns true if the absolute difference between a and b is less than the tolerance defined by the
    /// number of decimal places; otherwise, returns false.</returns>
    public static bool AreSimilar(float a, float b, int decimalPlaces)
    {
        if (decimalPlaces < 1)
        {
            if (decimalPlaces == 0)
            {
                return Mathf.Abs(a - b) == 0;
            }

            decimalPlaces = Math.Abs(decimalPlaces);
        }

        var tolerance = 1f / Mathf.Pow(10f, decimalPlaces);
        return Mathf.Abs(a - b) < tolerance;
    }

    /// <summary>
    /// Truncates a given floating point value to x decimals after the decimal point.
    /// </summary>
    /// <param name="value">The floating point value to be rounded.</param>
    /// <param name="digitsAfterDecimal">The number of digits after the decimal</param>
    /// <returns>The rounded decimal.</returns>
    public static float TruncateAtDecimal(float value, int digitsAfterDecimal)
    {
        var divisor = Math.Pow(10, digitsAfterDecimal);
        return (float)(Mathf.Round((float)(value * divisor)) / divisor);
    }

    /// <summary>
    /// Truncates the floating point values of a Vector3 to x decimals after the decimal point.
    /// </summary>
    /// <param name="vector">The vector to be truncated</param>
    /// <param name="decimalPlaces">The number of digits after the decimal</param>
    /// <returns>The Vector3 with truncated values.</returns>
    public static Vector3 TruncateToDecimals(Vector3 vector, int decimalPlaces)
    {
        var x = TruncateAtDecimal(vector.x, decimalPlaces);
        var y = TruncateAtDecimal(vector.y, decimalPlaces);
        var z = TruncateAtDecimal(vector.z, decimalPlaces);
        return new Vector3(x, y, z);
    }
}