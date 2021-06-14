using Rhino.Geometry;
using Rhino.Geometry.Collections;
using System.Linq;
using TopSolid.Kernel.G.D1;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.G.D3.Curves;
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

        public static Rhino.Geometry.Surface ToHost(this BSplineSurface s)
        {
            bool r = s.IsRational;
            bool pU = s.IsUPeriodic;
            bool pV = s.IsVPeriodic;
            var dU = s.UDegree;
            var dV = s.VDegree;
            var kU = s.UBs;


            BSpline bU = new BSpline(pU, dU, kU);
            BSpline bV = new BSpline(pV, dV, kV);


            return bs;


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
