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

using TopSolid.Kernel.G;
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
using System;
using Version = TopSolid.Kernel.SX.Version;
using Double = TopSolid.Kernel.SX.Double;
using TopSolid.Kernel.WX;
using System.Runtime.ConstrainedExecution;

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

        static public Rhino.Geometry.LineCurve ToRhino(this TKG.D2.Curves.LineCurve l)
        {
            return new Rhino.Geometry.LineCurve(new Point3d(l.Ps.X, l.Ps.Y, 0), new Point3d(l.Pe.X, l.Pe.Y, 0));
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
            {
                return new Rhino.Geometry.Circle(tscircle.Plane.ToRhino(), tscircle.Radius).ToNurbsCurve();
            }
            else if (curve is EllipseCurve tsEllipse)
            {
                return new Rhino.Geometry.Ellipse(tsEllipse.Plane.ToRhino(), tsEllipse.RadiusX, tsEllipse.RadiusY).ToNurbsCurve();
            }
            else if (curve is TKGD3.Curves.LineCurve tsline)
            {
                return tsline.ToRhino();
            }
            else  //(curve is TKGD3.Curves.PolylineCurve tspoly) //TODO
            {
                //return tspoly.ToRhino();
                return null;
            }


        }

        static public TopSolid.Kernel.G.D3.Curves.Curve ToHost(this Rhino.Geometry.Curve rhinoCurve)
        {

            if (rhinoCurve is Rhino.Geometry.LineCurve line)
            {
                return line.ToHost();
            }

            if (rhinoCurve is ArcCurve arcCurve)
            {
                Arc arc = arcCurve.Arc;
                CircleCurve circleCurve = new CircleCurve(arc.Plane.ToHost(), arc.Radius);
                CircleMaker maker = new CircleMaker(SX.Version.Current, topSolidLinear, topSolidAngular);
                maker.SetByCenterAndTwoPoints(
                    arc.Center.ToHost(),
                    arc.StartPoint.ToHost(),
                    arc.EndPoint.ToHost(),
                    false,
                    new UnitVector(arc.Plane.Normal.ToHost()),
                    circleCurve);

                return circleCurve;
            }

            if (rhinoCurve is NurbsCurve nurbsCurve)
            {
                return nurbsCurve.ToHost();
            }

            return rhinoCurve.ToNurbsCurve().ToHost();

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

            #region Variables Declaration           
            Rhino.Collections.Point3dList Cpts = new Rhino.Collections.Point3dList();
            Rhino.Geometry.NurbsCurve rhCurve = null;
            double tol_TS = TopSolid.Kernel.G.Precision.ModelingLinearTolerance;
            #endregion

            #region Conversion cases         
            if (curve.IsLinear())
            {
                rhCurve = ToRhino(new TKG.D3.Curves.LineCurve(curve.Ps, curve.Pe)).ToNurbsCurve();
            }

            //TODO : Case of a complete circle
            //checks if Circular and Converts to Rhino Arc
            else if (curve.IsCircular())
            {
                try
                {
                    rhCurve = new Arc(ToRhino(curve.Ps), ToRhino(curve.Pm), ToRhino(curve.Pe)).ToNurbsCurve();
                }
                catch { }
            }

            else
            {
                foreach (TopSolid.Kernel.G.D3.Point P in curve.CPts)
                {
                    Cpts.Add(ToRhino(P));
                }

                rhCurve = NurbsCurve.Create(false, curve.Degree, Cpts);

                int k = 0;
                foreach (Point3d P in Cpts)
                {
                    try
                    {
                        rhCurve.Points.SetPoint(k, P, curve.CWts[k]);
                    }
                    catch
                    {
                        rhCurve.Points.SetPoint(k, P, 1);
                    }
                    k++;
                }

                for (int i = 1; i < curve.Bs.Count - 1; i++)
                {
                    rhCurve.Knots[i - 1] = curve.Bs[i];
                }
            }
            #endregion
            if (curve.IsClosed())
                rhCurve.MakeClosed(tol_TS);

            return rhCurve;
        }

        static public Rhino.Geometry.NurbsCurve ToRhino(this TKGD3.Curves.IGeometricProfile profile)
        {
            PolyCurve rhCurve = new PolyCurve();
            foreach (IGeometricSegment seg in profile.Segments)
            {
                rhCurve.AppendSegment(seg.GetOrientedCurve().Curve.GetBSplineCurve(false, false).ToRhino());
            }

            return rhCurve.ToNurbsCurve();
        }

        static public Rhino.Geometry.NurbsCurve ToRhino(this TKGD2.Curves.IGeometricProfile profile)
        {
            PolyCurve rhCurve = new PolyCurve();
            foreach (TKGD2.Curves.IGeometricSegment seg in profile.Segments)
            {
                rhCurve.AppendSegment(seg.GetOrientedCurve().Curve.GetBSplineCurve(false, false).ToRhino());
            }

            return rhCurve.ToNurbsCurve();
        }

        static public Rhino.Geometry.NurbsCurve ToRhino(this TKGD2.Curves.BSplineCurve curve)
        {

            #region Variables Declaration           
            Rhino.Collections.Point3dList Cpts = new Rhino.Collections.Point3dList();
            Rhino.Geometry.NurbsCurve rhCurve = null;
            double tol_TS = TopSolid.Kernel.G.Precision.ModelingLinearTolerance;

            #endregion

            #region Conversion cases         
            //if (curve.IsLinear())
            //{
            //    rhCurve = ToRhino(new TKG.D2.Curves.LineCurve(curve.Ps, curve.Pe)).ToNurbsCurve();
            //}

            ////TODO : Case of a complete circle
            ////checks if Circular and Converts to Rhino Arc
            //else if (curve.IsCircular())
            //{
            //    try
            //    {
            //        rhCurve = new Arc(ToRhino(curve.Ps), ToRhino(curve.Pm), ToRhino(curve.Pe)).ToNurbsCurve();
            //    }
            //    catch { }
            //}

            //else
            bool isrational = curve.IsRational;


            {
                foreach (TopSolid.Kernel.G.D2.Point P in curve.CPts)
                {
                    Cpts.Add(P.X, P.Y, 0);
                }

                rhCurve = NurbsCurve.Create(curve.IsPeriodic, curve.Degree, Cpts);

                int k = 0;
                foreach (Point3d P in Cpts)
                {
                    try
                    {
                        rhCurve.Points.SetPoint(k, P, curve.CWts[k]);
                    }
                    catch
                    {
                        rhCurve.Points.SetPoint(k, P, 1);
                    }
                    k++;
                }

                for (int i = 1; i < curve.Bs.Count - 1; i++)
                {
                    rhCurve.Knots[i - 1] = curve.Bs[i];
                }
            }
            bool rev;
            if (curve.IsClosed())
                rev = rhCurve.MakeClosed(tol_TS);
            #endregion


            return rhCurve;
        }

        static public Rhino.Geometry.NurbsCurve ToRhino(this TKGD3.Sketches.Profile profile)
        {
            Rhino.Collections.CurveList rhCurvesList = new Rhino.Collections.CurveList();
            Rhino.Geometry.NurbsCurve rhCurve = null;

            for (int i = 0; i < (profile.Segments.Count()); i++)
            {
                rhCurvesList.Add(ToRhino(profile.Segments.ElementAt(i).Geometry.GetBSplineCurve(false, false, TopSolid.Kernel.G.Precision.LinearPrecision)));
            }

            if (NurbsCurve.JoinCurves(rhCurvesList).Length != 0)
            {
                rhCurve = NurbsCurve.JoinCurves(rhCurvesList)[0].ToNurbsCurve();
            }

            return rhCurve;
        }

        static public Rhino.Geometry.NurbsCurve ToRhino(this TKGD2.Sketches.Profile profile)
        {
            Rhino.Collections.CurveList rhCurvesList = new Rhino.Collections.CurveList();
            Rhino.Geometry.NurbsCurve rhCurve = null;

            for (int i = 0; i < (profile.Segments.Count()); i++)
            {
                rhCurvesList.Add(ToRhino(profile.Segments.ElementAt(i).Geometry.GetBSplineCurve(false, false, TKG.Precision.LinearPrecision)));
            }

            if (NurbsCurve.JoinCurves(rhCurvesList).Length != 0)
            {
                rhCurve = NurbsCurve.JoinCurves(rhCurvesList)[0].ToNurbsCurve();
            }

            return rhCurve;
        }

        #endregion



        #region Surface
        public static TKG.D3.Surfaces.Surface ToHost(this Rhino.Geometry.Surface rhinoSurface)
        {
            //TKG.D3.Surfaces.Surface surface = null;
            if (rhinoSurface is Rhino.Geometry.PlaneSurface rhinoPlaneSurf)
            {
                return rhinoPlaneSurf.ToHost();
            }

            if (rhinoSurface is Rhino.Geometry.RevSurface rhinoSevSurface)
            {
                return rhinoSevSurface.ToHost();
            }

            if (rhinoSurface is Rhino.Geometry.Extrusion rhinoExtrusion)
            {
                return rhinoExtrusion.ToHost();
            }

            if (rhinoSurface is NurbsSurface nurbsSurface)
            {
                return nurbsSurface.ToHost();
            }

            return rhinoSurface.ToNurbsSurface().ToHost();
        }

        public static TKG.D3.Surfaces.PlaneSurface ToHost(this Rhino.Geometry.PlaneSurface rhinoPlaneSurface)
        {

            Rhino.Geometry.Plane plane;
            rhinoPlaneSurface.TryGetPlane(out plane);
            var extent = new TKGD2.Extent(rhinoPlaneSurface.Domain(0).ToHost(), rhinoPlaneSurface.Domain(1).ToHost());
            TKGD3.Surfaces.PlaneSurface planeSurface = new TKGD3.Surfaces.PlaneSurface(plane.ToHost(), extent);
            return planeSurface;

        }

        public static TKG.D3.Surfaces.RevolvedSurface ToHost(this Rhino.Geometry.RevSurface rhinorevSurface)
        {
            TKGD2.Extent extent = new TKGD2.Extent(rhinorevSurface.Domain(0).ToHost(), rhinorevSurface.Domain(1).ToHost());
            RevolvedSurface revolvedSurface = new RevolvedSurface(rhinorevSurface.Curve.ToHost(), rhinorevSurface.Axis.LineToAxis(), extent);
            return revolvedSurface;
        }

        public static TKG.D3.Surfaces.ExtrudedSurface ToHost(this Rhino.Geometry.Extrusion rhinoExtrusion)
        {
            TKGD2.Extent extent = new TKGD2.Extent(rhinoExtrusion.Domain(0).ToHost(), rhinoExtrusion.Domain(1).ToHost());
            ExtrudedSurface extrudedSurface = new ExtrudedSurface(rhinoExtrusion.Profile3d(0, 0).ToHost(), ((UnitVector)rhinoExtrusion.PathLineCurve().Line.Direction.ToHost()), extent);
            return extrudedSurface;
        }

        public static TKG.D3.Surfaces.BSplineSurface ToHost(this NurbsSurface nurbsSurface)
        {
            bool isRational = nurbsSurface.IsRational;
            bool isUPeriodic = nurbsSurface.IsPeriodic(0);
            bool isVPeriodic = nurbsSurface.IsPeriodic(1);
            int degreeU = nurbsSurface.Degree(0);
            int degreeV = nurbsSurface.Degree(1);
            DoubleList knotU = ToDoubleList(nurbsSurface.KnotsU);
            DoubleList knotV = ToDoubleList(nurbsSurface.KnotsV);
            PointList controlPoints = ToPointList(nurbsSurface.Points);
            DoubleList weightList = ToDoubleList(nurbsSurface.Points);

            BSpline bsplineU = new BSpline(isUPeriodic, degreeU, knotU);
            BSpline bsplineV = new BSpline(isVPeriodic, degreeV, knotV);

            if (isRational)
            {
                BSplineSurface bsplineSurface = new BSplineSurface(bsplineU, bsplineV, controlPoints, weightList);
                return bsplineSurface;
            }
            else
            {
                BSplineSurface bsplineSurface = new BSplineSurface(bsplineU, bsplineV, controlPoints);
                return bsplineSurface;
            }
        }

        public static Rhino.Geometry.Surface ToRhino(this IParametricSurface inTSSurface)
        {
            Rhino.Geometry.Surface surf = null;
            if (inTSSurface is TKGD3.Surfaces.PlaneSurface planarsurf)
            {
                surf = new Rhino.Geometry.PlaneSurface(planarsurf.Plane.ToRhino(), planarsurf.Range.XExtent.ToRhino(), planarsurf.Range.YExtent.ToRhino());
            }

            else if (inTSSurface is RevolvedSurface revSurf)
            {
                if (revSurf.Curve.IsLinear())
                {
                    TKGD3.Curves.LineCurve line = (TKGD3.Curves.LineCurve)revSurf.Curve;
                    surf = RevSurface.Create(line.ToRhino().Line, new Line(revSurf.Axis.Po.ToRhino(), revSurf.Axis.Vx.ToRhino()));
                }

                else
                {
                    surf = RevSurface.Create(revSurf.Curve.ToRhino(), new Line(revSurf.Axis.Po.ToRhino(), revSurf.Axis.Vx.ToRhino()));
                }
            }

            else if (inTSSurface is TKGD3.Surfaces.Surface surface)

            {
                surf = surface.GetBsplineGeometry(TKG.Precision.LinearPrecision, false, false, false).ToRhino();
            }
            return surf;
            //return surface.GetBsplineGeometry(TKG.Precision.ModelingLinearTolerance, false, false, false).ToRhino();
        }

        public static Rhino.Geometry.Surface ToRhino(this BSplineSurface surface)
        {
            BSplineSurface _surface = surface;


            bool is_rational = _surface.IsRational;
            int number_of_dimensions = 3;
            int u_degree = _surface.UDegree;
            int v_degree = _surface.VDegree;
            int u_control_point_count = _surface.UCptsCount;
            int v_control_point_count = _surface.VCptsCount;

            var control_points = new Point3d[u_control_point_count, v_control_point_count];

            for (int u = 0; u < u_control_point_count; u++)
            {
                for (int v = 0; v < v_control_point_count; v++)
                {
                    control_points[u, v] = new Point3d(_surface.GetCPt(u, v).X, _surface.GetCPt(u, v).Y, _surface.GetCPt(u, v).Z);
                }
            }

            // creates internal uninitialized arrays for 
            // control points and knots
            var rhsurface = NurbsSurface.Create(
              number_of_dimensions,
              is_rational,
              u_degree + 1,
              v_degree + 1,
              u_control_point_count,
              v_control_point_count
              );

            //add the knots + Adjusting to Rhino removing the 2 extra knots (Superfluous)
            for (int u = 1; u < (_surface.UBs.Count - 1); u++)
                rhsurface.KnotsU[u - 1] = _surface.UBs[u];
            for (int v = 1; v < (_surface.VBs.Count - 1); v++)
                rhsurface.KnotsV[v - 1] = _surface.VBs[v];

            // add the control points
            for (int u = 0; u < rhsurface.Points.CountU; u++)
            {
                for (int v = 0; v < rhsurface.Points.CountV; v++)
                {
                    rhsurface.Points.SetPoint(u, v, control_points[u, v]);
                    try
                    {
                        rhsurface.Points.SetWeight(u, v, _surface.GetCWt(u, v));
                    }
                    catch
                    {
                        rhsurface.Points.SetWeight(u, v, 1);
                    }
                }
            }
            return rhsurface;
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
            List<Brep> listofBrepsrf = new List<Brep>();
            Brep brep = new Brep();

            foreach (Face f in shape.Faces)
            {
                listofBrepsrf.Add(FaceToBrep(f));
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
            List<TKGD3.Shapes.Vertex> vertexlist = new List<TKGD3.Shapes.Vertex>();
            double tol_Rh = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            double tol_TS = TopSolid.Kernel.G.Precision.ModelingLinearTolerance;

            //Topology indexes ?
            int c_index = 0;


            //Create the Brep Surface
            Brep brepsrf = new Brep();


            //function added on request, gets the 2DCurves, 3dCurves and Edges in the correct order
            OrientedSurface osurf = face.GetOrientedBsplineTrimmedGeometry(tol_TS, false, false, false, outer, list2D, list3D, listEdges);

            //Add Vertices
            int ver = 0;
            foreach (EdgeList list in listEdges)
            {
                foreach (TKGD3.Shapes.Edge e in list)
                {
                    //TODO UNCOMMENT as soon as error with IsReversedwithFin is resolved
                    if (!e.IsReversedWithFin(face))
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
                    var crv = ic.GetOrientedCurve().Curve.GetBSplineCurve(false, false);



                    //var f = listEdges.ElementAt(1);

                    //TODO understand why throws an error here for ind = 1

                    if (!listEdges.ElementAt(ind)[indperLoop].IsReversedWithFin(face))
                    {
                        crv.Reverse();
                    }


                    //if (!listEdges.ElementAt(ind)[c_index].IsReversedWithFin())
                    //    rev = brepsrf.Curves3D[c_index].Reverse();

                    c_index = brepsrf.AddEdgeCurve(Convert.ToRhino(crv));
                    indperLoop++;
                }
                ind++;
                indperLoop = 0;
            }

            //Edges
            int i = 0;
            int iperLoop = 0;
            int listcount = 0;
            //int j = 0;
            List<BrepEdge> rhEdges = new List<BrepEdge>();
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
                            if (!e.IsReversedWithFin(face))
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
                            if (!e.IsReversedWithFin(face))
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
            List<BrepTrim> rhTrim = new List<BrepTrim>();
            brepsrf.AddSurface(ToRhino(osurf.Surface as BSplineSurface));
            BrepFace bface = brepsrf.Faces.Add(0);
            BrepLoop rh_loop = null;

            //Get the 2D Curves and convert them to Rhino
            int x = 0;
            foreach (TKGD2.Curves.IGeometricProfile c in list2D)
            {
                var tsloop = face.Loops.ElementAt(loopindex);

                if (tsloop.IsOuter)
                    rh_loop = brepsrf.Loops.Add(BrepLoopType.Outer, bface);
                else
                    rh_loop = brepsrf.Loops.Add(BrepLoopType.Inner, bface);


                foreach (TKGD2.Curves.IGeometricSegment ic in c.Segments)
                {
                    Rhino.Geometry.Curve crv;
                    TKGD2.Curves.BSplineCurve tcrvv = ic.GetOrientedCurve().Curve.GetBSplineCurve(false, false);

                    if (tsloop.IsOuter)
                    {
                        //if (ic.IsReversed)
                        //{
                        //    tcrvv.Reverse();
                        //}

                        crv = Convert.ToRhino(tcrvv);
                        x = brepsrf.AddTrimCurve(crv);
                        rhTrim.Add(brepsrf.Trims.Add(rhEdges[x], ic.IsReversed, rh_loop, x));
                    }

                    else
                    {
                        tcrvv.Reverse();


                        crv = Convert.ToRhino(tcrvv);
                        x = brepsrf.AddTrimCurve(crv);
                        rhTrim.Add(brepsrf.Trims.Add(rhEdges[x], ic.IsReversed, rh_loop, x));
                    }

                    rhTrim[x].SetTolerances(tol_Rh, tol_Rh);
                    rhTrim[x].TrimType = BrepTrimType.Boundary;
                    rhTrim[x].IsoStatus = IsoStatus.None;
                    string log1 = null;
                    rhTrim[x].IsValidWithLog(out log1);

                    x++;
                }
                loopindex++;
            }


            if (osurf.IsReversed)
            {
                brepsrf.Faces.First().OrientationIsReversed = true;
            }

            //string log = null;
            //brepsrf.IsValidWithLog(out log);


            bool match = true;
            if (!brepsrf.IsValid)
            {
                brepsrf.Repair(tol_TS);
                //brepsrf.IsValidWithLog(out log);
                match = brepsrf.Trims.MatchEnds();
                //brepsrf.IsValidWithLog(out log);

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


        internal static void MakeSurfacesAndLoops(Brep inBrep, ShapeList ioSurfs, List<TKG.D3.Sketches.PositionedSketch> ioLoops3d, List<TKG.D2.Sketches.PositionedSketch> ioLoops2d)
        {
            if (inBrep != null && ioSurfs != null && ioLoops3d != null && ioLoops2d != null)
            {
                SheetMaker sheetMaker = new SheetMaker(Version.Current);

                foreach (BrepFace f in inBrep.Faces)
                {

                    //Brep face = f.ToBrep();
                    if (inBrep.IsValid)
                    {
                        sheetMaker.Surface = Convert.ToHost(f.DuplicateSurface().ToNurbsSurface());

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

                                if (!Double.IsFinite(u))
                                    u = 0.0;

                                if (!Double.IsFinite(v))
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
                            List<TKGD3.Curves.CurveList> loops3d = new List<CurveList>();

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


                            List<TKGD2.Curves.CurveList> loops2d = new List<TKGD2.Curves.CurveList>();
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

            TrimmedSheetMaker sheetMaker = new TrimmedSheetMaker(Version.Current);
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
                    //sheetMaker.UsesBRepMethod = true;//AHW setting to true causes an error
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

            TrimmedSheetMaker sheetMaker = new TrimmedSheetMaker(Version.Current);
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
            SheetMaker sheetMaker = new SheetMaker(Version.Current);
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
