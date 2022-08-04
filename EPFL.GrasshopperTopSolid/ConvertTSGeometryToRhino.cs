using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TKG = TopSolid.Kernel.G;
using Grasshopper.Kernel;
using TopSolid.Kernel.G.D3.Curves;

namespace EPFL.GrasshopperTopSolid
{
    public static class TSGeometryToRhino
    {
        static public IGH_GeometricGoo ToRhino(TKG.IGeometry tsGeometry)
        {
            IGH_GeometricGoo ghGeometry = null;

            switch (tsGeometry)
            {
                case TopSolid.Kernel.G.D3.Point point:
                    {
                        GH_Point ghPoint = new GH_Point();
                        GH_Convert.ToGHPoint(point.ToRhino(), GH_Conversion.Both, ref ghPoint);
                        ghGeometry = ghPoint;
                        return ghGeometry;
                    }

                case IGeometricProfile profile:
                    {
                        GH_Curve ghCurve = new GH_Curve();
                        GH_Convert.ToGHCurve(profile.ToRhino(), GH_Conversion.Both, ref ghCurve);
                        ghGeometry = ghCurve;
                        return ghGeometry;
                    }

                case TKG.D2.Curves.IGeometricProfile profile2d:
                    {
                        GH_Curve ghCurve = new GH_Curve();
                        GH_Convert.ToGHCurve(profile2d.ToRhino(), GH_Conversion.Both, ref ghCurve);
                        ghGeometry = ghCurve;
                        return ghGeometry;
                    }

                case TKG.D3.Curves.Curve curve:
                    {
                        GH_Curve ghCurve = new GH_Curve();
                        GH_Convert.ToGHCurve(curve.ToRhino(), GH_Conversion.Both, ref ghCurve);
                        ghGeometry = ghCurve;
                        return ghGeometry;
                    }

                case TKG.D3.Shapes.Shape shape:
                    {
                        GH_Brep ghBrep = new GH_Brep();
                        var breppp = shape.ToRhino();
                        GH_Convert.ToGHBrep(shape.ToRhino().First(), GH_Conversion.Both, ref ghBrep);
                        ghGeometry = ghBrep;
                        return ghGeometry;
                    }

            }
            return null;


        }
    }
}
