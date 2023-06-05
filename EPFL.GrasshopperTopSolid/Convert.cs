using NLog.Fluent;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using System.Collections.Generic;
using System.Linq;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.D3.Sketches;
using TopSolid.Kernel.DB.D2.Sketches;

using G = TopSolid.Kernel.G;
using TopSolid.Kernel.G.D1;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.G.D3.Curves;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.G.D3.Shapes.Creations;
using TopSolid.Kernel.G.D3.Shapes.Modifications;
using TopSolid.Kernel.G.D3.Shapes.Sew;
using TopSolid.Kernel.G.D3.Shapes.Sketches;
using TopSolid.Kernel.G.D3.Sketches;
using TopSolid.Kernel.G.D3.Surfaces;
using TopSolid.Kernel.SX;
using TopSolid.Kernel.SX.Collections;
using TopSolid.Kernel.TX.Items;
using TKG = TopSolid.Kernel.G;
using TKGD2 = TopSolid.Kernel.G.D2;
using TKGD3 = TopSolid.Kernel.G.D3;
using TSXGen = TopSolid.Kernel.SX.Collections.Generic;
using SketchEntity = TopSolid.Kernel.DB.D2.Sketches.SketchEntity;
using TopSolid.Kernel.G.D3.Shapes.Healing;
using TopSolid.Kernel.DB.Operations;
using TopSolid.Kernel.DB.D3.Sketches.Operations;
using TopSolid.Kernel.DB.D3.Curves;
using TopSolid.Kernel.TX.Undo;
using TX = TopSolid.Kernel.TX;
using SX = TopSolid.Kernel.SX;
using TUI = TopSolid.Kernel.UI;
using TopSolid.Kernel.SX.Drawing;
using Console = System.Console;
using TopSolid.Kernel.G;
using System;
using TopSolid.Kernel.WX;
using System.Diagnostics;
using TopSolid.Kernel.SX.Collections.Generic;
using Rhino.PlugIns;

namespace EPFL.GrasshopperTopSolid
{
    public static class Convert
    {
        static public double topSolidLinear = TopSolid.Kernel.G.Precision.ModelingLinearTolerance;
        static public double topSolidAngular = TopSolid.Kernel.G.Precision.AngularPrecision;


        #region Point
        static public TKG.D3.Point ToHost(this Point3d p)
        {
            return new TKG.D3.Point(p.X, p.Y, p.Z);
        }

        static public TKG.D2.Point ToHost2d(this Point3d p)
        {
            return new TKG.D2.Point(p.X, p.Y);
        }
        static public TKG.D3.Point ToHost(this Rhino.Geometry.Point p)
        {
            Point3d pt = p.Location;
            return new TKG.D3.Point(pt.X, pt.Y, pt.Z);
        }
        static public TKG.D2.Point ToHost2d(this Rhino.Geometry.Point p)
        {
            Point3d pt = p.Location;
            return new TKG.D2.Point(pt.X, pt.Y);
        }
        static public Point3d ToRhino(this TKG.D3.Point p)
        {
            return new Point3d(p.X, p.Y, p.Z);
        }

        static public Point2d ToRhino(this TKG.D2.Point p)
        {
            return new Point2d(p.X, p.Y);
        }



        #endregion
        #region Vector
        static public TKG.D3.Vector ToHost(this Vector3d v)
        {
            return new TKG.D3.Vector(v.X, v.Y, v.Z);
        }
        static public Vector3d ToRhino(this TKG.D3.Vector v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }

        public static Rhino.Geometry.Vector3d ToRhino(this TKGD3.Axis axis)
        {
            return new Vector3d(axis.Vx.X, axis.Vx.Y, axis.Vx.Z);
        }

        static public Vector3d ToRhino(this TKG.D3.UnitVector v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }


        #endregion
        #region Line
        static public TKG.D3.Curves.LineCurve ToHost(this Rhino.Geometry.Line l)
        {
            return new TKG.D3.Curves.LineCurve(l.From.ToHost(), l.To.ToHost());
        }

        static public Rhino.Geometry.LineCurve ToRhino(this TKG.D3.Curves.LineCurve l)
        {
            if (l.Range.IsInfinite)
                return new Rhino.Geometry.LineCurve(l.Axis.Po.ToRhino(), new Rhino.Geometry.Point3d(
                    l.Axis.Po.X + l.Axis.Vx.X,
                    l.Axis.Po.Y + l.Axis.Vx.Y,
                    l.Axis.Po.Z + l.Axis.Vx.Z));

            return new Rhino.Geometry.LineCurve(l.Ps.ToRhino(), l.Pe.ToRhino());
        }

        static public Rhino.Geometry.NurbsCurve ToRhino(this TKGD3.Curves.EllipseCurve ellipseCurve)
        {
            var ellipse = new Rhino.Geometry.Ellipse(ellipseCurve.Plane.ToRhino(), ellipseCurve.RadiusX, ellipseCurve.RadiusY);
            if (!ellipseCurve.IsClosed())
            {
                return ellipseCurve.GetBSplineCurve(false, false).ToRhino();
            }
            return NurbsCurve.CreateFromEllipse(ellipse);

        }

        static public Rhino.Geometry.LineCurve ToRhino(this TKG.D2.Curves.LineCurve l)
        {
            var line = new Rhino.Geometry.Line(new Point3d(l.Ps.X, l.Ps.Y, 0), new Point3d(l.Pe.X, l.Pe.Y, 0));
            return new Rhino.Geometry.LineCurve(line);
        }



        //2D
        static public TKG.D2.Curves.LineCurve ToHost2d(this Rhino.Geometry.LineCurve lineCurve)
        {
            return new TKG.D2.Curves.LineCurve(lineCurve.PointAtStart.ToHost2d(), lineCurve.PointAtEnd.ToHost2d());
        }

        #endregion

        #region Plane
        static public TKG.D3.Plane ToHost(this Rhino.Geometry.Plane plane)
        {
            var vx = plane.XAxis;
            vx.Unitize();
            var y = plane.YAxis;
            y.Unitize();
            return new TKG.D3.Plane(plane.Origin.ToHost(), new UnitVector(vx.ToHost()), new UnitVector(y.ToHost()));
        }

        static public TKG.D3.Frame ToHost(this Rhino.Geometry.Plane p, Vector xVec, Vector yVec, Vector zVec)
        {
            return new TKG.D3.Frame(p.Origin.ToHost(), new UnitVector(xVec), new UnitVector(yVec), new UnitVector(zVec));
        }

        //2D
        static public TKG.D2.Frame ToHost2d(this Rhino.Geometry.Plane plane)
        {
            Rhino.Geometry.Vector3d x = plane.XAxis;
            x.Unitize();
            Rhino.Geometry.Vector3d y = plane.YAxis;
            y.Unitize();
            TKG.D2.UnitVector vx = new TKGD2.UnitVector(x.X, x.Y);
            TKG.D2.UnitVector vy = new TKGD2.UnitVector(x.X, x.Y);
            return new TKG.D2.Frame(plane.Origin.ToHost2d(), vx, vy);
        }

        static public Rhino.Geometry.Plane ToRhino(this TKG.D3.Frame p)
        {
            return new Rhino.Geometry.Plane(p.Po.ToRhino(), new Vector3d(p.Ax.Vx.X, p.Ax.Vx.Y, p.Ax.Vx.Z));
        }

        static public Rhino.Geometry.Plane ToRhino(this TKG.D2.Frame p)
        {
            Point3d origin = new Point3d(p.Po.X, p.Po.Y, 0);
            return new Rhino.Geometry.Plane(origin, new Vector3d(p.Vx.X, p.Vx.Y, 0), new Vector3d(p.Vy.X, p.Vy.Y, 0));

        }
        static public Rhino.Geometry.Plane ToRhino(this TKG.D3.Plane p)
        {
            return new Rhino.Geometry.Plane(p.Po.ToRhino(), new Vector3d(p.Ax.Vx.X, p.Ax.Vx.Y, p.Ax.Vx.Z), new Vector3d(p.Ay.Vx.X, p.Ay.Vx.Y, p.Ay.Vx.Z));
            //return new Rhino.Geometry.Plane(p.Po.ToRhino(), new Vector3d(p.Ax.Vx.X, p.Ax.Vx.Y, p.Ax.Vx.Z));
        }
        #endregion

        #region Polyline
        static public TKG.D3.Curves.PolylineCurve ToHost(this Rhino.Geometry.Polyline p)
        {
            var pointList = new PointList(p.Count);
            foreach (var pt in p)
            {
                pointList.Add(pt.ToHost());
            }

            return new TKG.D3.Curves.PolylineCurve(p.IsClosed, pointList);
        }



        static public TKG.D2.Curves.PolylineCurve ToHost2d(this Rhino.Geometry.Polyline p)
        {
            var pointList = new TopSolid.Kernel.G.D2.PointList(p.Count);
            foreach (var pt in p)
            {
                pointList.Add(pt.ToHost2d());
            }

            return new TKG.D2.Curves.PolylineCurve(p.IsClosed, pointList);
        }
        #endregion
        #region Curve
        static public Rhino.Geometry.Curve ToRhino(this TKGD3.Curves.Curve curve)
        {
            if (curve is CircleCurve tscircle)
                return tscircle.ToRhino();

            if (curve is EllipseCurve tsEllipse)
                return tsEllipse.ToRhino();

            if (curve is TKGD3.Curves.LineCurve tsline)
                return tsline.ToRhino();
            if (curve is TKGD3.Curves.BSplineCurve bsCurve)
                return bsCurve.ToRhino();

            return curve.GetBSplineCurve(false, false).ToRhino();
        }

        static public Rhino.Geometry.Curve ToRhino(this TKGD2.Curves.Curve curve)
        {
            if (curve is TKGD2.Curves.LineCurve tsline)
                return tsline.ToRhino();

            if (curve is TKGD2.Curves.CircleCurve tsCircle)
                return tsCircle.ToRhino();

            if (curve is TKGD2.Curves.EllipseCurve tsEllipse)
                return new Rhino.Geometry.Ellipse(tsEllipse.Frame.ToRhino(), tsEllipse.RadiusX, tsEllipse.RadiusY).ToNurbsCurve();

            if (curve is TKGD2.Curves.BSplineCurve bsCurve)
                return bsCurve.ToRhino();

            return curve.GetBSplineCurve(false, false).ToRhino();
        }

        static public Rhino.Geometry.ArcCurve ToRhino(this TKGD3.Curves.CircleCurve circleCurve)
        {
            if (circleCurve.IsClosed())
            {
                Circle circle = new Circle(circleCurve.Plane.ToRhino(), circleCurve.Radius);
                return new ArcCurve(circle);
            }

            Arc arc = new Arc(circleCurve.Ps.ToRhino(), circleCurve.Pm.ToRhino(), circleCurve.Pe.ToRhino());
            return new ArcCurve(arc);
        }

        static public Rhino.Geometry.ArcCurve ToRhino(this TKGD2.Curves.CircleCurve circleCurve)
        {
            if (circleCurve.IsClosed())
            {
                Circle circle = new Circle(circleCurve.Frame.ToRhino(), circleCurve.Radius);
                return new ArcCurve(circle);
            }

            Rhino.Geometry.Point3d Ps, Pm, Pe;
            Ps = new Rhino.Geometry.Point3d(circleCurve.Ps.X, circleCurve.Ps.Y, 0);
            Pm = new Rhino.Geometry.Point3d(circleCurve.Pm.X, circleCurve.Pm.Y, 0);
            Pe = new Rhino.Geometry.Point3d(circleCurve.Pe.X, circleCurve.Pe.Y, 0);
            Arc arc = new Arc(Ps, Pm, Pe);

            //Arc arc = new Arc(circleCurve.Ps.), circleCurve.Pm.ToRhino(), circleCurve.Pe.ToRhino());
            return new ArcCurve(arc);
        }

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
            TKGD3.Point Ps, Pm, Pe;
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
            return new TKGD3.Curves.LineCurve(lineCurve.PointAtStart.ToHost(), lineCurve.PointAtEnd.ToHost());
        }

        static public TopSolid.Kernel.G.D3.Curves.PolylineCurve ToHost(this Rhino.Geometry.PolylineCurve polylineCurve)
        {
            Polyline polyline = polylineCurve.ToPolyline();
            PointList pointList = new PointList(polyline.Count);
            var pts = polyline.Select(x => x.ToHost());
            foreach (TKGD3.Point point in pts)
            {
                pointList.Add(point);
            }
            return new TKGD3.Curves.PolylineCurve(polyline.IsClosed, pointList);
        }

        static public TKGD3.Curves.Curve ToHost(this Rhino.Geometry.PolyCurve polyCurve)
        {
            return polyCurve.ToNurbsCurve().ToHost();
        }



        static public TopSolid.Kernel.G.D3.Curves.BSplineCurve ToHost(this Rhino.Geometry.NurbsCurve nurbsCurve)
        {
            bool isRational = nurbsCurve.IsRational;
            bool isPeriodic = nurbsCurve.IsPeriodic;
            int degree = nurbsCurve.Degree;
            DoubleList knotList = ToDoubleList(nurbsCurve.Knots);
            PointList pts = ToPointList(nurbsCurve.Points);
            DoubleList weightList = ToDoubleList(nurbsCurve.Points);
            BSpline bspline = new BSpline(isPeriodic, degree, knotList);
            if (isRational)
            {
                //var w = c.Points.ConvertAll(x => x.Weight);
                BSplineCurve bsplineCurve = new BSplineCurve(bspline, pts, weightList);
                return bsplineCurve;
            }
            else
            {
                BSplineCurve bsplineCurve = new BSplineCurve(bspline, pts);
                return bsplineCurve;
            }
        }

        static public TopSolid.Kernel.G.D2.Curves.Curve ToHost2d(this Rhino.Geometry.Curve rhinoCurve)
        {
            TKGD3.Curves.Curve topSolidCurve;

            if (rhinoCurve is Rhino.Geometry.LineCurve line)
            {
                return line.ToHost2d();
            }

            if (rhinoCurve is ArcCurve arcCurve)
            {
                Arc arc = arcCurve.Arc;
                TKGD2.Frame plane2d = arc.Plane.ToHost2d();
                TKGD2.Curves.CircleCurve circleCurve = new TKGD2.Curves.CircleCurve(plane2d, arc.Radius);
                TKGD2.Curves.CircleMaker maker = new TKGD2.Curves.CircleMaker(SX.Version.Current, topSolidLinear, topSolidAngular);
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


        static public TopSolid.Kernel.G.D2.Curves.BSplineCurve ToHost2d(this Rhino.Geometry.NurbsCurve nurbsCurve, TKGD3.Surfaces.Surface surface = null)
        {
            bool isRational = nurbsCurve.IsRational;
            bool isPeriodic = nurbsCurve.IsPeriodic;
            int degree = nurbsCurve.Degree;
            DoubleList knotList = ToDoubleList(nurbsCurve.Knots);
            TKGD2.PointList pts = ToPointList2D(nurbsCurve.Points);
            DoubleList weightList = ToDoubleList(nurbsCurve.Points);
            BSpline b = new BSpline(isPeriodic, degree, knotList);
            if (isRational)
            {
                //var w = c.Points.ConvertAll(x => x.Weight);
                TKGD2.Curves.BSplineCurve bsplineCurve = new TKGD2.Curves.BSplineCurve(b, pts, weightList);
                return bsplineCurve;
            }
            else
            {
                TKGD2.Curves.BSplineCurve bsplineCurve = new TKGD2.Curves.BSplineCurve(b, pts);
                return bsplineCurve;
            }

        }

        /// <summary>
        /// Converts a single segment of a TopSolid Profile to a Rhino NurbsCurve
        /// </summary>
        /// <param name="curve">BSplineCurve to convert</param>
        /// <returns></returns>
        static public Rhino.Geometry.NurbsCurve ToRhino(this BSplineCurve curve)
        {
            Rhino.Collections.Point3dList Cpts = new Rhino.Collections.Point3dList();

            curve = curve.GetBSplineCurve(false, false);


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

            if (!nurbsCurve.IsValid)
            {
                string log = "";
                nurbsCurve.IsValidWithLog(out log);
                System.Console.WriteLine(log);
            }

            //bool coincide = curve.IsClosed() && !curve.CPts.ExtractFirst().CoincidesWith(curve.CPts.ExtractLast());

            //if (curve.IsClosed() && !curve.CPts.ExtractFirst().CoincidesWith(curve.CPts.ExtractLast()))
            //{
            //    //nurbsCurve.Points.Append(curve.Ps.ToRhino());
            //    //nurbsCurve.MakeClosed(TKG.Precision.LinearPrecision);
            //}

            //bool closed = false;
            //if (curve.IsPeriodic)
            //    closed = nurbsCurve.MakeClosed(TKG.Precision.LinearPrecision);

            return nurbsCurve;

        }

        static public Rhino.Geometry.Curve ToRhino(this TKGD3.Curves.IGeometricProfile profile)
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

        static public Rhino.Geometry.Curve ToRhino(this TKGD2.Curves.IGeometricProfile profile)
        {
            if (profile.SegmentCount == 1)
                return profile.Segments.First().Curve.ToRhino();

            PolyCurve rhCurve = new PolyCurve();
            foreach (TKGD2.Curves.IGeometricSegment seg in profile.Segments)
            {
                rhCurve.AppendSegment(seg.GetOrientedCurve().Curve.GetBSplineCurve(false, false).ToRhino());
            }

            return rhCurve.ToNurbsCurve();
        }



        static public Rhino.Geometry.NurbsCurve ToRhino(this TKGD2.Curves.BSplineCurve curve)
        {
            Rhino.Collections.Point3dList Cpts = new Rhino.Collections.Point3dList();
            Rhino.Geometry.NurbsCurve rhCurve = null;
            double tol_TS = TopSolid.Kernel.G.Precision.ModelingLinearTolerance;
            bool isrational = curve.IsRational;


            foreach (TopSolid.Kernel.G.D2.Point P in curve.CPts)
            {
                Cpts.Add(P.X, P.Y, 0);
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

        static public Rhino.Geometry.PolyCurve ToRhino(this TKGD3.Sketches.Profile profile)
        {
            Rhino.Geometry.PolyCurve rhCurve = new PolyCurve();

            for (int i = 0; i < (profile.Segments.Count()); i++)
            {
                rhCurve.Append(profile.Segments.ElementAt(i).Geometry.ToRhino());
            }

            return rhCurve;
        }

        static public Rhino.Geometry.PolyCurve ToRhino(this TKGD2.Sketches.Profile profile)
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



        #region Surface

        public static TKG.D3.Surfaces.Surface ToHost(this Rhino.Geometry.Surface rhinoSurface)
        {
            if (rhinoSurface is Rhino.Geometry.PlaneSurface planeSurface)
                return planeSurface.ToHost();

            if (rhinoSurface is Extrusion extrusionSurface && extrusionSurface.ProfileCount == 1)
                return extrusionSurface.ToHost();

            if (rhinoSurface is SumSurface sumSurface)
                return sumSurface.ToHost();

            if (rhinoSurface is Rhino.Geometry.RevSurface rhinoRevSurface)
                return rhinoRevSurface.ToHost();

            if (rhinoSurface is NurbsSurface nurbsSurface)
                return nurbsSurface.ToHost();

            return rhinoSurface.ToNurbsSurface().ToHost();

        }
        public static TKG.D3.Surfaces.RevolvedSurface ToHost(this Rhino.Geometry.RevSurface rhinorevSurface)
        {
            TKGD2.Extent extent = new TKGD2.Extent(rhinorevSurface.Domain(0).ToHost(), rhinorevSurface.Domain(1).ToHost());
            RevolvedSurface revolvedSurface = new RevolvedSurface(rhinorevSurface.Curve.ToHost(), rhinorevSurface.Axis.LineToAxis(), extent);
            return revolvedSurface;
        }
        public static TKGD3.Surfaces.PlaneSurface ToHost(this Rhino.Geometry.PlaneSurface rhinoPlaneSurf)
        {
            Rhino.Geometry.Plane plane = Rhino.Geometry.Plane.WorldXY;
            bool success = rhinoPlaneSurf.TryGetPlane(out plane);
            return new TKGD3.Surfaces.PlaneSurface(plane.ToHost(), rhinoPlaneSurf.DomainToTopSolidExtent());
        }
        public static TKG.D3.Surfaces.ExtrudedSurface ToHost(this Rhino.Geometry.Extrusion rhinoExtrusion)
        {
            TKGD3.Curves.Curve profile = rhinoExtrusion.Profile3d(0, 0).ToHost();
            Rhino.Geometry.LineCurve path = rhinoExtrusion.PathLineCurve();
            G.D3.Axis direction = new Axis(path.PointAtStart.ToHost(), path.PointAtEnd.ToHost());
            return new ExtrudedSurface(profile, direction.Vx, rhinoExtrusion.DomainToTopSolidExtent());
        }

        public static TKG.D3.Surfaces.BSplineSurface ToHost(this NurbsSurface nurbsSurface)
        {
            bool isRational = nurbsSurface.IsRational;
            bool isPeriodicU = nurbsSurface.IsPeriodic(0);
            bool isPeriodicV = nurbsSurface.IsPeriodic(1);
            var degreeU = nurbsSurface.Degree(0);
            var degreeV = nurbsSurface.Degree(1);
            var knotsU = ToDoubleList(nurbsSurface.KnotsU);
            var knotsV = ToDoubleList(nurbsSurface.KnotsV);
            var controlPoints = ToPointList(nurbsSurface.Points);
            var weightDoubleList = ToDoubleList(nurbsSurface.Points);

            BSpline bsplineU = new BSpline(isPeriodicU, degreeU, knotsU);
            BSpline bsplineV = new BSpline(isPeriodicV, degreeV, knotsV);

            if (isRational)
                return new BSplineSurface(bsplineU, bsplineV, controlPoints, weightDoubleList);

            return new BSplineSurface(bsplineU, bsplineV, controlPoints);
        }



        public static Rhino.Geometry.Surface ToRhino(this IParametricSurface topSolidSurface)
        {
            //Rhino.Geometry.Surface surf = null;
            if (topSolidSurface is TKGD3.Surfaces.PlaneSurface planarsurf)
                return new Rhino.Geometry.PlaneSurface(planarsurf.Plane.ToRhino(), planarsurf.Range.XExtent.ToRhino(), planarsurf.Range.YExtent.ToRhino());


            if (topSolidSurface is RevolvedSurface revSurf)
            {
                if (revSurf.Curve.IsLinear())
                {
                    TKGD3.Curves.LineCurve line = (TKGD3.Curves.LineCurve)revSurf.Curve;
                    return RevSurface.Create(line.ToRhino().Line, new Line(revSurf.Axis.Po.ToRhino(), revSurf.Axis.Vx.ToRhino()));
                }
                return RevSurface.Create(revSurf.Curve.ToRhino(), new Line(revSurf.Axis.Po.ToRhino(), revSurf.Axis.Vx.ToRhino()));
            }

            if (topSolidSurface is ConeSurface coneSurface)
                return coneSurface.ToRhino();

            if (topSolidSurface is BSplineSurface bsplineSurface)
                return bsplineSurface.ToRhino();

            if (topSolidSurface is ExtrudedSurface extrudedsurface)
                return extrudedsurface.ToRhino();


            TKGD3.Surfaces.Surface surface = topSolidSurface as TKGD3.Surfaces.Surface;


            return surface.GetBsplineGeometry(TKG.Precision.LinearPrecision, false, false, false).ToRhino();

        }

        public static Rhino.Geometry.Surface ToRhino(this ConeSurface coneSurface)

        {
            var vector = new Vector(coneSurface.GetApex(), coneSurface.Frame.Po).ToRhino();
            var plane = new Rhino.Geometry.Plane(coneSurface.GetApex().ToRhino(), vector);
            var height = coneSurface.Frame.Po.GetDistance(coneSurface.GetApex());
            Rhino.Geometry.Cone cone = new Cone(plane, height, coneSurface.Radius);

            return NurbsSurface.CreateFromCone(cone);
        }


        public static Rhino.Geometry.Surface ToRhino(this TKGD3.Surfaces.ExtrudedSurface extrudedSurface)
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
                    control_points[u, v] = new Point3d(bsplineSurface.GetCPt(u, v).X, bsplineSurface.GetCPt(u, v).Y, bsplineSurface.GetCPt(u, v).Z);
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

            bool knotU = uDegree + uCount - 1 == bsplineSurface.UBs.Count - 2;
            bool knotV = vDegree + vCount - 1 == bsplineSurface.VBs.Count - 2;

            //add the knots + Adjusting to Rhino removing the 2 extra knots (Superfluous)
            for (int u = 1; u < (bsplineSurface.UBs.Count - 1); u++)
                rhinoSurface.KnotsU[u - 1] = bsplineSurface.UBs[u];
            for (int v = 1; v < (bsplineSurface.VBs.Count - 1); v++)
                rhinoSurface.KnotsV[v - 1] = bsplineSurface.VBs[v];

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
            rhinoSurface.IsValidWithLog(out log);
            //if (!rhinoSurface.IsValid )
            //{
            //    if (!knotU)
            //    {
            //        for (int u = 0; u < bsplineSurface.UBs.Count; u++)
            //            rhinoSurface.KnotsU[u] = bsplineSurface.UBs[u];
            //    } 
            //    if (!knotV)
            //    {
            //        for (int v = 0; v < bsplineSurface.VBs.Count; v++)
            //            rhinoSurface.KnotsV[v] = bsplineSurface.VBs[v];
            //    }
            //}
            //rhinoSurface.IsValidWithLog(out log);
            return rhinoSurface;
        }


        public static DoubleList ToDoubleList(NurbsSurfaceKnotList nurbsKnotList)
        {
            int count = nurbsKnotList.Count;
            DoubleList knotDoubleList = new DoubleList(count + 2);
            knotDoubleList.Add(nurbsKnotList[0]);
            foreach (var knot in nurbsKnotList)
            {
                knotDoubleList.Add(knot);
            }
            knotDoubleList.Add(knotDoubleList.Last());
            return knotDoubleList;
        }

        public static PointList ToPointList(NurbsSurfacePointList list)
        {
            var count = list.CountU * list.CountV;
            var points = new PointList(count);

            foreach (ControlPoint p in list)
            {
                var location = p.Location;
                var pt = new TKG.D3.Point(location.X, location.Y, location.Z);
                points.Add(pt);
            }

            return points;
        }
        static DoubleList ToDoubleList(NurbsSurfacePointList list)
        {
            var count = list.CountU * list.CountV;
            DoubleList w = new DoubleList(count);
            foreach (ControlPoint p in list)
            {
                var weight = p.Weight;
                w.Add(weight);
            }
            return w;
        }
        #endregion
        #region Breps


        static public Brep[] ToRhino(this Shape shape)
        {
            System.Collections.Generic.List<Brep> listofBrepsrf = new System.Collections.Generic.List<Brep>();
            Brep brep = new Brep();

            foreach (Face f in shape.Faces)
            {
                brep = FaceToBrep(f);
                //if (!brep.IsValid)
                //{
                //    brep = new Brep();
                //}
                listofBrepsrf.Add(brep);
            }

            var result = Brep.JoinBreps(listofBrepsrf, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            foreach (Brep b in result)
            {
                b.Trims.MatchEnds();
                b.Repair(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            }

            for (int i = 0; i < result.Length; i++)
            {
                result[i].Repair(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            }

            return result;

        }

        static private Brep FaceToBrep(Face face)
        {
            //Create the *out* variables
            BoolList outer = new BoolList();
            TSXGen.List<TKGD2.Curves.IGeometricProfile> list2D = new TSXGen.List<TKGD2.Curves.IGeometricProfile>();
            TSXGen.List<TKGD3.Curves.IGeometricProfile> list3D = new TSXGen.List<TKGD3.Curves.IGeometricProfile>();
            TSXGen.List<EdgeList> listEdges = new TSXGen.List<EdgeList>();
            TSXGen.List<TKGD3.Shapes.Vertex> vertexlist = new TSXGen.List<TKGD3.Shapes.Vertex>();
            double tol_Rh = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;



            double tol_TS = TopSolid.Kernel.G.Precision.ModelingLinearTolerance;




            //Topology indexes ?
            int c_index = 0;


            //Create the Brep Surface
            Brep brepsrf = new Brep();

            int loopCount = face.LoopCount;
            //function added on request, gets the 2DCurves, 3dCurves and Edges in the correct order
            OrientedSurface osurf = face.GetOrientedBsplineTrimmedGeometry(tol_TS, false, false, false, outer, list2D, list3D, listEdges);
            TKGD3.Surfaces.Surface topSolidSurface = osurf.Surface;
            //OrientedSurface osurf2 = face.GetOrientedBsplineTrimmedGeometry(tol_TS, false, false, false, FaceTrimmingLoopsConfine.No, outer, list2D, list3D, listEdges);

            var list2Dflat = list2D.SelectMany(f => f.Segments).ToList();
            var list3Dflat = list3D.SelectMany(f => f.Segments).ToList();
            var listEdgesflat = listEdges.SelectMany(m => m).ToList();


            //for debug
            var verticesPoints = face.Vertices.Select(v => v.GetGeometry());
            var revEdges = listEdgesflat.Select(e => Tuple.Create(e.StartVertex.GetGeometry(), e.EndVertex.GetGeometry(), e.IsReversedWithFin(face)));
            var revCruve3D = list3Dflat.Select(c => Tuple.Create(c.Ps, c.Pe, c.IsReversed, c.GetOrientedCurve().IsReversed));
            var revCruve2D = list2Dflat.Select(d => Tuple.Create(d.Ps, d.Pe, d.IsReversed, d.GetOrientedCurve().IsReversed));

            //For Debug
            bool correct = CheckTopologicalCoherence(list2D, list3D, listEdges, loopCount);
            var geotype = face.GeometryType;

            if (!correct)
            {
                //osurf =  face.GetOrientedBsplineTrimmedGeometry(tol_TS, false, false, false, FaceTrimmingLoopsConfine.Periph, outer, list2D, list3D, listEdges);
                //  list2Dflat = list2D.SelectMany(f => f.Segments).ToList();
                //  list3Dflat = list3D.SelectMany(f => f.Segments).ToList();
                //  listEdgesflat = listEdges.SelectMany(m => m).ToList();
                if (face.GeometryType == SurfaceGeometryType.Cone)
                {
                    topSolidSurface = face.GetGeometry(true) as ConeSurface;
                }

                Console.WriteLine("Error");
            }

            //Add Vertices
            int ver = 0;
            foreach (EdgeList list in listEdges)
            {
                foreach (TKGD3.Shapes.Edge e in list)
                {
                    //TODO UNCOMMENT as soon as error with IsReversedwithFin is resolved
                    if (e.IsReversedWithFin(face))
                    {
                        vertexlist.Add(e.EndVertex);
                    }
                    else
                    {
                        vertexlist.Add(e.StartVertex);
                    }
                    ver++;
                }
            }
            foreach (TKG.D3.Shapes.Vertex v in vertexlist)
            {
                if (!v.IsEmpty) //sometimes we receive null vertices
                {
                    brepsrf.Vertices.Add(Convert.ToRhino(v.GetGeometry()), tol_TS);
                }
                else
                {
                    brepsrf.Vertices.Add();
                }
            }

            //Get the 3D Curves and convert them to Rhino            
            int ind = 0;
            int indperLoop = 0;
            //bool rev;
            foreach (TKGD3.Curves.IGeometricProfile c in list3D)
            {

                foreach (TKGD3.Curves.IGeometricSegment ic in c.Segments)
                {
                    var orientedcrv = ic.GetOrientedCurve();
                    var crv = orientedcrv.Curve.ToRhino();
                    //var crvi = ic.GetOrientedCurve().Curve;
                    //AHW commented to prevent error, check if it affects good conversion

                    if (orientedcrv.IsReversed)
                    {
                        crv.Reverse();
                    }

                    c_index = brepsrf.AddEdgeCurve(crv);
                    indperLoop++;
                }
                ind++;
                indperLoop = 0;
            }

            //Edges
            int i = 0;
            int iperLoop = 0;
            System.Collections.Generic.List<BrepEdge> rhEdges = new System.Collections.Generic.List<BrepEdge>();
            foreach (EdgeList list in listEdges)
            {
                foreach (Edge e in list)
                {
                    //Other possible method to add edges
                    //if (i + 1 == list.Count)
                    //{
                    //    if (!e.IsReversedWithFin())
                    //        edge.Add(brepsrf.Edges.Add(brepsrf.Vertices[0], brepsrf.Vertices[i], i, tol_TS));
                    //    else
                    //        edge.Add(brepsrf.Edges.Add(brepsrf.Vertices[i], brepsrf.Vertices[0], i, tol_TS));
                    //}
                    //else
                    //{
                    //    if (!e.IsReversedWithFin())
                    //        edge.Add(brepsrf.Edges.Add(brepsrf.Vertices[i + 1], brepsrf.Vertices[i], i, tol_TS));
                    //    else
                    //        edge.Add(brepsrf.Edges.Add(brepsrf.Vertices[i], brepsrf.Vertices[i + 1], i, tol_TS));
                    //}

                    if (iperLoop + 1 == list.Count)
                    {
                        if (e.VertexCount != 2)
                        {
                            rhEdges.Add(brepsrf.Edges.Add(i, i, i, tol_TS));
                        }
                        else
                        {
                            //TODO UNCOMMENT
                            if (e.IsReversedWithFin(face))
                                rhEdges.Add(brepsrf.Edges.Add(i - list.Count + 1, i, i, tol_TS));
                            else
                                rhEdges.Add(brepsrf.Edges.Add(i, i - list.Count + 1, i, tol_TS));
                        }
                    }
                    else
                    {
                        if (e.VertexCount != 2)
                        {
                            rhEdges.Add(brepsrf.Edges.Add(i, i, i, tol_TS));
                        }
                        // TODO UnComment
                        else
                        {
                            if (e.IsReversedWithFin(face))
                                rhEdges.Add(brepsrf.Edges.Add(i + 1, i, i, tol_TS));
                            else
                                rhEdges.Add(brepsrf.Edges.Add(i, i + 1, i, tol_TS));
                        }
                    }
                    i++;
                    iperLoop++;
                }
                iperLoop = 0;

            }



            int loopindex = 0;
            System.Collections.Generic.List<BrepTrim> rhTrim = new System.Collections.Generic.List<BrepTrim>();
            brepsrf.AddSurface(topSolidSurface.ToRhino());
            BrepFace bface = brepsrf.Faces.Add(0);
            BrepLoop rhinoLoop = null;

            //Get the 2D Curves and convert them to Rhino
            int x = 0;
            foreach (TKGD2.Curves.IGeometricProfile c in list2D)
            {
                var tsloop = face.Loops.ElementAt(loopindex);

                if (tsloop.IsOuter)
                    rhinoLoop = brepsrf.Loops.Add(BrepLoopType.Outer, bface);
                else
                    rhinoLoop = brepsrf.Loops.Add(BrepLoopType.Inner, bface);


                foreach (TKGD2.Curves.IGeometricSegment ic in c.Segments)
                {
                    Rhino.Geometry.Curve crv;
                    TKGD2.Curves.BSplineCurve tcrvv = ic.GetOrientedCurve().Curve.GetBSplineCurve(false, false);

                    if (tsloop.IsOuter)
                    {
                        if (ic.IsReversed)
                        {
                            tcrvv.Reverse();
                        }

                        crv = Convert.ToRhino(tcrvv);
                        x = brepsrf.AddTrimCurve(crv);

                        if (x < rhEdges.Count)
                            rhTrim.Add(brepsrf.Trims.Add(rhEdges[x], ic.IsReversed, rhinoLoop, x));
                        else
                        {
                            BrepEdge brepEdge = brepsrf.Edges.Add(x);
                            rhTrim.Add(brepsrf.Trims.Add(brepEdge, ic.IsReversed, rhinoLoop, x));
                        }
                    }

                    else
                    {
                        //AHW TODO unComment
                        if (ic.IsReversed)
                        {
                            tcrvv.Reverse();
                        }

                        crv = Convert.ToRhino(tcrvv);
                        x = brepsrf.AddTrimCurve(crv);
                        if (x < rhEdges.Count)
                            rhTrim.Add(brepsrf.Trims.Add(rhEdges[x], ic.IsReversed, rhinoLoop, x));
                        else
                        {
                            BrepEdge brepEdge = brepsrf.Edges.Add(x);
                            rhTrim.Add(brepsrf.Trims.Add(brepEdge, ic.IsReversed, rhinoLoop, x));
                        }

                    }

                    if (x < rhTrim.Count)
                    {
                        rhTrim[x].TrimType = BrepTrimType.Boundary;
                        rhTrim[x].IsoStatus = IsoStatus.None;
                        string log1 = null;
                        rhTrim[x].IsValidWithLog(out log1);
                    }
                    x++;
                }
                loopindex++;
            }


            if (osurf.IsReversed)
            {
                brepsrf.Faces.First().OrientationIsReversed = true;
            }

            string log = null;
            brepsrf.IsValidWithLog(out log);


            bool match = true;
            if (!brepsrf.IsValid)
            {
                brepsrf.Repair(tol_TS);
                brepsrf.IsValidWithLog(out log);
                match = brepsrf.Trims.MatchEnds();
                brepsrf.IsValidWithLog(out log);

            }

            brepsrf.SetTolerancesBoxesAndFlags(false, true, true, true, true, false, false, false);

            if (!match || !brepsrf.IsValid)
            {
                brepsrf.Repair(tol_TS);
                //brepsrf.IsValidWithLog(out log);
                match = brepsrf.Trims.MatchEnds();
            }
            return brepsrf;

        }


        static public ShapeList ToHost(this Brep brep, double tol = TopSolid.Kernel.G.Precision.ModelingLinearTolerance)
        {

            double tol_TS = tol;

            brep.Trims.MatchEnds();
            brep.Repair(tol_TS);
            Shape shape = null;
            ShapeList ioShapes = new ShapeList();
            //List<PositionedSketch> list3dSktech = new List<PositionedSketch>();
            //List<TKGD2.Sketches.PositionedSketch> list2dSketch = new List<TKGD2.Sketches.PositionedSketch>();

            if (brep.IsValid)
            {
                foreach (BrepFace bface in brep.Faces)
                {
                    shape = null;

                    //MakeSurfacesAndLoops(bface.ToBrep(), ioShapes, list3dSktech, list2dSketch);

                    shape = MakeSheetFrom3d(brep, bface, tol_TS);

                    if (shape == null || shape.IsEmpty)
                        shape = MakeSheetFrom2d(brep, bface, tol_TS);

                    if (shape == null || shape.IsEmpty)
                    {
                        shape = MakeSheet(brep, bface);
                        //inLog.Report("Face not limited.");
                        MessageBox.Show("Face not limited.");
                    }

                    if (shape == null || shape.IsEmpty)
                    { }//inLog.Report("Missing face.");
                    else
                        ioShapes.Add(shape);
                }
            }

            return ioShapes;
        }

        //for Debug and to prevent errors
        /// <summary>
        /// for debug, returns true if 3D segments = 2D segments = edges
        /// </summary>
        /// <param name="list2D">list of 2D profiles</param>
        /// <param name="list3D">list of 3D profiles</param>
        /// <param name="listEdges">list of EdgeList with loops</param>
        /// <param name="loopCount">Face.LoopCount</param>
        /// <returns></returns>
        static public bool CheckTopologicalCoherence(TSXGen.List<TKGD2.Curves.IGeometricProfile> list2D,
        TSXGen.List<TKGD3.Curves.IGeometricProfile> list3D,
        TSXGen.List<EdgeList> listEdges, int loopCount)
        {
            IEnumerable<TKGD2.Curves.GeometricSegment> list2dSegmentsFlattened = list2D.SelectMany(x => x.Segments);
            IEnumerable<TKGD3.Curves.IGeometricSegment> list3dSgementsFlattened = list3D.SelectMany(x => x.Segments);
            IEnumerable<Edge> listEdgesFlattened = listEdges.SelectMany(x => x);

            int count2dProfiles = list2D.Count;
            int count2dSegmentsFlattened = list2dSegmentsFlattened.Count();

            int count3dProfiles = list3D.Count;
            int count3dSegmentsFlattened = list3dSgementsFlattened.Count();

            int coundEdges = listEdges.Count;
            int countEdgesFlattened = listEdgesFlattened.Count();



            bool profilesEqualsLoopCount = count2dProfiles == count3dProfiles && count2dProfiles == loopCount && count3dProfiles == coundEdges;
            bool flattenedequals = count2dSegmentsFlattened == count3dSegmentsFlattened && count2dSegmentsFlattened == countEdgesFlattened;

            bool result = profilesEqualsLoopCount && flattenedequals;
            if (!result)
                Console.WriteLine("problem !!");
            return result;
        }


        internal static void MakeSurfacesAndLoops(Brep inBrep, ShapeList ioSurfs, TSXGen.List<TKG.D3.Sketches.PositionedSketch> ioLoops3d, TSXGen.List<TKG.D2.Sketches.PositionedSketch> ioLoops2d)
        {
            if (inBrep != null && ioSurfs != null && ioLoops3d != null && ioLoops2d != null)
            {
                SheetMaker sheetMaker = new SheetMaker(SX.Version.Current);

                foreach (BrepFace f in inBrep.Faces)
                {

                    //Brep face = f.ToBrep();
                    if (inBrep.IsValid)
                    {
                        sheetMaker.Surface = Convert.ToHost(f.DuplicateSurface());

                        Shape shape = null;
                        try
                        {
                            shape = sheetMaker.Make(null, null);
                        }
                        catch
                        {
                        }

                        if (shape != null && shape.IsEmpty == false)
                        {
                            int i;
                            TKGD3.Point po;
                            TKGD3.Vector vu, vv;
                            //Color[] colors = new Color[] { Color.White, Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Cyan, Color.Violet };
                            TKGD3.Curves.LineCurve l1;
                            TKGD3.Curves.LineCurve l2;
                            TKGD3.Sketches.Segment seg;

                            try
                            {
                                double u = sheetMaker.Surface.Us;
                                double v = sheetMaker.Surface.Vs;

                                if (!SX.Double.IsFinite(u))
                                    u = 0.0;

                                if (!SX.Double.IsFinite(v))
                                    v = 0.0;

                                po = sheetMaker.Surface.GetPoint(u, v);
                                vu = sheetMaker.Surface.GetDerivative(1, 0, u, v);
                                vv = sheetMaker.Surface.GetDerivative(0, 1, u, v);
                            }
                            catch
                            {
                                continue;
                            }

                            ioSurfs.Add(shape);

                            // Surface frame.

                            TKGD3.Sketches.PositionedSketch sketch = new TKGD3.Sketches.PositionedSketch(null, null, false);
                            sketch.SetManagementType(TKGD2.Sketches.SketchVertexManagementType.AllowsVertices, TKGD2.Sketches.SketchProfileManagementType.Manual);

                            if (vu.Norm > Precision.MinimalLength)
                            {
                                l1 = new TKGD3.Curves.LineCurve(po, po + vu);
                                seg = sketch.AddSegment(ItemOperationKey.PreviewKey, TKGD3.Sketches.Vertex.Empty, TKGD3.Sketches.Vertex.Empty, l1, false);
                                //seg.Color = Color.Red;
                            }

                            if (vv.Norm > Precision.MinimalLength)
                            {
                                l2 = new TKGD3.Curves.LineCurve(po, po + vv);
                                seg = sketch.AddSegment(ItemOperationKey.PreviewKey, TKGD3.Sketches.Vertex.Empty, TKGD3.Sketches.Vertex.Empty, l2, false);
                                //seg.Color = Color.Green;
                            }

                            ioLoops3d.Add(sketch);

                            // Face Loops.
                            int j = 0;
                            TSXGen.List<TKGD3.Curves.CurveList> loops3d = new TSXGen.List<CurveList>();

                            foreach (var crv in f.ToBrep().Edges)
                            {
                                loops3d[j].Add(Convert.ToHost(crv.EdgeCurve.ToNurbsCurve()));
                                j++;
                            }

                            if (loops3d != null)
                            {
                                foreach (TKGD3.Curves.CurveList cvs in loops3d)
                                {
                                    sketch = new TKGD3.Sketches.PositionedSketch(null, null, false);
                                    sketch.SetManagementType(TKGD2.Sketches.SketchVertexManagementType.AllowsVertices, TKGD2.Sketches.SketchProfileManagementType.Manual);

                                    i = 0;
                                    foreach (TKGD3.Curves.Curve cv in cvs)
                                    {
                                        sketch.AddSegment(ItemOperationKey.PreviewKey, TKGD3.Sketches.Vertex.Empty, TKGD3.Sketches.Vertex.Empty, cv, false);

                                        // deriv
                                        try
                                        {
                                            l1 = new TKGD3.Curves.LineCurve(cv.Ps, cv.Ps + cv.Vs);
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                        seg = sketch.AddSegment(ItemOperationKey.PreviewKey, TKGD3.Sketches.Vertex.Empty, TKGD3.Sketches.Vertex.Empty, l1, false);
                                        //seg.Color = colors[i++];

                                        //if (i == colors.Length)
                                        //    i = 1;
                                    }

                                    ioLoops3d.Add(sketch);
                                }
                            }


                            TSXGen.List<TKGD2.Curves.CurveList> loops2d = new TSXGen.List<TKGD2.Curves.CurveList>();
                            foreach (Rhino.Geometry.Curve crv in inBrep.Curves2D)
                            {
                                if (f.AdjacentEdges().Contains(crv.ComponentIndex().Index))
                                    loops2d.First().Add(Convert.ToHost2d(crv.ToNurbsCurve()));

                            }
                            if (loops2d != null)
                            {
                                foreach (TKGD2.Curves.CurveList cvs in loops2d)
                                {
                                    TKGD2.Sketches.PositionedSketch sketch2d = new TKGD2.Sketches.PositionedSketch(null, null, false);
                                    sketch2d.SetManagementType(TKGD2.Sketches.SketchVertexManagementType.AllowsVertices, TKGD2.Sketches.SketchProfileManagementType.Manual);

                                    i = 0;
                                    foreach (TKGD2.Curves.Curve cv in cvs)
                                    {
                                        TKGD2.Curves.LineCurve lin;
                                        sketch2d.AddSegment(ItemOperationKey.PreviewKey, TKGD2.Sketches.Vertex.Empty, TKGD2.Sketches.Vertex.Empty, cv, false);

                                        // deriv
                                        try
                                        {
                                            lin = new TKGD2.Curves.LineCurve(cv.Ps, cv.Ps + cv.Vs);
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                        TKGD2.Sketches.Segment seg2d = sketch2d.AddSegment(ItemOperationKey.PreviewKey, TKGD2.Sketches.Vertex.Empty, TKGD2.Sketches.Vertex.Empty, lin, false);
                                        //seg2d.Color = colors[i++];

                                        //if (i == colors.Length)
                                        //    i = 1;
                                    }

                                    ioLoops2d.Add(sketch2d);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static Shape MakeSheetFrom3d(Brep inBRep, BrepFace inFace, double inLinearPrecision)
        {
            Shape shape = null;

            TrimmedSheetMaker sheetMaker = new TrimmedSheetMaker(SX.Version.Current);
            sheetMaker.LinearTolerance = inLinearPrecision;
            sheetMaker.UsesBRepMethod = false;

            TX.Items.ItemMonikerKey key = new TX.Items.ItemMonikerKey(TX.Items.ItemOperationKey.BasicKey);

            // Get surface and set to maker.
            Rhino.Geometry.Surface surface = inBRep.Surfaces[inFace.FaceIndex];

            // Reverse surface and curves in 3d mode(according to the drilled cylinder crossed by cube in v5_example.3dm).
            //if (inFace.rev)
            //    surface = ImporterHelper.MakeReversed(surface); // Useless.

            // Closed BSpline surfaces must not be periodic for parasolid with 3d curves (according to wishbone.3dm and dinnermug.3dm).
            // If new problems come, see about the periodicity of the curves.

            //TODO check if planar to simplify
            TKGD3.Surfaces.Surface topSolidSurface = Convert.ToHost(surface);

            if (topSolidSurface != null && topSolidSurface is BSplineSurface bsplineSurface && (topSolidSurface.IsUPeriodic || topSolidSurface.IsVPeriodic))
            {
                topSolidSurface = (BSplineSurface)bsplineSurface.Clone();

                if (topSolidSurface.IsUPeriodic)
                    bsplineSurface.MakeUNonPeriodic();

                if (topSolidSurface.IsVPeriodic)
                    bsplineSurface.MakeVNonPeriodic();

                sheetMaker.Surface = new OrientedSurface(bsplineSurface, false);
            }

            else
            {
                sheetMaker.Surface = new OrientedSurface(topSolidSurface, false);
            }

            sheetMaker.SurfaceMoniker = new ItemMoniker(false, (byte)ItemType.ShapeFace, key, 1);

            // Get spatial curves and set to maker.
            TopSolid.Kernel.SX.Collections.Generic.List<TKGD3.Curves.CurveList> loops3d = new TSXGen.List<TKGD3.Curves.CurveList>();
            TopSolid.Kernel.SX.Collections.Generic.List<ItemMonikerList> listItemMok = new TSXGen.List<ItemMonikerList>();
            int i = 0;
            int loopIndex = 0;
            foreach (Rhino.Geometry.BrepLoop loop in inFace.Loops)
            {
                loops3d.Add(new TKGD3.Curves.CurveList());
                listItemMok.Add(new ItemMonikerList());
                foreach (var trim in loop.Trims)
                {
                    if (loops3d.Count < loopIndex - 1 || listItemMok.Count < loopIndex - 1) break;
                    loops3d[loopIndex].Add(trim.Edge.EdgeCurve.ToHost());
                    listItemMok[loopIndex].Add(new ItemMoniker(false, (byte)ItemType.SketchSegment, key, i++));
                }
                loopIndex++;
            }

            if (loops3d != null && loops3d.Count != 0)
            {
                // if (inFace.rev == false || ImporterHelper.MakeReversed(loops3d)) // Useless
                {
                    sheetMaker.SetCurves(loops3d, listItemMok);

                    bool valid = sheetMaker.IsValid;
                    //AHW setting to true causes an error
                    //sheetMaker.UsesBRepMethod = true;
                    //var x = new ItemMoniker(new CString($"S{op2.Id}"));
                    try
                    {
                        shape = sheetMaker.Make(null, ItemOperationKey.BasicKey);
                        //shape.CheckIsValidOccurrence();                        
                    }
                    catch (Exception e)

                    {
                        Console.WriteLine(e.Message + "\n" + e.Data.ToString());
                    }
                }
            }

            return shape;
        }

        private static Shape MakeSheetFrom2d(Brep inBRep, BrepFace inFace, double inLinearPrecision)
        {
            Shape shape = null;

            TrimmedSheetMaker sheetMaker = new TrimmedSheetMaker(SX.Version.Current);
            sheetMaker.LinearTolerance = inLinearPrecision;
            Rhino.Geometry.Surface surface = inFace.DuplicateSurface();

            // Closed BSpline surfaces must be made periodic for parasolid with 2d curves (according to torus and sphere in v5_example.3dm).
            // If new problems come, see about the periodicity of the curves.
            TKGD3.Surfaces.Surface topSolidSurface = Convert.ToHost(surface);
            if (topSolidSurface != null && topSolidSurface is BSplineSurface bsplineSurface && ((bsplineSurface.IsUClosed && bsplineSurface.IsUPeriodic == false) || (bsplineSurface.IsVClosed && bsplineSurface.IsVPeriodic == false)))
            {
                bsplineSurface = (BSplineSurface)bsplineSurface.Clone();

                if (bsplineSurface.IsUClosed)
                    bsplineSurface.MakeUPeriodic();

                if (bsplineSurface.IsVClosed)
                    bsplineSurface.MakeVPeriodic();

                //surface = bsSurface;

                OrientedSurface osurf = new OrientedSurface(bsplineSurface, inFace.OrientationIsReversed);
                sheetMaker.Surface = osurf;
            }
            else
            {
                OrientedSurface osurf = new OrientedSurface(topSolidSurface, inFace.OrientationIsReversed);
                sheetMaker.Surface = osurf;
            }

            TopSolid.Kernel.SX.Collections.Generic.List<TKGD2.Curves.CurveList> loops2d = new TSXGen.List<TKGD2.Curves.CurveList>();
            TopSolid.Kernel.SX.Collections.Generic.List<ItemMonikerList> listItemMok = new TSXGen.List<ItemMonikerList>();
            ItemMonikerKey key = new ItemMonikerKey(ItemOperationKey.BasicKey);
            int i = 0;
            int loopIndex = 0;
            foreach (Rhino.Geometry.BrepLoop loop in inFace.Loops)
            {
                loops2d.Add(new TKGD2.Curves.CurveList());
                listItemMok.Add(new ItemMonikerList());
                foreach (var trim in loop.Trims)
                {
                    if (loops2d.Count < loopIndex - 1 || listItemMok.Count < loopIndex - 1) break;
                    loops2d[loopIndex].Add(trim.TrimCurve.ToHost2d());
                    listItemMok[loopIndex].Add(new ItemMoniker(false, (byte)ItemType.SketchSegment, key, i++));
                }
                loopIndex++;
            }


            //var entity = new TopSolid.Kernel.DB.D2.Sketches.PositionedSketchEntity(TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument, 0);
            //TopSolid.Kernel.G.D2.Sketches.Sketch sk2d = new TKGD2.Sketches.Sketch(entity, null, false);

            sheetMaker.SetCurves(loops2d, listItemMok);
            try
            {
                shape = sheetMaker.Make(null, ItemOperationKey.BasicKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return shape;
        }

        private static Shape MakeSheet(Brep inBRep, BrepFace inFace)
        {
            Shape shape = null;
            SheetMaker sheetMaker = new SheetMaker(SX.Version.Current);
            sheetMaker.Surface = Convert.ToHost(inFace.ToNurbsSurface());

            try
            {
                shape = sheetMaker.Make(null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return shape;
        }



        internal static void MakeShapes(Brep inBRep, LogBuilder inLog, double inLinearPrecision, double inAngularPrecision, ShapeList ioShapes)
        {
            if (inBRep != null && inLog != null && ioShapes != null)
            {
                foreach (BrepFace face in inBRep.Faces)
                {
                    if (inBRep.IsValid)
                    {
                        Shape shape = null;

                        shape = MakeSheetFrom3d(inBRep, face, inLinearPrecision);

                        if (shape == null || shape.IsEmpty)
                            shape = MakeSheetFrom2d(inBRep, face, inLinearPrecision);

                        if (shape == null || shape.IsEmpty)
                        {
                            shape = MakeSheet(inBRep, face);
                            //inLog.Report("Face not limited.");
                        }

                        if (shape == null || shape.IsEmpty)
                        { }
                        //inLog.Report("Missing face.");
                        else
                            ioShapes.Add(shape);
                    }
                    else if (inLog != null)
                    { }
                    //inLog.Report("Invalid face.");
                }
            }
        }
        //*/
        #endregion

        #region Other Solid or surface Geometries

        public static Rhino.Geometry.Box ToRhino(this TKGD3.Box box)
        {
            return new Rhino.Geometry.Box(box.Frame.ToRhino(), box.GetExtent().XExtent.ToRhino(), box.GetExtent().YExtent.ToRhino(), box.GetExtent().ZExtent.ToRhino());
        }

        //public static TKGD3.Box ToHost(this Rhino.Geometry.Box box)
        //{
        //    var TsBox = new TKGD3.Box();
        //    
        //}


        #endregion

        //Methods for IEnumerables and other utility converters
        #region utilities
        static bool KnotAlmostEqualTo(double max, double min) =>
        KnotAlmostEqualTo(max, min, 1.0e-09);

        public static Interval ToRhino(this TKG.D1.Extent extent)
        {
            return new Interval(extent.Min, extent.Max);
        }

        public static TKG.S.D3.Point PointToSPoint(this TKGD3.Point point)
        {
            return new TKG.S.D3.Point((float)point.X, (float)point.Y, (float)point.Z);
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

        //AHW Obselete
        //static DoubleList ToDoubleList(NurbsCurveKnotList list, int degree)
        //{
        //    var count = list.Count;
        //    var knots = new double[count + 2];

        //    var min = list[0];
        //    var max = list[count - 1];
        //    var mid = 0.5 * (min + max);
        //    var factor = 1.0 / (max - min); // normalized

        //    // End knot
        //    knots[count + 1] = /*(list[count - 1] - max) * factor +*/ 1.0;
        //    for (int k = count - 1; k >= count - degree; --k)
        //        knots[k + 1] = /*(list[k] - max) * factor +*/ 1.0;

        //    // Interior knots (in reverse order)
        //    int multiplicity = degree + 1;
        //    for (int k = count - degree - 1; k >= degree; --k)
        //    {
        //        double current = list[k] <= mid ?
        //          (list[k] - min) * factor + 0.0 :
        //          (list[k] - max) * factor + 1.0;

        //        double next = knots[k + 2];
        //        if (KnotAlmostEqualTo(next, current))
        //        {
        //            multiplicity++;
        //            if (multiplicity > degree - 2)
        //                current = KnotPrevNotEqual(next);
        //            else
        //                current = next;
        //        }
        //        else multiplicity = 1;

        //        knots[k + 1] = current;
        //    }

        //    // Start knot
        //    for (int k = degree - 1; k >= 0; --k)
        //        knots[k + 1] = /*(list[k] - min) * factor +*/ 0.0;
        //    knots[0] = /*(list[0] - min) * factor +*/ 0.0;

        //    knots.ToList();
        //    var knotDoubleList = new DoubleList();
        //    foreach (double d in knots)
        //    {
        //        knotDoubleList.Add(d);
        //    }
        //    return knotDoubleList;
        //}
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

        static PointList ToPointList(NurbsCurvePointList list)
        {
            var count = list.Count;
            PointList points = new PointList();
            foreach (ControlPoint p in list)
            {
                var location = p.Location;
                var pt = new TKG.D3.Point(location.X, location.Y, location.Z);
                points.Add(pt);
            }

            return points;
        }

        static TKGD2.PointList ToPointList2D(this NurbsCurvePointList list)
        {
            var count = list.Count;
            TKGD2.PointList points = new TKGD2.PointList();
            foreach (ControlPoint p in list)
            {
                var location = p.Location;
                var pt = new TKG.D2.Point(location.X, location.Y);
                points.Add(pt);
            }

            return points;
        }

        public static TKGD2.Extent DomainToTopSolidExtent(this Rhino.Geometry.Surface surface)
        {
            G.D1.Extent extentX = new G.D1.Extent(surface.Domain(0).T0, surface.Domain(0).T1);
            G.D1.Extent extentY = new G.D1.Extent(surface.Domain(1).T0, surface.Domain(1).T1);

            return new TKGD2.Extent(extentX, extentY);
        }
        static Axis LineToAxis(this Line line)
        {
            return new Axis(line.From.ToHost(), line.To.ToHost());
        }

        static TKG.D1.Extent ToHost(this Interval interval)
        {
            var extent = new TKG.D1.Extent(interval.T0, interval.T1);
            return extent;
        }
        #endregion
    }
}
