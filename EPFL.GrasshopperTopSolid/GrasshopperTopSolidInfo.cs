using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace EPFL.GrasshopperTopSolid
{
    public class GrasshopperTopSolidInfo : GH_AssemblyInfo
    {
        public override string Name => "GrasshopperTopSolid";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Plugin for RhinoInside.Topsolid";

        public override Guid Id => new Guid("0C0A3899-619E-486F-90EC-A5C6F9B6DAED");

        //Return a string identifying you or your company.
        public override string AuthorName => "EPFL ENAC IA CNPA";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "cnpa@epfl.ch";
    }

    public class GrasshopperTopSolidIcon : Grasshopper.Kernel.GH_AssemblyPriority
    {
        public override Grasshopper.Kernel.GH_LoadingInstruction PriorityLoad()
        {
            Grasshopper.Instances.ComponentServer.AddCategoryIcon("TopSolid", Properties.Resources.TopSolid);
            Grasshopper.Instances.ComponentServer.AddCategorySymbolName("TopSolid", 'T');
            return Grasshopper.Kernel.GH_LoadingInstruction.Proceed;
        }
    }

}