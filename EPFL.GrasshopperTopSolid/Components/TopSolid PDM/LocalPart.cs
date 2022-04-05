using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Cad.Design.DB.Local.Operations;
using TopSolid.Kernel.DB.Operations;
using TopSolid.Kernel.TX.Undo;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_PDM
{
    public class LocalPart : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the LocalPart class.
        /// </summary>
        public LocalPart()
          : base("LocalPart", "LPart",
              "Creates Local Part in Assembly",
              "TopSolid", "TopSolid PDM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "part name", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            DA.GetData(0, ref name);
            UndoSequence.UndoCurrent();
            UndoSequence.Start("Local", true);
            AssemblyDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as AssemblyDocument;
            LocalPartsCreation partOp = new LocalPartsCreation(doc, 0);
            var part = new TopSolid.Cad.Design.DB.PartEntity(doc, 0);
            part.Name = name;

            part.Create();
            partOp.AddChildPart(part);
            partOp.Create();
            UndoSequence.End();
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
            get { return new Guid("A486E72F-0FF1-47EC-B825-DE499CC3EA6E"); }
        }
    }
}