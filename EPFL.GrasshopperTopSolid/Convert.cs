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
