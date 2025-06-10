using System;
using System.Collections.Generic;
using Cirtes.Strato.Cad.DB.Documents;
using Grasshopper.Kernel;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB.Documents;

namespace EPFL.GrasshopperTopSolid.Components.STRATO
{
    public class GetCurrentStratoDocument : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetCurrentStratoDocument class.
        /// </summary>
        public GetCurrentStratoDocument()
          : base("GetCurrentStratoDocument", "StDoc",
              "Gets Current Strato Document",
              "TopSolid", "Strato")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("TS Current Document", "TS Doc", "Gets TopSolid Current Part or Assembly Document", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            SlicePartsDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as SlicePartsDocument;
            DA.SetData(0, doc);
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.Cirtes_Strato_Cad_UI_Divisions_Slices_SliceCommand, 24, 24).ToBitmap();


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C7370ECC-10B7-42CD-A033-51CF00CC0486"); }
        }
    }
}