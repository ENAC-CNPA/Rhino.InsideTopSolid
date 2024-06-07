using EPFL.GrasshopperTopSolid;

namespace EPFL.GrasshopperTopSolid
{
    using Units;

    /// <summary>
    /// Tolerance values to be used on <see cref="TopSolid.Kernel.G.IGeometry"/> instances.
    /// </summary>
    readonly struct GeometryTolerance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryTolerance"/> using the indicated unit system.
        /// </summary>
        /// <param name="scale">A <see cref="Rhino.UnitSystem"/> to be used in conversions.</param>
        public GeometryTolerance(UnitScale scale)
        {
            if (scale == UnitScale.None)
            {
                DefaultTolerance = Internal.DefaultTolerance;
                AngleTolerance = Internal.AngleTolerance;
                VertexTolerance = Internal.VertexTolerance;
                ShortCurveTolerance = Internal.ShortCurveTolerance;
            }
            else
            {
                DefaultTolerance = UnitScale.Convert(Internal.DefaultTolerance, UnitScale.Internal, scale);
                AngleTolerance = Internal.AngleTolerance;
                VertexTolerance = UnitScale.Convert(Internal.VertexTolerance, UnitScale.Internal, scale);
                ShortCurveTolerance = UnitScale.Convert(Internal.ShortCurveTolerance, UnitScale.Internal, scale);
            }
        }

        internal GeometryTolerance(double tol, double angle, double vertex, double curve)
        {
            DefaultTolerance = tol;
            AngleTolerance = angle;
            VertexTolerance = vertex;
            ShortCurveTolerance = curve;
        }

        /// <summary>
        /// Default tolerance.
        /// </summary>
        /// <remarks>
        /// Two vectors within this distance are considered coincident.
        /// </remarks>
        public readonly Tolerance DefaultTolerance;

        /// <summary>
        /// Angle tolerance.
        /// </summary>
        /// <remarks>
        /// Two angle measurements closer than this value are considered identical. Value is in radians.
        /// </remarks>
        public readonly Tolerance AngleTolerance;

        /// <summary>
        /// Vertex tolerance.
        /// </summary>
        /// <remarks>
        /// Two points within this distance are considered coincident.
        /// </remarks>
        public readonly Tolerance VertexTolerance;

        /// <summary>
        /// Curve length tolerance
        /// </summary>
        /// <remarks>
        /// A curve shorter than this distance is considered degenerated.
        /// </remarks>
        public readonly Tolerance ShortCurveTolerance;

        // Initialized to NaN to notice if Internal is used before is assigned.
        //
        //private const double AbsoluteTolerance = (1.0 / 12.0) / 16.0; // 1/16″ in feet
        //public static GeometryObjectTolerance Internal { get; internal set; } = new GeometryObjectTolerance
        //(
        //  angle:  Math.PI / 1800.0, // 0.1° in rad,
        //  vertex: AbsoluteTolerance / 10.0,
        //  curve:  AbsoluteTolerance / 2.0
        //);

        /// <summary>
        /// Default <see cref="GeometryTolerance"/> to be used on <see cref="ARDB.GeometryObject"/> instances.
        /// </summary>
        public static GeometryTolerance Internal { get; internal set; } = new GeometryTolerance(double.NaN, double.NaN, double.NaN, double.NaN);

        /// <summary>
        /// Default <see cref="GeometryTolerance"/> expresed in Rhino model unit system.
        /// </summary>
        public static GeometryTolerance Model => new GeometryTolerance(UnitConverter.Model.UnitScale);

        /// <summary>
        /// Default <see cref="GeometryTolerance"/> expresed in Rhino page unit system.
        /// </summary>
        public static GeometryTolerance Page => new GeometryTolerance(UnitConverter.Page.UnitScale);
    }
}
