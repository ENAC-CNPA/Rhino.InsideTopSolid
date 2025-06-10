
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

using System.Linq;


using TopSolid.Kernel.G.D1;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.G.D3.Curves;
using TopSolid.Kernel.G.D3.Shapes;

using TopSolid.Kernel.G.D3.Surfaces;
using TopSolid.Kernel.SX.Collections;
using TopSolid.Kernel.TX.Items;
using TK = TopSolid.Kernel;
//using TK.G = TopSolid.Kernel.G;
//using G.D2 = TopSolid.Kernel.G.D2;
//using G.D3 = TopSolid.Kernel.G.D3;
using TopSolid.Kernel.G;

using TopSolid.Kernel.TX.Units;

using SX = TopSolid.Kernel.SX;
using G = TopSolid.Kernel.G;

namespace EPFL.GrasshopperTopSolid
{
    public static class Convert
    {

        #region scaleManagement To TopSolid

        internal static double ModelScaleFactorToHost => UnitConverter.ToInternalLength;

        internal static GeometryTolerance ToleranceHost => GeometryTolerance.Internal;

        //TODO make ModelScaleFactorToRhino and ToleranceRhino


        public static double ToInternalLength(double value) => ToInternalLength(value, ModelScaleFactorToHost);
        internal static double ToInternalLength(double value, double factor) => value * factor;

        #endregion

        static public double topSolidLinear = TopSolid.Kernel.G.Precision.ModelingLinearTolerance;
        static public double topSolidAngular = TopSolid.Kernel.G.Precision.AngularPrecision;

        #region Point
        #region To Host
        static public TK.G.D3.Point ToHost(this Point3d p)
        {
            return p.ToHost(1.0);
        }
        static public TK.G.D3.Point ToHost(this Point3d p, double scaleFactor)
        {
            return new TK.G.D3.Point(p.X * scaleFactor, p.Y * scaleFactor, p.Z * scaleFactor);
        }

        static public TK.G.D3.Point ToHost(this Point3f p)
        {
            return p.ToHost(1.0);
        }
        static public TK.G.D3.Point ToHost(this Point3f p, double scaleFactor)
        {
            return new TK.G.D3.Point(p.X * scaleFactor, p.Y * scaleFactor, p.Z * scaleFactor);
        }

        static public TK.G.D2.Point ToHost2d(this Point3d p)
        {
            return p.ToHost2d(1.0);
        }
        static public TK.G.D2.Point ToHost2d(this Point3d p, double scaleFactor)
        {
            return new TK.G.D2.Point(p.X * scaleFactor, p.Y * scaleFactor);
        }
        #endregion
        #region To Rhino
        static public Point3d ToRhino(this TK.G.D3.Point p)
        {
            return p.ToRhino(1.0);
        }
        static public Point3d ToRhino(this TK.G.D3.Point p, double scaleFactor)
        {
            return new Point3d(p.X * scaleFactor, p.Y * scaleFactor, p.Z * scaleFactor);
        }

        static public Point2d ToRhino(this TK.G.D2.Point p)
        {
            return p.ToRhino(1.0);
        }
        static public Point2d ToRhino(this TK.G.D2.Point p, double scaleFactor)
        {
            return new Point2d(p.X * scaleFactor, p.Y * scaleFactor);
        }

        #endregion


        #endregion
        #region Vector
        #region To Host
        static public TK.G.D3.Vector ToHost(this Vector3d v)
        {
            return v.ToHost(1.0);
        }
        static public TK.G.D3.Vector ToHost(this Vector3d v, double scaleFactor)
        {
            return new TK.G.D3.Vector(v.X * scaleFactor, v.Y * scaleFactor, v.Z * scaleFactor);
        }
        #endregion
        #region ToRhino
        static public Vector3d ToRhino(this TK.G.D3.Vector v)
        {
            return v.ToRhino(1.0);
        }
        static public Vector3d ToRhino(this TK.G.D3.Vector v, double scaleFactor)
        {
            return new Vector3d(v.X * scaleFactor, v.Y * scaleFactor, v.Z * scaleFactor);
        }

        static public Vector3d ToRhino(this TK.G.D3.UnitVector v)
        {
            return v.ToRhino(1.0);
        }
        static public Vector3d ToRhino(this TK.G.D3.UnitVector v, double scaleFactor)
        {
            return new Vector3d(v.X * scaleFactor, v.Y * scaleFactor, v.Z * scaleFactor);
        }
        #endregion

        #endregion

        #region Line
        #region To Host
        static public TK.G.D3.Curves.LineCurve ToHost(this Rhino.Geometry.Line l)
        {
            return new TK.G.D3.Curves.LineCurve(l.From.ToHost(), l.To.ToHost());
        }
        static public TK.G.D2.Curves.LineCurve ToHost2d(this Rhino.Geometry.LineCurve lineCurve)
        {
            return new TK.G.D2.Curves.LineCurve(lineCurve.PointAtStart.ToHost2d(), lineCurve.PointAtEnd.ToHost2d());
        }
        #endregion
        #region To Rhino
        static public Rhino.Geometry.LineCurve ToRhino(this TK.G.D3.Curves.LineCurve l)
        {
            return l.ToRhino(1.0);
        }
        static public Rhino.Geometry.LineCurve ToRhino(this TK.G.D3.Curves.LineCurve l, double scaleFactor)
        {
            if (l.Range.IsInfinite)
                return new Rhino.Geometry.LineCurve(l.Axis.Po.ToRhino(),
                    new Rhino.Geometry.Point3d(
                    l.Axis.Po.X * scaleFactor + l.Axis.Vx.X * scaleFactor,
                    l.Axis.Po.Y * scaleFactor + l.Axis.Vx.Y * scaleFactor,
                    l.Axis.Po.Z * scaleFactor + l.Axis.Vx.Z * scaleFactor));

            return new Rhino.Geometry.LineCurve(l.Ps.ToRhino(), l.Pe.ToRhino());
        }

        static public Rhino.Geometry.LineCurve ToRhino(this TK.G.D2.Curves.LineCurve l)
        {
            return new Rhino.Geometry.LineCurve(l.Ps.ToRhino(), l.Pe.ToRhino());
        }
        #endregion
        #endregion

        #region Plane
        #region To Host


        static public TK.G.D3.Plane ToHost(this Rhino.Geometry.Plane plane)
        {
            return plane.ToHost(1.0);
        }
        static public TK.G.D3.Plane ToHost(this Rhino.Geometry.Plane plane, double scaleFactor)
        {
            return new TK.G.D3.Plane(plane.Origin.ToHost(),
                new G.D3.UnitVector(plane.XAxis.X * scaleFactor, plane.XAxis.Y * scaleFactor, plane.XAxis.Z * scaleFactor),
                new G.D3.UnitVector(plane.YAxis.X * scaleFactor, plane.YAxis.Y * scaleFactor, plane.YAxis.Z * scaleFactor));
        }

        static public TK.G.D2.Frame ToHost2d(this Rhino.Geometry.Plane plane)
        {
            return plane.ToHost2d(1.0);
        }
        static public TK.G.D2.Frame ToHost2d(this Rhino.Geometry.Plane plane, double scaleFactor)
        {
            return new TK.G.D2.Frame(
                plane.Origin.ToHost2d(),
                new G.D2.UnitVector(plane.XAxis.X * scaleFactor, plane.XAxis.Y * scaleFactor),
                new G.D2.UnitVector(plane.YAxis.X * scaleFactor, plane.YAxis.Y * scaleFactor));
        }
        #endregion

        #region To Rhino

        static public Rhino.Geometry.Plane ToRhino(this TK.G.D3.Frame frame)
        {
            return frame.ToRhino(1.0);
        }
        static public Rhino.Geometry.Plane ToRhino(this TK.G.D3.Frame frame, double scaleFactor)
        {
            return new Rhino.Geometry.Plane(
                frame.Po.ToRhino(),
                new Vector3d(frame.Ax.Vx.X * scaleFactor, frame.Ax.Vx.Y * scaleFactor, frame.Ax.Vx.Z * scaleFactor));
        }

        static public Rhino.Geometry.Plane ToRhino(this TK.G.D2.Frame p)
        {
            return p.ToRhino(1.0);
        }
        static public Rhino.Geometry.Plane ToRhino(this TK.G.D2.Frame p, double scaleFactor)
        {
            Point3d origin = new Point3d(p.Po.X * scaleFactor, p.Po.Y * scaleFactor, 0);
            return new Rhino.Geometry.Plane(
                origin,
                new Vector3d(p.Vx.X * scaleFactor, p.Vx.Y * scaleFactor, 0),
                new Vector3d(p.Vy.X * scaleFactor, p.Vy.Y * scaleFactor, 0));
        }

        static public Rhino.Geometry.Plane ToRhino(this TK.G.D3.Plane p)
        {
            return p.ToRhino(1.0);
        }
        static public Rhino.Geometry.Plane ToRhino(this TK.G.D3.Plane p, double scaleFactor)
        {
            return new Rhino.Geometry.Plane(
                p.Po.ToRhino(),
                new Vector3d(p.Ax.Vx.X * scaleFactor, p.Ax.Vx.Y * scaleFactor, p.Ax.Vx.Z * scaleFactor),
                new Vector3d(p.Ay.Vx.X * scaleFactor, p.Ay.Vx.Y * scaleFactor, p.Ay.Vx.Z * scaleFactor));
        }

        #endregion
        #endregion

        #region Curve

        #region To Rhino

        static public Rhino.Geometry.Curve ToRhino(this G.D3.Curves.Curve curve)
        {
            if (curve is CircleCurve tscircle)
                return tscircle.ToRhino();

            if (curve is EllipseCurve tsEllipse)
                return tsEllipse.ToRhino();

            if (curve is G.D3.Curves.LineCurve tsline)
                return tsline.ToRhino();
            if (curve is G.D3.Curves.BSplineCurve bsCurve)
                return bsCurve.ToRhino();

            return curve.GetBSplineCurve(false, false).ToRhino();
        }

        static public Rhino.Geometry.NurbsCurve ToRhino(this G.D3.Curves.EllipseCurve ellipseCurve)
        {
            return ellipseCurve.ToRhino(1.0);
        }
        static public Rhino.Geometry.NurbsCurve ToRhino(this G.D3.Curves.EllipseCurve ellipseCurve, double scaleFactor)
        {
            var ellipse = new Rhino.Geometry.Ellipse(ellipseCurve.Plane.ToRhino(), ellipseCurve.RadiusX * scaleFactor, ellipseCurve.RadiusY * scaleFactor);
            //bool isClosed = ellipseCurve.IsClosed();
            if (!ellipseCurve.IsClosed())
            {
                return ellipseCurve.GetBSplineCurve(true, true).ToRhino();
            }
            return NurbsCurve.CreateFromEllipse(ellipse);
        }

        static public Rhino.Geometry.Curve ToRhino(this G.D2.Curves.Curve curve)
        {
            return curve.ToRhino(1.0);
        }
        static public Rhino.Geometry.Curve ToRhino(this G.D2.Curves.Curve curve, double scaleFactor)
        {
            if (curve is G.D2.Curves.LineCurve tsline)
                return tsline.ToRhino();

            if (curve is G.D2.Curves.CircleCurve tsCircle)
                return tsCircle.ToRhino();

            if (curve is G.D2.Curves.EllipseCurve tsEllipse)
                return new Rhino.Geometry.Ellipse(tsEllipse.Frame.ToRhino(), tsEllipse.RadiusX * scaleFactor, tsEllipse.RadiusY * scaleFactor).ToNurbsCurve();

            if (curve is G.D2.Curves.BSplineCurve bsCurve)
                return bsCurve.ToRhino();

            return curve.GetBSplineCurve(false, false).ToRhino();
        }

        static public Rhino.Geometry.ArcCurve ToRhino(this G.D3.Curves.CircleCurve circleCurve)
        {
            return circleCurve.ToRhino(1.0);
        }
        static public Rhino.Geometry.ArcCurve ToRhino(this G.D3.Curves.CircleCurve circleCurve, double scaleFactor)
        {
            if (circleCurve.IsClosed())
            {
                Circle circle = new Circle(circleCurve.Plane.ToRhino(), circleCurve.Radius * scaleFactor);
                return new ArcCurve(circle);
            }

            Arc arc = new Arc(circleCurve.Ps.ToRhino(), circleCurve.Pm.ToRhino(), circleCurve.Pe.ToRhino());
            return new ArcCurve(arc);
        }

        static public Rhino.Geometry.ArcCurve ToRhino(this G.D2.Curves.CircleCurve circleCurve)
        {
            return circleCurve.ToRhino(1.0);
        }

        static public Rhino.Geometry.ArcCurve ToRhino(this G.D2.Curves.CircleCurve circleCurve, double scaleFactor)
        {
            if (circleCurve.IsClosed())
            {
                Circle circle = new Circle(circleCurve.Frame.ToRhino(), circleCurve.Radius * scaleFactor);
                return new ArcCurve(circle);
            }
            circleCurve.Ps.ToRhino();
            Arc arc = new Arc(
                new Rhino.Geometry.Point3d(circleCurve.Ps.X * scaleFactor, circleCurve.Ps.Y * scaleFactor, 0),
                new Rhino.Geometry.Point3d(circleCurve.Pm.X * scaleFactor, circleCurve.Pm.Y * scaleFactor, 0),
                new Rhino.Geometry.Point3d(circleCurve.Pe.X * scaleFactor, circleCurve.Pe.Y * scaleFactor, 0)
            );

            //Arc arc = new Arc(circleCurve.Ps.), circleCurve.Pm.ToRhino(), circleCurve.Pe.ToRhino());
            return new ArcCurve(arc);
        }

        static public Rhino.Geometry.Curve ToRhino(this G.D3.Curves.IGeometricProfile profile)
        {
            if (profile.SegmentCount == 1)
                return profile.Segments.First().GetOrientedCurve().Curve.ToRhino();

            PolyCurve rhCurve = new PolyCurve();
            foreach (IGeometricSegment seg in profile.Segments)
            {
                rhCurve.AppendSegment(seg.GetOrientedCurve().Curve.GetBSplineCurve(false, false).ToRhino());
            }

            return rhCurve.ToNurbsCurve();
        }

        static public Rhino.Geometry.Curve ToRhino(this G.D2.Curves.IGeometricProfile profile)
        {
            if (profile.SegmentCount == 1)
                return profile.Segments.First().Curve.ToRhino();

            PolyCurve rhCurve = new PolyCurve();
            foreach (G.D2.Curves.IGeometricSegment seg in profile.Segments)
            {
                rhCurve.AppendSegment(seg.GetOrientedCurve().Curve.GetBSplineCurve(false, false).ToRhino());
            }

            return rhCurve.ToNurbsCurve();
        }




        static public Rhino.Geometry.NurbsCurve ToRhino(this G.D2.Curves.BSplineCurve curve)
        {
            curve.MakeNonPeriodic();

            Rhino.Collections.Point3dList Cpts = new Rhino.Collections.Point3dList();
            Rhino.Geometry.NurbsCurve rhCurve;
            double tol_TS = TopSolid.Kernel.G.Precision.ModelingLinearTolerance;
            //bool isrational = curve.IsRational;

            foreach (TopSolid.Kernel.G.D2.Point controlPoint in curve.CPts)
            {
                Cpts.Add(controlPoint.ToRhino().X, controlPoint.ToRhino().Y, 0);
            }

            rhCurve = NurbsCurve.Create(curve.IsPeriodic, curve.Degree, Cpts);

            double weight = 1;
            int k = 0;

            if (curve.CWts != null && !curve.CWts.IsEmpty)
            {
                foreach (Point3d P in Cpts)
                {
                    weight = curve.CWts[k];
                    rhCurve.Points.SetPoint(k, P, weight);
                    k++;
                }
            }

            else
            {
                foreach (Point3d P in Cpts)
                {
                    rhCurve.Points.SetPoint(k, P, weight);
                    k++;
                }
            }

            for (int i = 1; i < curve.Bs.Count - 1; i++)
            {
                rhCurve.Knots[i - 1] = curve.Bs[i];
            }

            bool rev;
            if (curve.IsClosed())
                rev = rhCurve.MakeClosed(tol_TS);

            return rhCurve;
        }

        static public Rhino.Geometry.PolyCurve ToRhino(this G.D3.Sketches.Profile profile)
        {
            Rhino.Geometry.PolyCurve rhCurve = new PolyCurve();

            for (int i = 0; i < (profile.Segments.Count()); i++)
            {
                rhCurve.Append(profile.Segments.ElementAt(i).Geometry.ToRhino());
            }

            return rhCurve;
        }

        static public Rhino.Geometry.PolyCurve ToRhino(this G.D2.Sketches.Profile profile)
        {
            Rhino.Geometry.PolyCurve rhCurve = new PolyCurve();

            for (int i = 0; i < (profile.Segments.Count()); i++)
            {
                Rhino.Geometry.Curve curve = profile.MakeGeometricProfile().Segments.ElementAt(i).Curve.ToRhino();
                rhCurve.Append(profile.MakeGeometricProfile().Segments.ElementAt(i).Curve.ToRhino());
            }

            return rhCurve;
        }
        #endregion


        #region To Host

        static public TopSolid.Kernel.G.D3.Curves.Curve ToHost(this Rhino.Geometry.Curve rhinoCurve)
        {
            if (rhinoCurve is ArcCurve arcCurve)
                return arcCurve.ToHost();
            if (rhinoCurve is Rhino.Geometry.LineCurve lineCurve)
                return lineCurve.ToHost();
            if (rhinoCurve is Rhino.Geometry.PolylineCurve polyLineCurve)
                return polyLineCurve.ToHost();
            if (rhinoCurve is PolyCurve polyCurve)
                return polyCurve.ToHost();
            if (rhinoCurve is NurbsCurve nurbsCurve)
                return nurbsCurve.ToHost();

            return rhinoCurve.ToNurbsCurve().ToHost();
        }

        static public TopSolid.Kernel.G.D3.Curves.Curve ToHost(this Rhino.Geometry.ArcCurve arcCurve)
        {
            G.D3.Point Ps, Pm, Pe;
            CircleMaker circleMaker = new CircleMaker();
            Arc arc = arcCurve.Arc;

            if (arc.IsCircle)
            {
                Ps = arc.PointAt(0.25).ToHost();
                Pm = arc.PointAt(0.5).ToHost();
                Pe = arc.PointAt(0.75).ToHost();
                return circleMaker.MakeByThreePoints(Ps, Pm, Pe, true);
            }

            Ps = arc.StartPoint.ToHost();
            Pm = arc.MidPoint.ToHost();
            Pe = arc.EndPoint.ToHost();

            return circleMaker.MakeByThreePoints(Ps, Pm, Pe, false);
        }

        static public TopSolid.Kernel.G.D3.Curves.LineCurve ToHost(this Rhino.Geometry.LineCurve lineCurve)
        {
            return new G.D3.Curves.LineCurve(lineCurve.PointAtStart.ToHost(), lineCurve.PointAtEnd.ToHost());
        }

        static public TopSolid.Kernel.G.D3.Curves.PolylineCurve ToHost(this Rhino.Geometry.PolylineCurve polylineCurve)
        {
            Polyline polyline = polylineCurve.ToPolyline();
            G.D3.PointList pointList = new G.D3.PointList(polyline.Count);
            foreach (G.D3.Point point in polyline.Select(x => x.ToHost()))
            {
                pointList.Add(point);
            }
            return new G.D3.Curves.PolylineCurve(polyline.IsClosed, pointList);
        }

        static public G.D3.Curves.Curve ToHost(this Rhino.Geometry.PolyCurve polyCurve)
        {
            return polyCurve.ToNurbsCurve().ToHost();
        }


        static public TopSolid.Kernel.G.D3.Curves.BSplineCurve ToHost(this Rhino.Geometry.NurbsCurve nurbsCurve)
        {
            bool isRational = nurbsCurve.IsRational;
            int degree = nurbsCurve.Degree;
            int offKnot, nbCP;
            DoubleList topKnots = new DoubleList();
            bool isPeriodic = nurbsCurve.IsPeriodic;

            if (isPeriodic)
            {
                offKnot = nurbsCurve.Degree - 1;
                nbCP = nurbsCurve.Points.Count - nurbsCurve.Degree;
            }
            else
            {
                offKnot = 0;
                nbCP = nurbsCurve.Points.Count;

                topKnots.Add(nurbsCurve.Knots[0]);
            }

            for (int i = offKnot; i < nurbsCurve.Knots.Count - offKnot; i++)
                topKnots.Add(nurbsCurve.Knots[i]);

            if (isPeriodic == false)
                topKnots.Add(nurbsCurve.Knots.Last());

            DoubleList topWeights = new DoubleList();
            var pnts = new G.D3.PointList();
            for (int i = 0; i < nbCP; i++)
            {
                if (nurbsCurve.IsRational)
                {
                    topWeights.Add(nurbsCurve.Points[i].Weight);
                }
                pnts.Add(nurbsCurve.Points[i].Location.ToHost());
            }

            BSpline bspline = new BSpline(isPeriodic, degree, topKnots);
            if (isRational)
            {
                BSplineCurve bsplineCurve = new BSplineCurve(bspline, pnts, topWeights);
                return bsplineCurve;
            }
            else
            {
                BSplineCurve bsplineCurve = new BSplineCurve(bspline, pnts);
                return bsplineCurve;
            }
        }

        static public TopSolid.Kernel.G.D2.Curves.Curve ToHost2d(this Rhino.Geometry.Curve rhinoCurve)
        {
            return rhinoCurve.ToHost2d(1.0);
        }
        static public TopSolid.Kernel.G.D2.Curves.Curve ToHost2d(this Rhino.Geometry.Curve rhinoCurve, double scaleFactor)
        {
            if (rhinoCurve is Rhino.Geometry.LineCurve line)
            {
                return line.ToHost2d();
            }

            if (rhinoCurve is ArcCurve arcCurve)
            {
                Arc arc = arcCurve.Arc;
                G.D2.Frame plane2d = arc.Plane.ToHost2d();
                G.D2.Curves.CircleCurve circleCurve = new G.D2.Curves.CircleCurve(plane2d, arc.Radius * scaleFactor);
                G.D2.Curves.CircleMaker maker = new G.D2.Curves.CircleMaker(SX.Version.Current, topSolidLinear, topSolidAngular);
                maker.SetByCenterAndTwoPoints(
                    arc.Center.ToHost2d(),
                    arc.StartPoint.ToHost2d(),
                    arc.EndPoint.ToHost2d(),
                    false,
                    circleCurve);
                return circleCurve;
            }

            if (rhinoCurve is NurbsCurve nurbsCurve)
            {
                return nurbsCurve.ToHost2d();
            }
            return rhinoCurve.ToNurbsCurve().ToHost2d();
        }


        static public TopSolid.Kernel.G.D2.Curves.BSplineCurve ToHost2d(this Rhino.Geometry.NurbsCurve nurbsCurve, G.D3.Surfaces.Surface surface = null)
        {
            bool isRational = nurbsCurve.IsRational;
            bool isPeriodic = nurbsCurve.IsPeriodic;
            int degree = nurbsCurve.Degree;
            DoubleList knotList = ToDoubleList(nurbsCurve.Knots);
            G.D2.PointList pts = ToPointList2D(nurbsCurve.Points);
            DoubleList weightList = ToDoubleList(nurbsCurve.Points);
            BSpline b = new BSpline(isPeriodic, degree, knotList);
            if (isRational)
            {
                //var w = c.Points.ConvertAll(x => x.Weight);
                G.D2.Curves.BSplineCurve bsplineCurve = new G.D2.Curves.BSplineCurve(b, pts, weightList);
                return bsplineCurve;
            }
            else
            {
                G.D2.Curves.BSplineCurve bsplineCurve = new G.D2.Curves.BSplineCurve(b, pts);
                return bsplineCurve;
            }

        }

        #endregion
        /// <summary>
        /// Converts a single segment of a TopSolid Profile to a Rhino NurbsCurve
        /// </summary>
        /// <param name="curve">BSplineCurve to convert</param>
        /// <returns></returns>
        static public Rhino.Geometry.NurbsCurve ToRhino(this BSplineCurve curve)
        {
            curve.MakeNonPeriodic();

            Rhino.Collections.Point3dList Cpts = new Rhino.Collections.Point3dList();

            //curve = curve.GetBSplineCurve(false, false);
            //var x = curve.GeometryType;
            //for Debug
            //if (curve.IsPeriodic)
            //curve.MakeNonPeriodic();

            foreach (TopSolid.Kernel.G.D3.Point P in curve.CPts)
            {
                Cpts.Add(ToRhino(P));
            }

            //int dimension = 3;
            //bool isRational = curve.IsRational;
            //int order = curve.Degree + 1;
            //int pointCount = curve.CPts.Count;

            //NurbsCurve nurbsCurve = new NurbsCurve(dimension, isRational, order, pointCount);

            NurbsCurve nurbsCurve = NurbsCurve.Create(curve.IsPeriodic, curve.Degree, Cpts);


            int k = 0;
            double weight = 1;
            if (curve.CWts != null && !curve.CWts.IsEmpty)
            {
                foreach (Point3d P in Cpts)
                {
                    weight = curve.CWts[k];
                    nurbsCurve.Points.SetPoint(k, P, weight);
                    k++;
                }
            }

            else
            {
                foreach (Point3d P in Cpts)
                {
                    nurbsCurve.Points.SetPoint(k, P, weight);
                    k++;
                }

            }



            bool periodic = curve.Bs.IsPeriodic;

            if (curve.Bs.Count == curve.Degree + curve.CPts.Count - 1)
            {
                for (int i = 0; i < curve.Bs.Count; i++)
                {
                    nurbsCurve.Knots.Append(curve.Bs[i]);
                }
            }

            else
            {
                for (int i = 1; i < curve.Bs.Count - 1; i++)
                {
                    nurbsCurve.Knots.Append(curve.Bs[i]);
                }
            }

            //if (!nurbsCurve.IsValid)
            //{
            //    //string log = "";
            //    //nurbsCurve.IsValidWithLog(out log);
            //    System.Console.WriteLine(log);
            //}

            //bool coincide = curve.IsClosed() && !curve.CPts.ExtractFirst().CoincidesWith(curve.CPts.ExtractLast());

            //if (curve.IsClosed() && !curve.CPts.ExtractFirst().CoincidesWith(curve.CPts.ExtractLast()))
            //{
            //    //nurbsCurve.Points.Append(curve.Ps.ToRhino());
            //    //nurbsCurve.MakeClosed(TK.G.Precision.LinearPrecision);
            //}

            //bool closed = false;
            //if (curve.IsPeriodic)
            //    closed = nurbsCurve.MakeClosed(TK.G.Precision.LinearPrecision);

            return nurbsCurve;

        }



        #endregion
        #region Surface

        public static TK.G.D3.Surfaces.Surface ToHost(this Rhino.Geometry.Surface rhinoSurface)
        {
            if (rhinoSurface is Rhino.Geometry.PlaneSurface planeSurface)
                return planeSurface.ToHost();

            if (rhinoSurface is Extrusion extrusionSurface && extrusionSurface.ProfileCount == 1)
                return extrusionSurface.ToHost();

            if (rhinoSurface is SumSurface sumSurface)
                return sumSurface.ToHost();

            //if (rhinoSurface is Rhino.Geometry.RevSurface rhinoRevSurface)
            //    return rhinoRevSurface.ToHost();

            if (rhinoSurface is NurbsSurface nurbsSurface)
                return nurbsSurface.ToHost();

            return rhinoSurface.ToNurbsSurface().ToHost();

        }
        //public static TK.G.D3.Surfaces.RevolvedSurface ToHost(this Rhino.Geometry.RevSurface rhinorevSurface)
        //{
        //    G.D2.Extent extent = new G.D2.Extent(rhinorevSurface.Domain(0).ToHost(), rhinorevSurface.Domain(1).ToHost());
        //    RevolvedSurface revolvedSurface = new RevolvedSurface(rhinorevSurface.Curve.ToHost(), rhinorevSurface.Axis.LineToAxis(), extent);
        //    return revolvedSurface;
        //}
        public static G.D3.Surfaces.PlaneSurface ToHost(this Rhino.Geometry.PlaneSurface rhinoPlaneSurf)
        {
            Rhino.Geometry.Plane plane = Rhino.Geometry.Plane.WorldXY;
            bool success = rhinoPlaneSurf.TryGetPlane(out plane);
            return new G.D3.Surfaces.PlaneSurface(plane.ToHost(), rhinoPlaneSurf.DomainToTopSolidExtent());
        }
        public static TK.G.D3.Surfaces.ExtrudedSurface ToHost(this Rhino.Geometry.Extrusion rhinoExtrusion)
        {
            G.D3.Curves.Curve profile = rhinoExtrusion.Profile3d(0, 0).ToHost();
            Rhino.Geometry.LineCurve path = rhinoExtrusion.PathLineCurve();
            G.D3.Axis direction = new G.D3.Axis(path.PointAtStart.ToHost(), path.PointAtEnd.ToHost());
            return new ExtrudedSurface(profile, direction.Vx, rhinoExtrusion.DomainToTopSolidExtent());
        }

        public static TK.G.D3.Surfaces.BSplineSurface ToHost(this NurbsSurface nurbsSurface)
        {
            /*
            bool isRational = nurbsSurface.IsRational;
            bool isPeriodicU = nurbsSurface.IsPeriodic(0);
            bool isPeriodicV = nurbsSurface.IsPeriodic(1);
            var degreeU = nurbsSurface.Degree(0);
            var degreeV = nurbsSurface.Degree(1);
            DoubleList knotsU = ToDoubleList(nurbsSurface.KnotsU);
            DoubleList knotsV = ToDoubleList(nurbsSurface.KnotsV);
            PointList controlPoints = ToPointList(nurbsSurface.Points);
            var weightDoubleList = ToDoubleList(nurbsSurface.Points);
            BSpline bsplineU = new BSpline(isPeriodicU, degreeU, knotsU);
            BSpline bsplineV = new BSpline(isPeriodicV, degreeV, knotsV);
            */

            int offKnot, nbCPu, nbCPv;
            DoubleList topKnots = new DoubleList();

            bool isPeriodic = nurbsSurface.IsPeriodic(0);
            if (isPeriodic) // According to v4_WishBone.3dm.
            {
                offKnot = nurbsSurface.Degree(0) - 1;
                nbCPu = nurbsSurface.Points.CountU - nurbsSurface.Degree(0);
            }
            else
            {
                offKnot = 0;
                nbCPu = nurbsSurface.Points.CountU;

                topKnots.Add(nurbsSurface.KnotsU[0]);
            }

            for (int i = offKnot; i < nurbsSurface.KnotsU.Count - offKnot; i++)
                topKnots.Add(nurbsSurface.KnotsU[i]);

            if (isPeriodic == false)
                topKnots.Add(nurbsSurface.KnotsU.Last());

            BSpline bsplineU = new BSpline(isPeriodic, nurbsSurface.Degree(0), topKnots);

            topKnots.Clear();

            isPeriodic = nurbsSurface.IsPeriodic(1);
            if (isPeriodic)
            {
                offKnot = nurbsSurface.Degree(1) - 1;
                nbCPv = nurbsSurface.Points.CountV - nurbsSurface.Degree(1);
            }
            else
            {
                offKnot = 0;
                nbCPv = nurbsSurface.Points.CountV;

                topKnots.Add(nurbsSurface.KnotsV[0]);
            }

            for (int i = offKnot; i < nurbsSurface.KnotsV.Count - offKnot; i++)
                topKnots.Add(nurbsSurface.KnotsV[i]);

            if (isPeriodic == false)
                topKnots.Add(nurbsSurface.KnotsV.Last());

            BSpline bsplV = new BSpline(isPeriodic, nurbsSurface.Degree(1), topKnots);

            DoubleList topWeights = new DoubleList();
            G.D3.PointList topPnts = new G.D3.PointList();


            for (int i = 0; i < nbCPu; i++)
                for (int j = 0; j < nbCPv; j++)
                {
                    if (nurbsSurface.IsRational)
                    {
                        topWeights.Add(nurbsSurface.Points.GetWeight(i, j));
                        //topPnts.Add((new Point3d(nurbsSurface.Points.GetControlPoint(i, j).Location) / nurbsSurface.Points.GetWeight(i, j)).ToHost());
                        topPnts.Add(new Point3d(nurbsSurface.Points.GetControlPoint(i, j).Location).ToHost());
                    }
                    else
                        topPnts.Add((new Point3d(nurbsSurface.Points.GetControlPoint(i, j).Location)).ToHost());
                }

            return new BSplineSurface(bsplineU, bsplV, topPnts, topWeights);




        }



        public static Rhino.Geometry.Surface ToRhino(this IParametricSurface topSolidSurface)
        {
            //Rhino.Geometry.Surface surf = null;
            if (topSolidSurface is G.D3.Surfaces.PlaneSurface planarsurf)
                return new Rhino.Geometry.PlaneSurface(planarsurf.Plane.ToRhino(), planarsurf.Range.XExtent.ToRhino(), planarsurf.Range.YExtent.ToRhino());


            if (topSolidSurface is RevolvedSurface revSurf)
            {
                if (revSurf.Curve.IsLinear())
                {
                    G.D3.Curves.LineCurve line = (G.D3.Curves.LineCurve)revSurf.Curve;
                    return RevSurface.Create(line.ToRhino().Line, new Line(revSurf.Axis.Po.ToRhino(), revSurf.Axis.Vx.ToRhino()));
                }
                return RevSurface.Create(revSurf.Curve.ToRhino(), new Line(revSurf.Axis.Po.ToRhino(), revSurf.Axis.Vx.ToRhino()));
            }

            /*if (topSolidSurface is ConeSurface coneSurface)
                return coneSurface.ToRhino();*/

            if (topSolidSurface is BSplineSurface bsplineSurface)
                return bsplineSurface.ToRhino();

            if (topSolidSurface is ExtrudedSurface extrudedsurface)
                return extrudedsurface.ToRhino();


            G.D3.Surfaces.Surface surface = topSolidSurface as G.D3.Surfaces.Surface;


            return surface.GetBsplineGeometry(TK.G.Precision.LinearPrecision, false, false, false).ToRhino();

        }

        /*
        public static Rhino.Geometry.Surface ToRhino(this ConeSurface coneSurface)
        {
            return coneSurface.ToRhino(1.0);
        }
        public static Rhino.Geometry.Surface ToRhino(this ConeSurface coneSurface, double scaleFactor)
        {
            var vector = new G.D3.Vector(coneSurface.GetApex(), coneSurface.Frame.Po).ToRhino();
            var plane = new Rhino.Geometry.Plane(coneSurface.GetApex().ToRhino(), vector);
            var height = coneSurface.Frame.Po.GetDistance(coneSurface.GetApex());
            Rhino.Geometry.Cone cone = new Cone(plane, height * scaleFactor, coneSurface.Radius * scaleFactor);
            return NurbsSurface.CreateFromCone(cone);
        }
        */

        public static Rhino.Geometry.Surface ToRhino(this G.D3.Surfaces.ExtrudedSurface extrudedSurface)
        {
            return Extrusion.CreateExtrusion(extrudedSurface.Curve.ToRhino(), extrudedSurface.Direction.ToRhino());
        }

        public static Rhino.Geometry.NurbsSurface ToRhino(this BSplineSurface bsplineSurface)
        {
            //bsplineSurface = bsplineSurface.GetBsplineGeometry(Precision.ModelingLinearTolerance, false, false, false);
            bool is_rational = bsplineSurface.IsRational;
            int dim = 3;
            int uDegree = bsplineSurface.UDegree;
            int vDegree = bsplineSurface.VDegree;
            int uCount = bsplineSurface.UCptsCount;
            int vCount = bsplineSurface.VCptsCount;

            var control_points = new Point3d[uCount, vCount];

            for (int u = 0; u < uCount; u++)
            {
                for (int v = 0; v < vCount; v++)
                {
                    control_points[u, v] = bsplineSurface.GetCPt(u, v).ToRhino();
                }
            }

            // creates internal uninitialized arrays for 
            // control points and knots
            var rhinoSurface = NurbsSurface.Create(
              dim,
              is_rational,
              uDegree + 1,
              vDegree + 1,
              uCount,
              vCount
              );

            //bool knotU = uDegree + uCount - 1 == bsplineSurface.UBs.Count - 2;
            //bool knotV = vDegree + vCount - 1 == bsplineSurface.VBs.Count - 2;

            if (bsplineSurface.IsUPeriodic)
            {
                //bool success = rhinoSurface.KnotsU.CreatePeriodicKnots(bsplineSurface.UBs[1] - bsplineSurface.UBs[0]);

                Rhino.Geometry.Surface surface = NurbsSurface.CreatePeriodicSurface(rhinoSurface, 0, false);
                for (int u = 0; u < (bsplineSurface.UBs.Count); u++)
                    rhinoSurface.KnotsU[u] = bsplineSurface.UBs[u];
            }


            else
            {
                for (int u = 1; u < (bsplineSurface.UBs.Count - 1); u++)
                    rhinoSurface.KnotsU[u - 1] = bsplineSurface.UBs[u];
            }

            if (bsplineSurface.IsVPeriodic)
            {

                for (int v = 0; v < (bsplineSurface.VBs.Count); v++)
                    rhinoSurface.KnotsV[v] = bsplineSurface.VBs[v];

            }
            else
            {
                for (int v = 1; v < (bsplineSurface.VBs.Count - 1); v++)
                    rhinoSurface.KnotsV[v - 1] = bsplineSurface.VBs[v];
            }
            double weight = 1;
            bool CwtsNotEmpty = bsplineSurface.CWts != null && !bsplineSurface.CWts.IsEmpty;

            // add the control points
            for (int u = 0; u < rhinoSurface.Points.CountU; u++)
            {
                for (int v = 0; v < rhinoSurface.Points.CountV; v++)
                {
                    if (CwtsNotEmpty)
                        weight = bsplineSurface.GetCWt(u, v);
                    rhinoSurface.Points.SetPoint(u, v, control_points[u, v], weight);
                }
            }
            string log = "";
            var valid = rhinoSurface.IsValidWithLog(out log);
            return rhinoSurface;
        }

        #endregion

        #region Mesh
        static public IGeometry ToHost(this Rhino.Geometry.Mesh rhinoMesh)
        {

            return new Shape(null);
        }


        #endregion

        #region Other Solid or surface Geometries

        public static Rhino.Geometry.Box ToRhino(this G.D3.Box box)
        {
            return new Rhino.Geometry.Box(box.Frame.ToRhino(), box.GetExtent().XExtent.ToRhino(), box.GetExtent().YExtent.ToRhino(), box.GetExtent().ZExtent.ToRhino());
        }

        //public static G.D3.Box ToHost(this Rhino.Geometry.Box box)
        //{

        //    var TsBox = new G.D3.Box(Frame.OXYZ,0, 0, 0);

        //}


        #endregion

        //Methods for IEnumerables and other utility converters
        #region utilities
        static bool KnotAlmostEqualTo(double max, double min) =>
        KnotAlmostEqualTo(max, min, 1.0e-09);

        public static Interval ToRhino(this TK.G.D1.Extent extent)
        {
            return new Interval(extent.Min, extent.Max);
        }



        static bool KnotAlmostEqualTo(double max, double min, double tol)
        {
            var length = max - min;
            if (length <= tol)
                return true;

            return length <= max * tol;
        }


        static double KnotPrevNotEqual(double max) =>
          KnotPrevNotEqual(max, 1.0000000E-9 * 1000.0);

        static double KnotPrevNotEqual(double max, double tol)
        {
            const double delta2 = 2.0 * 1E-16;
            var value = max - tol - delta2;

            if (!KnotAlmostEqualTo(max, value, tol))
                return value;

            return max - (max * (tol + delta2));
        }
        static DoubleList ToDoubleList(NurbsCurvePointList list)
        {
            var count = list.Count;
            DoubleList weightsList = new DoubleList(count);
            foreach (ControlPoint p in list)
            {
                var weight = p.Weight;
                weightsList.Add(weight);
            }
            return weightsList;
        }



        static DoubleList ToDoubleList(NurbsCurveKnotList rhinoKnotlist)
        {
            DoubleList knotDoubleList = new DoubleList();
            knotDoubleList.Add(rhinoKnotlist[0]);
            foreach (var value in rhinoKnotlist)
            {
                knotDoubleList.Add(value);
            }
            knotDoubleList.Add(rhinoKnotlist.Last());
            return knotDoubleList;
        }
        static G.D2.PointList ToPointList2D(this NurbsCurvePointList list)
        {
            return list.ToPointList2D(1.0);
        }
        static G.D2.PointList ToPointList2D(this NurbsCurvePointList list, double scaleFactor)
        {
            var count = list.Count;
            G.D2.PointList points = new G.D2.PointList();
            foreach (ControlPoint p in list)
            {
                var location = p.Location;
                var pt = new TK.G.D2.Point(location.X * scaleFactor, location.Y * scaleFactor);
                points.Add(pt);
            }
            return points;
        }
        public static G.D2.Extent DomainToTopSolidExtent(this Rhino.Geometry.Surface surface)
        {
            G.D1.Extent extentX = new G.D1.Extent(surface.Domain(0).T0, surface.Domain(0).T1);
            G.D1.Extent extentY = new G.D1.Extent(surface.Domain(1).T0, surface.Domain(1).T1);

            return new G.D2.Extent(extentX, extentY);
        }



        #endregion


        #region Units Conversion
        private static G.D3.Transform transform;
        static G.D3.Transform Transform
        {
            get
            {

                return G.D3.Transform.Identity;

            }
            set
            {
                transform = value;
            }

        }


        static SimpleUnit UnitConversion(Rhino.UnitSystem inUnit)
        {
            SimpleUnit outUnit = LengthUnits.Meter;

            switch (inUnit)
            {
                case UnitSystem.Angstroms:
                    outUnit = LengthUnits.Angstrom;
                    break;

                case UnitSystem.Nanometers:
                    outUnit = LengthUnits.Nanometer;
                    break;

                case UnitSystem.Microns:
                    outUnit = LengthUnits.Micrometer;
                    break;

                case UnitSystem.Millimeters:
                    outUnit = LengthUnits.Millimeter;
                    break;

                case UnitSystem.Centimeters:
                    outUnit = LengthUnits.Centimeter;
                    break;

                case UnitSystem.Decimeters:
                    outUnit = LengthUnits.Decimeter;
                    break;

                case UnitSystem.Kilometers:
                    outUnit = LengthUnits.Kilometer;
                    break;

                case UnitSystem.Microinches:
                    outUnit = LengthUnits.Microinch;
                    break;

                case UnitSystem.Mils:
                    outUnit = LengthUnits.Mil;
                    break;

                case UnitSystem.Inches:
                    outUnit = LengthUnits.Inch;
                    break;

                case UnitSystem.Feet:
                    outUnit = LengthUnits.Foot;
                    break;

                case UnitSystem.Yards:
                    outUnit = LengthUnits.Yard;
                    break;

                case UnitSystem.Miles:
                    outUnit = LengthUnits.Mile;
                    break;

                case UnitSystem.PrinterPoints:
                    outUnit = LengthUnits.Point;
                    break;

                case UnitSystem.Meters:
                default:
                    break;
            }

            return outUnit;
        }

        static Rhino.UnitSystem UnitConversion(SimpleUnit inUnit)
        {
            if (inUnit == LengthUnits.Angstrom)
                return Rhino.UnitSystem.Angstroms;
            else if (inUnit == LengthUnits.Nanometer)
                return Rhino.UnitSystem.Nanometers;
            else if (inUnit == LengthUnits.Micrometer)
                return Rhino.UnitSystem.Microns;
            else if (inUnit == LengthUnits.Millimeter)
                return Rhino.UnitSystem.Millimeters;
            else if (inUnit == LengthUnits.Centimeter)
                return Rhino.UnitSystem.Centimeters;
            else if (inUnit == LengthUnits.Decimeter)
                return Rhino.UnitSystem.Decimeters;
            else if (inUnit == LengthUnits.Kilometer)
                return Rhino.UnitSystem.Kilometers;
            else if (inUnit == LengthUnits.Microinch)
                return Rhino.UnitSystem.Microinches;
            else if (inUnit == LengthUnits.Mil)
                return Rhino.UnitSystem.Mils;
            else if (inUnit == LengthUnits.Inch)
                return Rhino.UnitSystem.Inches;
            else if (inUnit == LengthUnits.Foot)
                return Rhino.UnitSystem.Feet;
            else if (inUnit == LengthUnits.Yard)
                return Rhino.UnitSystem.Yards;
            else if (inUnit == LengthUnits.Mile)
                return Rhino.UnitSystem.Miles;
            else if (inUnit == LengthUnits.Point)
                return Rhino.UnitSystem.PrinterPoints;
            else
                return Rhino.UnitSystem.Meters; // Default case
        }



        #endregion
    }
}
