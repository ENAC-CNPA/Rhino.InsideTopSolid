using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TKG = TopSolid.Kernel.G;
using Rhino.Geometry;

namespace EPFL.GrasshopperTopSolid
{
    public static class Convert
    {

        static public TKG.D3.Point ToHost(this Point3d p) 
        {
            return new TKG.D3.Point(p.X, p.Y, p.Z);        
        }
        static public Point3d ToRhino(this TKG.D3.Point p)
        {
            return new Point3d(p.X, p.Y, p.Z);
        }

    }
}
