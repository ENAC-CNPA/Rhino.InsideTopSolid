using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopSolid.Kernel.G;

namespace EPFL.GrasshopperTopSolid
{
    public static class ConvertTSGeometryToRhino
    {
        static public IGH_GeometricGoo ToRhino(IGeometry tsGeometry)
        {
            IGH_GeometricGoo ghGeometry = null;
            switch (tsGeometry)
            {
                case TopSolid.Kernel.G.D3.Point point:

                    ghGeometry.CastFrom(point.ToRhino());
                    return ghGeometry;
                    break;
            }
            return null;


        }
    }
}
