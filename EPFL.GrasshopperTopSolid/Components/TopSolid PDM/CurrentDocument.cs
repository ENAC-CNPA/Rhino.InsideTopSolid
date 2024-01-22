using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.TX.Documents;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_PDM
{
    public class CurrentDocument : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CurrentDocument class.
        /// </summary>
        public CurrentDocument()
          : base("CurrentDocument", "CurrentDoc",
              "Gets TopSolid Current Document",
              "TopSolid", "TopSolid PDM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("TS Current Document", "TS Doc", "Gets TopSolid Current Part or Assembly Document", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //TopSolid.Kernel.WX.Application.CurrentDocumentChanged += Application_CurrentDocumentChanged;
            DesignDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as DesignDocument;
            //Application_CurrentDocumentChanged(DA, EventArgs.Empty);
            DA.SetData(0, doc);
        }

        //private void Application_CurrentDocumentChanged(object sender, EventArgs e)
        //{
        //    IGH_DataAccess DA = sender as IGH_DataAccess;
        //    DA.SetData(0, TopSolid.Kernel.UI.Application.CurrentDocument);

        //}

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.Document, 24, 24).ToBitmap();

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("992E7E04-5386-4490-9011-7735B8CF7D36"); }
        }
    }
}