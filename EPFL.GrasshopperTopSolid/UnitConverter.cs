using System;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace EPFL.GrasshopperTopSolid
{
    using Units;

    /// <summary>
    /// Represents a converter for converting measurable values back and forth
    /// the Revit internal unit system and a external Rhino unit system.
    /// </summary>
    class UnitConverter
    {
        #region Default Instances
        /// <summary>
        /// Identity <see cref="UnitConverter"/>.
        /// </summary>
        public static readonly UnitConverter Identity = new UnitConverter(UnitScale.None);

        /// <summary>
        /// Default <see cref="UnitConverter"/> for converting to and from Rhino model unit system.
        /// </summary>
        public static readonly UnitConverter Model = new UnitConverter(ActiveSpace.ModelSpace, default);

        /// <summary>
        /// Default <see cref="UnitConverter"/> for converting to and from Rhino page unit system.
        /// </summary>
        public static readonly UnitConverter Page = new UnitConverter(ActiveSpace.PageSpace, default);
        #endregion

        #region Internal Factors
        /// <summary>
        /// Factor to do a direct conversion without any unit scaling.
        /// </summary>
        internal const double NoScale = 1.0;

        /// <summary>
        /// Factor for converting a length from Revit internal units to active Rhino document units.
        /// </summary>
        internal static double ToModelLength => (double)(UnitScale.Internal / Model.UnitScale);

        /// <summary>
        /// Factor for converting a length from active Rhino document units to Revit internal units.
        /// </summary>
        internal static double ToInternalLength => (double)(Model.UnitScale / UnitScale.Internal);
        #endregion

        #region Static Methods
        /// <summary>
        /// Converts a value from Host's internal unit system to <paramref name="target"/> units system.
        /// </summary>
        /// <param name="length">Length value to convert</param>
        /// <param name="target">The unit system to convert to.</param>
        /// <returns>Returns <paramref name="length"/> expressed in <paramref name="target"/> unit system.</returns>
        /// <remarks>
        /// This method may return <see cref="double.NaN"/> if the conversion is not defined.
        /// </remarks>
        public static double ConvertFromInternalUnits(double length, UnitScale target)
        {
            return length * (UnitScale.Internal.Ratio / target.Ratio);
        }

        /// <summary>
        /// Converts a value from <paramref name="source"/> unit system to Host's internal unit system.
        /// </summary>
        /// <param name="length">Length value to convert</param>
        /// <param name="source">The unit system to convert from.</param>
        /// <returns>Returns <paramref name="length"/> expressed in Revit internal unit system.</returns>
        /// <remarks>
        /// This method may return <see cref="double.NaN"/> if the conversion is not defined.
        /// </remarks>
        public static double ConvertToInternalUnits(double length, UnitScale source)
        {
            return length * (source.Ratio / UnitScale.Internal.Ratio);
        }

        /// <summary>
        /// Converts <paramref name="value"/> from <paramref name="source"/> units to <paramref name="target"/> units.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="dimensionality"> 0 = factor, 1 = length, 2 = area, 3 = volumen </param>
        /// <returns>Returns <paramref name="value"/> expressed in <paramref name="target"/> unit system.</returns>
        /// <remarks>
        /// This method may return <see cref="double.NaN"/> if the conversion is not defined.
        /// </remarks>
        internal static double Convert(double value, UnitScale source, UnitScale target, int dimensionality = 1)
        {
            // Fast path.
            switch (dimensionality)
            {
                case -1: return value * (target.Ratio / source.Ratio);
                case 0: return value;
                case +1: return value * (source.Ratio / target.Ratio);
            }

            return value * (Ratio.Pow(source.Ratio, dimensionality) / Ratio.Pow(target.Ratio, dimensionality));
        }

        static double Convert(double value, UnitScale from, UnitScale to)
        {
            return
              Convert(value, from, to);
        }
        #endregion

        #region Methods
        UnitConverter(Func<UnitScale> scale) => unitScale = scale;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitConverter"/> class to the indicated unit system.
        /// </summary>
        /// <param name="scale">A Rhino unit system to be used in conversions.</param>
        public UnitConverter(UnitScale scale)
        {
            if (scale == UnitScale.Unset)
                throw new ArgumentOutOfRangeException(nameof(scale));

            unitScale = () => scale;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitConverter"/> class referencing the indicated Rhino document.
        /// </summary>
        /// <param name="space">A Rhino space to take unit system from.</param>
        /// <param name="rhinoDoc">A Rhino document to be used in conversions, null to reference <see cref="RhinoDoc.ActiveDoc"/>.</param>
        public UnitConverter(ActiveSpace space, RhinoDoc rhinoDoc = default)
        {
            unitScale = () => UnitScale.GetUnitScale(rhinoDoc ?? RhinoDoc.ActiveDoc, space);
        }

        /// <summary>
        /// External unit system used to convert.
        /// </summary>
        /// <remarks>
        /// <para>This external unit system use to be the active Rhino document model or page units.</para>
        /// <para>The internal unit system is always the Revit unit system for lengths (feet).</para>
        /// </remarks>
        public UnitScale UnitScale => unitScale();
        readonly Func<UnitScale> unitScale;

        /// <summary>
        /// Converts a length from internal to external unit system.
        /// </summary>
        /// <param name="length">Length value to convert</param>
        /// <returns>Returns <paramref name="length"/> expressed in converter external unit system.</returns>
        /// <remarks>
        /// This method may return <see cref="double.NaN"/> if the conversion is not defined.
        /// </remarks>
        public double ConvertFromInternalUnits(double length) => ConvertFromInternalUnits(length, UnitScale);

        /// <summary>
        /// Converts a length from external to internal unit system.
        /// </summary>
        /// <param name="length">Length value to convert</param>
        /// <returns>Returns <paramref name="length"/> expressed in Revit internal unit system.</returns>
        /// <remarks>
        /// This method may return <see cref="double.NaN"/> if the conversion is not defined.
        /// </remarks>
        public double ConvertToInternalUnits(double length) => ConvertToInternalUnits(length, UnitScale);

        //internal double ConvertFromInternalUnits(double value)
        //{
        //    return Convert(value, UnitScale.Internal, UnitScale);
        //}

        //internal double ConvertToInternalUnits(double value)
        //{
        //    return Convert(value, UnitScale, UnitScale.Internal);
        //}
        #endregion
    }

    static class UnitConvertible
    {
        #region Scale
        internal static void Scale(ref Interval value, double factor)
        {
            value.T0 *= factor;
            value.T1 *= factor;
        }

        internal static void Scale(ref Point2f value, double factor)
        {
            value.X *= (float)factor;
            value.Y *= (float)factor;
        }
        internal static void Scale(ref Point2d value, double factor)
        {
            value.X *= factor;
            value.Y *= factor;
        }
        internal static void Scale(ref Vector2d value, double factor)
        {
            value.X *= factor;
            value.Y *= factor;
        }
        internal static void Scale(ref Vector2f value, double factor)
        {
            value.X *= (float)factor;
            value.Y *= (float)factor;
        }

        internal static void Scale(ref Point3f value, double factor)
        {
            value.X *= (float)factor;
            value.Y *= (float)factor;
            value.Z *= (float)factor;
        }
        internal static void Scale(ref Point3d value, double factor)
        {
            value.X *= factor;
            value.Y *= factor;
            value.Z *= factor;
        }
        internal static void Scale(ref Vector3d value, double factor)
        {
            value.X *= factor;
            value.Y *= factor;
            value.Z *= factor;
        }
        internal static void Scale(ref Vector3f value, double factor)
        {
            value.X *= (float)factor;
            value.Y *= (float)factor;
            value.Z *= (float)factor;
        }

        internal static void Scale(ref Transform value, double scaleFactor)
        {
            value.M03 *= scaleFactor;
            value.M13 *= scaleFactor;
            value.M23 *= scaleFactor;
        }

        internal static void Scale(ref BoundingBox value, double scaleFactor)
        {
            value.Min *= scaleFactor;
            value.Max *= scaleFactor;
        }

        internal static void Scale(ref Plane value, double scaleFactor)
        {
            value.Origin *= scaleFactor;
        }

        internal static void Scale(ref Line value, double scaleFactor)
        {
            value.From *= scaleFactor;
            value.To *= scaleFactor;
        }

        internal static void Scale(ref Arc value, double scaleFactor)
        {
            var plane = value.Plane;
            plane.Origin *= scaleFactor;
            value.Plane = plane;
            value.Radius *= scaleFactor;
        }

        internal static void Scale(ref Circle value, double scaleFactor)
        {
            var plane = value.Plane;
            plane.Origin *= scaleFactor;
            value.Plane = plane;
            value.Radius *= scaleFactor;
        }

        internal static void Scale(ref Ellipse value, double scaleFactor)
        {
            var plane = value.Plane;
            plane.Origin *= scaleFactor;
            value.Plane = plane;
            value.Radius1 *= scaleFactor;
            value.Radius2 *= scaleFactor;
        }

        /// <summary>
        /// Scales <paramref name="value"/> instance by <paramref name="factor"/> in place.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="factor"></param>
        /// <seealso cref="InOtherUnits{G}(G, double)"/>
        internal static void Scale<G>(G value, double factor) where G : GeometryBase
        {
            if (factor != 1.0 && value?.Scale(factor) == false)
                throw new InvalidOperationException($"Failed to Change {value} basis");
        }
        #endregion

        #region InOtherUnits
        internal static Interval InOtherUnits(this Interval value, double factor)
        { Scale(ref value, factor); return value; }

        internal static Point3f InOtherUnits(this Point3f value, double factor)
        { Scale(ref value, factor); return value; }

        internal static Point3d InOtherUnits(this Point3d value, double factor)
        { Scale(ref value, factor); return value; }

        internal static Vector3d InOtherUnits(this Vector3d value, double factor)
        { Scale(ref value, factor); return value; }

        internal static Vector3f InOtherUnits(this Vector3f value, double factor)
        { Scale(ref value, factor); return value; }

        internal static Transform InOtherUnits(this Transform value, double factor)
        { Scale(ref value, factor); return value; }

        internal static BoundingBox InOtherUnits(this BoundingBox value, double factor)
        { Scale(ref value, factor); return value; }

        internal static Plane InOtherUnits(this Plane value, double factor)
        { Scale(ref value, factor); return value; }

        internal static Line InOtherUnits(this Line value, double factor)
        { Scale(ref value, factor); return value; }

        internal static Arc InOtherUnits(this Arc value, double factor)
        { Scale(ref value, factor); return value; }

        internal static Circle InOtherUnits(this Circle value, double factor)
        { Scale(ref value, factor); return value; }

        internal static Ellipse InOtherUnits(this Ellipse value, double factor)
        { Scale(ref value, factor); return value; }

        /// <summary>
        /// Duplicates and scales <paramref name="value"/> to be stored in other units.
        /// <para>See <see cref="Scale{G}(G, double)"/> for in place scaling.</para>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="factor"></param>
        /// <returns>Returns a scaled duplicate of the input <paramref name="value"/> in other units.</returns>
        internal static G InOtherUnits<G>(this G value, double factor) where G : GeometryBase
        { value = (G)value.Duplicate(); if (factor != 1.0) Scale(value, factor); return value; }

        static double InOtherUnits(double value, UnitScale from, UnitScale to)
        {
            return
              UnitConverter.Convert(value, from, to);
        }
        #endregion

        #region InRhinoUnits
        public static Interval InRhinoUnits(this Interval value)
        { Scale(ref value, UnitConverter.ToModelLength); return value; }

        public static Point3f InRhinoUnits(this Point3f value)
        { Scale(ref value, UnitConverter.ToModelLength); return value; }

        public static Point3d InRhinoUnits(this Point3d value)
        { Scale(ref value, UnitConverter.ToModelLength); return value; }

        public static Vector3d InRhinoUnits(this Vector3d value)
        { Scale(ref value, UnitConverter.ToModelLength); return value; }

        public static Vector3f InRhinoUnits(this Vector3f value)
        { Scale(ref value, UnitConverter.ToModelLength); return value; }

        public static Transform InRhinoUnits(this Transform value)
        { Scale(ref value, UnitConverter.ToModelLength); return value; }

        public static BoundingBox InRhinoUnits(this BoundingBox value)
        { Scale(ref value, UnitConverter.ToModelLength); return value; }

        public static Plane InRhinoUnits(this Plane value)
        { Scale(ref value, UnitConverter.ToModelLength); return value; }

        public static Line InRhinoUnits(this Line value)
        { Scale(ref value, UnitConverter.ToModelLength); return value; }

        public static Arc InRhinoUnits(this Arc value)
        { Scale(ref value, UnitConverter.ToModelLength); return value; }

        public static Circle InRhinoUnits(this Circle value)
        { Scale(ref value, UnitConverter.ToModelLength); return value; }

        public static Ellipse InRhinoUnits(this Ellipse value)
        { Scale(ref value, UnitConverter.ToModelLength); return value; }

        /// <summary>
        /// Duplicates and scales <paramref name="value"/> to be stored in Acitve Rhino document units.
        /// <para>See <see cref="Scale{G}(G, double)"/> for in place scaling.</para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns a scaled duplicate of the input <paramref name="value"/> in Active Rhino document units.</returns>
        public static G InRhinoUnits<G>(this G value) where G : GeometryBase
        { Scale(value = (G)value.Duplicate(), UnitConverter.ToModelLength); return value; }

        internal static double InRhinoUnits(double value) =>
          InOtherUnits(value, UnitScale.Internal, UnitConverter.Model.UnitScale);

        internal static double InRhinoUnits(double value, RhinoDoc rhinoDoc)
        {
            if (rhinoDoc is null)
                throw new ArgumentNullException(nameof(rhinoDoc));

            return InOtherUnits(value, UnitScale.Internal, UnitScale.GetModelScale(rhinoDoc));
        }
        #endregion

        #region InHostUnits
        public static Interval InHostUnits(this Interval value)
        { Scale(ref value, UnitConverter.ToInternalLength); return value; }

        public static Point3f InHostUnits(this Point3f value)
        { Scale(ref value, UnitConverter.ToInternalLength); return value; }

        public static Point3d InHostUnits(this Point3d value)
        { Scale(ref value, UnitConverter.ToInternalLength); return value; }

        public static Vector3d InHostUnits(this Vector3d value)
        { Scale(ref value, UnitConverter.ToInternalLength); return value; }

        public static Vector3f InHostUnits(this Vector3f value)
        { Scale(ref value, UnitConverter.ToInternalLength); return value; }

        public static Transform InHostUnits(this Transform value)
        { Scale(ref value, UnitConverter.ToInternalLength); return value; }

        public static BoundingBox InHostUnits(this BoundingBox value)
        { Scale(ref value, UnitConverter.ToInternalLength); return value; }

        public static Plane InHostUnits(this Plane value)
        { Scale(ref value, UnitConverter.ToInternalLength); return value; }

        public static Line InHostUnits(this Line value)
        { Scale(ref value, UnitConverter.ToInternalLength); return value; }

        public static Arc InHostUnits(this Arc value)
        { Scale(ref value, UnitConverter.ToInternalLength); return value; }

        public static Circle InHostUnits(this Circle value)
        { Scale(ref value, UnitConverter.ToInternalLength); return value; }

        public static Ellipse InHostUnits(this Ellipse value)
        { Scale(ref value, UnitConverter.ToInternalLength); return value; }

        /// <summary>
        /// Duplicates and scales <paramref name="value"/> to be stored Revit internal units.
        /// <para>See <see cref="Scale{G}(G, double)"/> for in place scaling.</para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns a duplicate of <paramref name="value"/> in Revit internal units.</returns>
        public static G InHostUnits<G>(this G value) where G : GeometryBase
        { Scale(value = (G)value.Duplicate(), UnitConverter.ToInternalLength); return value; }

        internal static double InHostUnits(double value) =>
          InOtherUnits(value, UnitConverter.Model.UnitScale, UnitScale.Internal);

        internal static double InHostUnits(double value, RhinoDoc rhinoDoc)
        {
            if (rhinoDoc is null)
                return double.NaN;

            return InOtherUnits(value, UnitScale.GetModelScale(rhinoDoc), UnitScale.Internal);
        }
        #endregion
    }
}
