using System;
using System.Diagnostics;
using System.Reflection;
using TopSolid.Kernel.TX.Units;

namespace EPFL.GrasshopperTopSolid.Units
{


    static class UnitScaleConverter
    {

        public static UnitScale ToUnitScale(SimpleUnit value)
        {
            if (value.Type != UnitType.Length)
                throw new ConversionException($"{value} is not a length unit");

            if (value == LengthUnits.Meter) return UnitScale.Meters;
            //if (value == LengthUnits.MetersCentimeters) return UnitScale.Meters;
            if (value == LengthUnits.Decimeter) return UnitScale.Decimeters;
            if (value == LengthUnits.Centimeter) return UnitScale.Centimeters;
            if (value == LengthUnits.Millimeter) return UnitScale.Millimeters;

            if (value == LengthUnits.Inch) return UnitScale.Inches;
            //if (value == LengthUnits.FractionalInch) return UnitScale.Inches;
            if (value == LengthUnits.Foot) return UnitScale.Feet;
            //if (value == LengthUnits.FeetFractionalInches) return UnitScale.Feet;
            //if (value == LengthUnits.UsSurveyFeet) return UnitScale.UsSurveyFeet;

            Debug.Fail($"{value} conversion is not implemented");
            return UnitScale.Unset;
        }

        //public static UnitScale ToUnitScale(this SimpleUnit value, out int distanceDisplayPrecision)
        //{
        //    var lengthFormatoptions = value.GetFormatOptions(DBXS.SpecType.Measurable.Length);
        //    distanceDisplayPrecision = (int)Arithmetic.Clamp(Math.Truncate(-Math.Log10(lengthFormatoptions.Accuracy)), 0, 7);
        //    return ToUnitScale(lengthFormatoptions.GetUnitTypeId());
        //}
    }
}

