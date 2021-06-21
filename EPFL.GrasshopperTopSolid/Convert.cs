using Rhino.Geometry;
using Rhino.Geometry.Collections;
using System.Linq;
using TopSolid.Kernel.G.D1;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.G.D3.Curves;
using TopSolid.Kernel.G.D3.Sketches;
using TopSolid.Kernel.G.D3.Surfaces;
using TopSolid.Kernel.SX.Collections;
using TKG = TopSolid.Kernel.G;

namespace EPFL.GrasshopperTopSolid
{
    public static class Convert
    {
        #region Point
        static public TKG.D3.Point ToHost(this Point3d p)
        {
            return new TKG.D3.Point(p.X, p.Y, p.Z);
        }
        static public TKG.D3.Point ToHost(this Rhino.Geometry.Point p)
        {
            Point3d pt = p.Location;
            return new TKG.D3.Point(pt.X, pt.Y, pt.Z);
        }
        static public Point3d ToRhino(this TKG.D3.Point p)
        {
            return new Point3d(p.X, p.Y, p.Z);
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
        #endregion
        #region Line
        static public TKG.D3.Curves.LineCurve ToHost(this Rhino.Geometry.Line l)
        {
            return new TKG.D3.Curves.LineCurve(l.From.ToHost(), l.To.ToHost());
        }
        static public Rhino.Geometry.Line ToRhino(this TKG.D3.Curves.LineCurve l)
        {
            return new Rhino.Geometry.Line(l.Ps.ToRhino(), l.Pe.ToRhino());
        }


        #endregion
        #region Curve
        static public TopSolid.Kernel.G.D3.Curves.BSplineCurve ToHost(this Rhino.Geometry.NurbsCurve c)
        {
            bool r = c.IsRational;
            bool p = c.IsPeriodic;
            int d = c.Degree;
            DoubleList k = ToDoubleList(c.Knots, d);
            PointList pts = ToPointList(c.Points);
            DoubleList w = ToDoubleList(c.Points);
            BSpline b = new BSpline(p, d, k);
            if (r)
            {
                //var w = c.Points.ConvertAll(x => x.Weight);
                BSplineCurve bs = new BSplineCurve(b, pts, w);
                return bs;
            }
            else
            {
                BSplineCurve bs = new BSplineCurve(b, pts);
                return bs;
            }

        }

        /// <summary>
        /// Converts a single segment of a TopSolid Profile to a Rhino NurbsCurve
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        static public Rhino.Geometry.NurbsCurve ToRhino(BSplineCurve curve)
        {

            #region Variables Declaration           
            Rhino.Collections.Point3dList Cpts = new Rhino.Collections.Point3dList();
            Rhino.Geometry.NurbsCurve rhCurve = null;
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
                    rhCurve.Knots[i] = curve.Bs[i - 1];
                }
            }
            #endregion

            return rhCurve;
        }

        static public Rhino.Geometry.NurbsCurve ToRhino(Profile profile)
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

        static bool KnotAlmostEqualTo(double max, double min) =>
        KnotAlmostEqualTo(max, min, 1.0e-09);

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
            DoubleList w = new DoubleList(count);
            foreach (ControlPoint p in list)
            {
                var weight = p.Weight;
                w.Add(weight);
            }
            return w;
        }
        static DoubleList ToDoubleList(NurbsCurveKnotList list, int degree)
        {
            var count = list.Count;
            var knots = new double[count + 2];

            var min = list[0];
            var max = list[count - 1];
            var mid = 0.5 * (min + max);
            var factor = 1.0 / (max - min); // normalized

            // End knot
            knots[count + 1] = /*(list[count - 1] - max) * factor +*/ 1.0;
            for (int k = count - 1; k >= count - degree; --k)
                knots[k + 1] = /*(list[k] - max) * factor +*/ 1.0;

            // Interior knots (in reverse order)
            int multiplicity = degree + 1;
            for (int k = count - degree - 1; k >= degree; --k)
            {
                double current = list[k] <= mid ?
                  (list[k] - min) * factor + 0.0 :
                  (list[k] - max) * factor + 1.0;

                double next = knots[k + 2];
                if (KnotAlmostEqualTo(next, current))
                {
                    multiplicity++;
                    if (multiplicity > degree - 2)
                        current = KnotPrevNotEqual(next);
                    else
                        current = next;
                }
                else multiplicity = 1;

                knots[k + 1] = current;
            }

            // Start knot
            for (int k = degree - 1; k >= 0; --k)
                knots[k + 1] = /*(list[k] - min) * factor +*/ 0.0;
            knots[0] = /*(list[0] - min) * factor +*/ 0.0;

            knots.ToList();
            var kDl = new DoubleList();
            foreach (double d in knots)
            {
                kDl.Add(d);
            }
            return kDl;
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
        #endregion
        #region Surface
        public static TKG.D3.Surfaces.BSplineSurface ToHost(this NurbsSurface s)
        {
            bool r = s.IsRational;
            bool pU = s.IsPeriodic(0);
            bool pV = s.IsPeriodic(1);
            var dU = s.Degree(0);
            var dV = s.Degree(1);
            var kU = ToDoubleList(s.KnotsU);
            var kV = ToDoubleList(s.KnotsV);
            var cp = ToPointList(s.Points);
            var w = ToDoubleList(s.Points);

            BSpline bU = new BSpline(pU, dU, kU);
            BSpline bV = new BSpline(pV, dV, kV);

            if (r)
            {
                BSplineSurface bs = new BSplineSurface(bU, bV, cp, w);
                return bs;
            }
            else
            {
                BSplineSurface bs = new BSplineSurface(bU, bV, cp);
                return bs;
            }

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


        public static DoubleList ToDoubleList(NurbsSurfaceKnotList list)
        {
            var count = list.Count;
            var knots = new double[count + 2];

            int j = 0, k = 0;
            while (j < count)
                knots[++k] = list[j++];

            knots[0] = knots[1];
            knots[count + 1] = knots[count];
            var kDl = new DoubleList();
            foreach (double d in knots)
            {
                kDl.Add(d);
            }
            return kDl;
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
    }
}
