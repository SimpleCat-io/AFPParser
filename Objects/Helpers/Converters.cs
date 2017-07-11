﻿using System;
using System.Text;

namespace AFPParser
{
    public static class Converters
    {
        public static Encoding EBCDIC => Encoding.GetEncoding("IBM037");

        public enum eMeasurement { Inches, Centimeters }
        public static eMeasurement GetBaseUnit(byte b)
        {
            return CommonMappings.UnitBase[b] == CommonMappings.UnitBase[0] ? eMeasurement.Inches : eMeasurement.Centimeters;
        }
        public static double GetMeasurement(int units, int unitsPerBase)
        {
            return Math.Round(units / (unitsPerBase / 10.0), 3);
        }
        public static double GetInches(int units, int unitsPerBase, eMeasurement measurement)
        {
            double measured = GetMeasurement(units, unitsPerBase);
            if (measurement == eMeasurement.Centimeters) measured *= 2.54;
            return measured;
        }
        public static double GetCentimeters(int units, int unitsPerBase, eMeasurement measurement)
        {
            double measured = GetMeasurement(units, unitsPerBase);
            if (measurement == eMeasurement.Inches) measured /= 2.54;
            return measured;
        }
    }
}
