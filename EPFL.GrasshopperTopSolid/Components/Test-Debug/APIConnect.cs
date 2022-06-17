using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.DB.D3.Modeling.Documents;

namespace EPFL.GrasshopperTopSolid.Components.Test_Debug
{
    public class APIConnect : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the APIConnect class.
        /// </summary>
        public APIConnect()
          : base("APIConnect", "API",
              "Description",
              "TopSolid", "Test-Debug")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "Run Connect", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Success", "Success", "Connected", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool Run = false;
            if (DA.GetData("Run", ref Run))
            {
                if (Run != true)
                    return;
                TopSolid.Kernel.Automating.DocumentId id = new TopSolid.Kernel.Automating.DocumentId((TopSolid.Kernel.UI.Application.CurrentDocument as AssemblyDocument).PdmDocumentId);
                //var shapes = new TopSolid.Kernel.Automating.IShapes;
                //List<TopSolid.Kernel.Automating.ElementId> elementIds = TopSolid.Kernel.Automating.IShapes.GetShapes(id);

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
            get { return new Guid("94BC182D-E868-4B99-801F-1DE89C2D3615"); }
        }
    }
}