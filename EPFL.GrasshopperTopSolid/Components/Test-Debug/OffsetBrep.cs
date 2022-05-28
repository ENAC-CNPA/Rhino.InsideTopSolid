using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace EPFL.GrasshopperTopSolid.Components.Test_Debug
{
    public class OffsetBrep : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the OffsetBrep class.
        /// </summary>
        public OffsetBrep()
          : base("OffsetBrep", "Obrep",
              "Offset input Brep",
              "TopSolid", "Test-Debug")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Brep", "Brep", "BRep to Offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "Dist", "Offset Distance", GH_ParamAccess.item);
            pManager.AddBooleanParameter("solid?", "solid?", "solid closed Brep", GH_ParamAccess.item);
            pManager.AddBooleanParameter("extend", "ext", "Wether to maintain sharp corners", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "Tol", "Offset Tolerance", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("resultBrep", "result", "Brep offset Result", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = new Brep();

            if (DA.GetData(0, ref brep) && brep != null)
            {
                double dist = 0, tol = 0;
                bool solid = false, extend = false;
                Brep[] outBlends = null;
                Brep[] outWalls = null;
                Brep[] result = null;

                if (!DA.GetData(1, ref dist) ||
                !DA.GetData(4, ref tol) ||
                !DA.GetData(2, ref solid) ||
                !DA.GetData(3, ref extend))
                    return;



                result = Brep.CreateOffsetBrep(brep, dist, solid, extend, tol, out outBlends, out outWalls);

                DA.SetDataList("resultBrep", result.ToList());
            }
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1B06D3D4-503A-430E-BE8E-D18BC5ECA980"); }
        }
    }
}