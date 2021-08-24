using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using System.Collections.Generic;
using System.Linq;
using TopSolid.Kernel.G.D1;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.G.D3.Curves;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.G.D3.Sketches;
using TopSolid.Kernel.G.D3.Surfaces;
using TopSolid.Kernel.SX.Collections;
using TKG = TopSolid.Kernel.G;
using TKGD2 = TopSolid.Kernel.G.D2;
using TKGD3 = TopSolid.Kernel.G.D3;
using TSXGen = TopSolid.Kernel.SX.Collections.Generic;

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

        static public Rhino.Geometry.Line ToRhino(this TKG.D2.Curves.LineCurve l)
        {
            return new Rhino.Geometry.Line(new Point3d(l.Ps.X, l.Ps.Y, 0), new Point3d(l.Pe.X, l.Pe.Y, 0));
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
                    rhCurve.Knots[i - 1] = curve.Bs[i];
                }
            }
            #endregion

            return rhCurve;
        }



        static public Rhino.Geometry.NurbsCurve ToRhino(TKGD2.Curves.BSplineCurve curve)
        {

            #region Variables Declaration           
            Rhino.Collections.Point3dList Cpts = new Rhino.Collections.Point3dList();
            Rhino.Geometry.NurbsCurve rhCurve = null;
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
            {
                foreach (TopSolid.Kernel.G.D2.Point P in curve.CPts)
                {
                    Cpts.Add(P.X, P.Y, 0);
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

        static public List<Brep> ToRhino(Shape shape)
        {
            List<Brep> listofBrepsrf = new List<Brep>();
            Brep brep = new Brep();

            foreach (Face f in shape.Faces)
            {
                listofBrepsrf.Add(FaceToBrep(f));
            }

            //var result = Brep.JoinBreps(listofBrepsrf, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            //for (int i = 0; i < result.Length; i++)
            //{
            //    result[i].Repair(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            //}

            return listofBrepsrf;






            //shape.Edges.First().
            /*
             public RH.Brep BrepToNative(Brep brep)
    {
      var tol = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
      try
      {
        // TODO: Provenance exception is meaningless now, must change for provenance build checks.
        // if (brep.provenance != Speckle.Core.Kits.Applications.Rhino)
        //   throw new Exception("Unknown brep provenance: " + brep.provenance +
        //                       ". Don't know how to convert from one to the other.");

        var newBrep = new RH.Brep();
        brep.Curve3D.ForEach(crv => newBrep.AddEdgeCurve(CurveToNative(crv)));
        brep.Curve2D.ForEach(crv => newBrep.AddTrimCurve(CurveToNative(crv)));
        brep.Surfaces.ForEach(surf => newBrep.AddSurface(SurfaceToNative(surf)));
        brep.Vertices.ForEach(vert => newBrep.Vertices.Add(PointToNative(vert).Location, tol));
        brep.Edges.ForEach(edge =>
        {
          if (edge.Domain == null || (edge.Domain.start == edge.Curve.domain.start && edge.Domain.end == edge.Curve.domain.end))
            newBrep.Edges.Add(edge.Curve3dIndex);
          else
            newBrep.Edges.Add(edge.StartIndex, edge.EndIndex, edge.Curve3dIndex, IntervalToNative(edge.Domain), tol);
        });
        brep.Faces.ForEach(face =>
        {
          var f = newBrep.Faces.Add(face.SurfaceIndex);
          f.OrientationIsReversed = face.OrientationReversed;
        });

        brep.Loops.ForEach(loop =>
        {
          var f = newBrep.Faces[loop.FaceIndex];
          var l = newBrep.Loops.Add((RH.BrepLoopType)loop.Type, f);
          loop.Trims.ToList().ForEach(trim =>
          {
            RH.BrepTrim rhTrim;
            if (trim.EdgeIndex != -1)
              rhTrim = newBrep.Trims.Add(newBrep.Edges[trim.EdgeIndex], trim.IsReversed,
                newBrep.Loops[trim.LoopIndex], trim.CurveIndex);
            else if (trim.TrimType == BrepTrimType.Singular)
              rhTrim = newBrep.Trims.AddSingularTrim(newBrep.Vertices[trim.EndIndex],
                newBrep.Loops[trim.LoopIndex], (RH.IsoStatus)trim.IsoStatus, trim.CurveIndex);
            else
              rhTrim = newBrep.Trims.Add(trim.IsReversed, newBrep.Loops[trim.LoopIndex], trim.CurveIndex);

            rhTrim.IsoStatus = (IsoStatus)trim.IsoStatus;
            rhTrim.TrimType = (RH.BrepTrimType)trim.TrimType;
            rhTrim.SetTolerances(tol, tol);
          });
        });

        newBrep.Repair(tol);

        return newBrep;
      }
             */



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



            OrientedSurface osurf = face.GetOrientedBsplineTrimmedGeometry(tol_TS, false, false, false, outer, list2D, list3D, listEdges);


            //list2D.IndexOf(list2D.Where(x => x.Pe == x.Ps && x.Te == x.Tm).FirstOrDefault());

            //Add Vertices
            int ver = 0;
            foreach (EdgeList list in listEdges)
            {
                foreach (TKGD3.Shapes.Edge e in list)
                {
                    vertexlist.Add(e.StartVertex);
                    ver++;
                }
            }
            foreach (TKG.D3.Shapes.Vertex v in vertexlist)
            {
                brepsrf.Vertices.Add(Convert.ToRhino(v.GetGeometry()), tol_TS);
            }

            //Get the 3D Curves and convert them to Rhino
            //List<BrepEdge> brepEdges = new List<BrepEdge>();
            foreach (TKGD3.Curves.IGeometricProfile c in list3D)
            {
                foreach (TKGD3.Curves.IGeometricSegment ic in c.Segments)
                {
                    c_index = brepsrf.AddEdgeCurve(Convert.ToRhino(ic.GetOrientedCurve().Curve.GetBSplineCurve(false, false)));
                    //if (ic.IsReversed)
                    //    brepsrf.Curves3D[c_index].Reverse();
                    //brepEdges.Add(brepsrf.Edges.Add(c_index));

                }
            }

            //Edges
            int i = 0;
            //int j = 0;
            List<BrepEdge> edge = new List<BrepEdge>();
            foreach (EdgeList list in listEdges)
            {
                foreach (Edge e in list)
                {
                    if (i + 1 == list.Count)
                    {
                        //if (e.IsReversed())
                        //    brepsrf.Edges.Add(brepsrf.Vertices[0], brepsrf.Vertices[i], i, tol_Rh);
                        //else
                        edge.Add(brepsrf.Edges.Add(brepsrf.Vertices[i], brepsrf.Vertices[0], i, tol_TS));
                    }
                    else
                    {
                        //if (e.IsReversed())
                        //    brepsrf.Edges.Add(brepsrf.Vertices[i], brepsrf.Vertices[0], i, tol_Rh);
                        //else
                        edge.Add(brepsrf.Edges.Add(brepsrf.Vertices[i], brepsrf.Vertices[i + 1], i, tol_TS));

                    }

                    i++;
                }
            }



            int loopindex = 0;
            List<BrepTrim> rhTrim = new List<BrepTrim>();
            brepsrf.AddSurface(ToRhino(osurf.Surface as BSplineSurface));
            BrepFace bface = brepsrf.Faces.Add(0);
            BrepLoop rh_loop = null;


            //For Debug
            List<Rhino.Geometry.Curve> Rh_2dCurves = new List<Rhino.Geometry.Curve>();
            bool crvbool;

            //Get the 2D Curves and convert them to Rhino
            int x = 0;
            foreach (TKGD2.Curves.IGeometricProfile c in list2D)
            {
                //c.Adjust(TKGD2.Curves.AdjustType.ByMove, true, tol_TS);
                var tsloop = face.Loops.ElementAt(loopindex);

                if (tsloop.IsOuter)
                    rh_loop = brepsrf.Loops.Add(BrepLoopType.Outer, bface);
                else
                    rh_loop = brepsrf.Loops.Add(BrepLoopType.Inner, bface);



                foreach (TKGD2.Curves.IGeometricSegment ic in c.Segments)
                {
                    string trimlog = "";
                    bool trimbool;
                    Rhino.Geometry.Curve crv;

                    TKGD2.Curves.BSplineCurve tcrvv = ic.GetOrientedCurve().Curve.GetBSplineCurve(false, false);
                    if (ic.IsReversed)
                    {
                        tcrvv.Reverse();
                    }

                    crv = Convert.ToRhino(tcrvv);
                    Rh_2dCurves.Add(crv);

                    crvbool = (listEdges[loopindex][x].IsReversed());

                    x = brepsrf.AddTrimCurve(crv);


                    rhTrim.Add(brepsrf.Trims.Add(edge[x], !ic.IsReversed, rh_loop, x));
                    rhTrim[x].SetTolerances(tol_Rh, tol_Rh);

                    //rhTrim[x].TrimType = BrepTrimType.Unknown;
                    trimbool = rhTrim[x].IsValidWithLog(out trimlog);

                    x++;

                }

                //loopbool2 = brepsrf.Trims.MatchEnds(rh_loop);

                loopindex++;
            }


            string looplog = "";
            bool loopbool = rh_loop.IsValidWithLog(out looplog);


            if (osurf.IsReversed)
            {
                brepsrf.Faces.First().OrientationIsReversed = true;
            }



            //Debug invalid breps
            string whyyyyy = "";
            bool whyy = brepsrf.IsValidWithLog(out whyyyyy);

            string log1, log2, log3;
            bool topo = brepsrf.IsValidTopology(out log1);
            bool geo = brepsrf.IsValidGeometry(out log2);
            bool tol_flags = brepsrf.IsValidTolerancesAndFlags(out log3);

            brepsrf.Repair(tol_Rh);

            bool match = brepsrf.Trims.MatchEnds();
            brepsrf.SetTolerancesBoxesAndFlags(false, true, true, true, true, false, false, false);

            topo = brepsrf.IsValidTopology(out log1);
            geo = brepsrf.IsValidGeometry(out log2);
            tol_flags = brepsrf.IsValidTolerancesAndFlags(out log3);

            //match = brepsrf.Trims.MatchEnds();

            //topo = brepsrf.IsValidTopology(out log1);
            //geo = brepsrf.IsValidGeometry(out log2);
            //tol_flags = brepsrf.IsValidTolerancesAndFlags(out log3);
            if (!match || !brepsrf.IsValid)
            {
                brepsrf.Repair(tol_Rh);
                topo = brepsrf.IsValidTopology(out log1);
                geo = brepsrf.IsValidGeometry(out log2);
                tol_flags = brepsrf.IsValidTolerancesAndFlags(out log3);

                match = brepsrf.Trims.MatchEnds();
                topo = brepsrf.IsValidTopology(out log1);
                geo = brepsrf.IsValidGeometry(out log2);
                tol_flags = brepsrf.IsValidTolerancesAndFlags(out log3);


            }


            //match = brepsrf.Trims.MatchEnds();



            //brepsrf.Standardize();
            //brepsrf.Compact();

            //brepsrf.Repair(tol_TS);


            //Debug after repair


            whyy = brepsrf.IsValidWithLog(out whyyyyy);


            if (brepsrf.IsValid == false)
            {
                bool rep = brepsrf.Repair(tol_Rh);
            }

            whyy = brepsrf.IsValidWithLog(out whyyyyy);

            return brepsrf;

        }
    }
}
