using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TK = TopSolid.Kernel;
//using TKG = TopSolid.Kernel.G;
using Grasshopper.Kernel;
using TopSolid.Kernel.G.D3.Curves;

namespace EPFL.GrasshopperTopSolid
{
    public static class TsIGeometryToRhino
    {
        static public List<IGH_GeometricGoo> ToRhino(TK.G.IGeometry tsGeometry)
        {
            List<IGH_GeometricGoo> ghGeometry = new List<IGH_GeometricGoo>();

            switch (tsGeometry)
            {
                case TopSolid.Kernel.G.D3.Plane plane:
                    {
                        GH_Plane ghPlane = new GH_Plane();
                        GH_Convert.ToGHPlane(plane.ToRhino(), GH_Conversion.Both, ref ghPlane);
                        ghGeometry.Add(ghPlane);
                        break;
                    }

                case TopSolid.Kernel.G.D3.Point point:
                    {
                        GH_Point ghPoint = new GH_Point();
                        GH_Convert.ToGHPoint(point.ToRhino(), GH_Conversion.Both, ref ghPoint);
                        ghGeometry.Add(ghPoint);
                        break;
                    }

                case TopSolid.Kernel.G.D2.Point point:
                    {
                        GH_Point ghPoint = new GH_Point();
                        GH_Convert.ToGHPoint(point.ToRhino(), GH_Conversion.Both, ref ghPoint);
                        ghGeometry.Add(ghPoint);
                        break;
                    }

                case IGeometricProfile profile:
                    {
                        GH_Curve ghCurve = new GH_Curve();
                        GH_Convert.ToGHCurve(profile.ToRhino(), GH_Conversion.Both, ref ghCurve);
                        ghGeometry.Add(ghCurve);
                        break;
                    }
                case TK.G.D3.Curves.IGeometricSegment segment:
                    {
                        {
                            GH_Curve ghCurve = new GH_Curve();
                            GH_Convert.ToGHCurve(TsIGeometryToRhino.ToRhino(segment.GetOrientedCurve()), GH_Conversion.Both, ref ghCurve);
                            ghGeometry.Add(ghCurve);
                            break;
                        }
                    }
                case TK.G.D2.Curves.IGeometricSegment segment:
                    {
                        {
                            GH_Curve ghCurve = new GH_Curve();
                            GH_Convert.ToGHCurve(TsIGeometryToRhino.ToRhino(segment.GetOrientedCurve()), GH_Conversion.Both, ref ghCurve);
                            ghGeometry.Add(ghCurve);
                            break;
                        }
                    }


                case TK.G.D2.Curves.IGeometricProfile profile2d:
                    {
                        GH_Curve ghCurve = new GH_Curve();
                        GH_Convert.ToGHCurve(profile2d.ToRhino(), GH_Conversion.Both, ref ghCurve);
                        ghGeometry.Add(ghCurve);
                        break;
                    }

                case TK.G.D3.Curves.Curve curve:
                    {
                        GH_Curve ghCurve = new GH_Curve();
                        GH_Convert.ToGHCurve(curve.ToRhino(), GH_Conversion.Both, ref ghCurve);
                        ghGeometry.Add(ghCurve);
                        break;
                    }

                case TK.G.D3.Shapes.Shape shape:
                    {
                        GH_Brep ghBrep = new GH_Brep();
                        GH_Convert.ToGHBrep(shape.ToRhino().First(), GH_Conversion.Both, ref ghBrep);
                        ghGeometry.Add(ghBrep);
                        break;
                    }
                //TopSolid Frame converted to Rhino Planes
                #region Frames
                case TK.G.D3.Frame frame:
                    {
                        GH_Plane ghPlan = new GH_Plane();
                        GH_Convert.ToGHPlane(frame.ToRhino(), GH_Conversion.Both, ref ghPlan);
                        ghGeometry.Add(ghPlan);
                        break;
                    }

                case TK.G.D2.Frame frame:
                    {
                        GH_Plane ghPlan = new GH_Plane();
                        GH_Convert.ToGHPlane(frame.ToRhino(), GH_Conversion.Both, ref ghPlan);
                        ghGeometry.Add(ghPlan);
                        break;
                    }
                #endregion


                #region Sketches
                case TK.G.D3.Sketches.Sketch sk:
                    {

                        foreach (var profile in sk.Profiles)
                        {
                            GH_Curve curve = new GH_Curve();
                            GH_Convert.ToGHCurve(profile.ToRhino(), GH_Conversion.Both, ref curve);
                            ghGeometry.Add(curve);

                        }
                        break;
                    }

                case TK.G.D2.Sketches.Sketch sk:
                    {
                        foreach (var profile in sk.Profiles)
                        {
                            GH_Curve curve = new GH_Curve();
                            GH_Convert.ToGHCurve(profile.ToRhino(), GH_Conversion.Both, ref curve);
                            ghGeometry.Add(curve);

                        }
                        break;
                    }
                    #endregion
            }
            return ghGeometry;


        }
    }
}
