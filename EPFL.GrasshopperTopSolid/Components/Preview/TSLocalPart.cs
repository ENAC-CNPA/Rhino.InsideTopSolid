using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.UI;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Pdm;

namespace EPFL.GrasshopperTopSolid.Components.Preview
{
    public class TSLocalPart : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TSLocalPart class.
        /// </summary>
        public TSLocalPart()
          : base("TSLocalPart", "LP",
              "Bakes Rhino Geometry into Local Part",
              "TopSolid", "Preview")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometries", "G", "Rhino Geometries to bake (as list)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Assembly Document", "A", "target TopSolid Assembly to bake-in, if none provided will get current assembly", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddTextParameter("Name", "Name", "Part Name to be given", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddGenericParameter("TSAttributes", "attributes", "TopSolid's attributes for the created entities", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Bake?", "b?", "Set true to bake", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        bool run = false;
        DesignDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as DesignDocument;


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData("Bake?", ref run) || !run) return;

            //ent = null;
            GH_String name = new GH_String();
            IGH_GeometricGoo geo = null;

            bool sew = false;
            if (!DA.GetData(0, ref geo)) { return; }
            //if (geo == null) { return; }
            //if (geo.Count == 0) { return; }

            DA.GetData("Sew?", ref sew);
            DA.GetData("Name", ref name);


            //Setting target document from input, or else take current document by default
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();

            IDocument res = null;
            if (DA.GetData("TSDocument", ref wrapper))
            {
                if (wrapper.Value is string || wrapper.Value is GH_String)
                {
                    res = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault();
                    doc = res as DesignDocument;
                }
                else if (wrapper.Value is IDocumentItem)
                    doc = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as DesignDocument;
                else if (wrapper.Value is IDocument)
                    doc = wrapper.Value as DesignDocument;
            }

            if (doc == null)
                doc = TopSolid.Kernel.UI.Application.CurrentDocument as DesignDocument;

            //The baking process starts on button
            if (run == true)
            {

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
            get { return new Guid("56F50B6F-7470-450E-9414-060CCC13851F"); }
        }
    }
}