using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using G = TopSolid.Kernel.G;
using Grasshopper.Kernel;
using TopSolid.Kernel.G.D3.Curves;

namespace EPFL.GrasshopperTopSolid
{
    public static class ConvertTSGeometryToRhino
    {
        static public IGH_GeometricGoo ToRhino(G.IGeometry tsGeometry)
        {
            IGH_GeometricGoo ghGeometry = null;

            switch (tsGeometry)
            {
                case TopSolid.Kernel.G.D3.Point point:
                    GH_Point ghPoint = new GH_Point();
                    GH_Convert.ToGHPoint(point.ToRhino(), GH_Conversion.Both, ref ghPoint);
                    ghGeometry = ghPoint;
                    return ghGeometry;
                    break;

                case IGeometricProfile profile:
                    GH_Curve ghCurve = new GH_Curve();
                    GH_Convert.ToGHCurve(profile.ToRhino(), GH_Conversion.Both, ref ghCurve);
                    ghGeometry = ghCurve;
                    return ghGeometry;
                    break;

                case G.D2.Curves.IGeometricProfile profile2d:
                    GH_Curve ghCurve2d = new GH_Curve();
                    GH_Convert.ToGHCurve(profile2d.ToRhino(), GH_Conversion.Both, ref ghCurve2d);
                    ghGeometry = ghCurve2d;
                    return ghGeometry;
                    break;

                case G.D3.Curves.Curve curve: //TODO
                    //GH_Curve ghCurve = new GH_Curve();
                    //GH_Convert.ToGHCurve(curve.ToRhino(), GH_Conversion.Both, ref gh);

                    //return ghGeometry;
                    break;

            }
            return null;


        }
    }
}
