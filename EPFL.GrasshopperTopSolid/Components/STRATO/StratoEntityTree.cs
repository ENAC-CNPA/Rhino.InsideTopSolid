using System;
using System.Collections.Generic;
using System.Linq;
using Cirtes.Strato.Cad.DB.Documents;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Pdm;

namespace EPFL.GrasshopperTopSolid.Components.STRATO
{
    public class StratoEntityTree : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the StratoEntityTree class.
        /// </summary>
        public StratoEntityTree()
          : base("StratoEntityTree", "Strato",
              "Description",
              "TopSolid", "Strato")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "gets a Strato Entity Tree", GH_ParamAccess.item);
        }
        SlicePartsDocument document;
        GH_ObjectWrapper wrapper = new GH_ObjectWrapper();

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Entities", "E", "Documents entities", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //string name = "";
            //DA.GetData(0, ref name);
            GetInputdocument(DA);
            var items = document.RootEntity.DeepConstituents.ToList();
            DA.SetDataList(0, items);

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
            get { return new Guid("8A20944F-57F0-4C1F-8548-A94EE913D741"); }
        }

        private void GetInputdocument(IGH_DataAccess DA)
        {

            if (DA.GetData(0, ref wrapper))
            {
                if (wrapper.Value != null)
                {
                    if (wrapper.Value is string || wrapper.Value is GH_String)
                    {
                        var docs = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString());
                        if (docs.Count() == 1)
                            document = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault() as SlicePartsDocument;
                        else
                            foreach (var doc in docs)
                            {
                                document = doc as SlicePartsDocument;
                                if (document != null) break;
                            }
                    }

                    else if (wrapper.Value is IDocumentItem)
                        document = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as SlicePartsDocument;
                    else if (wrapper.Value is IDocument)
                        document = wrapper.Value as SlicePartsDocument;
                }
            }

            if (document is null)
                document = TopSolid.Kernel.UI.Application.CurrentDocument as SlicePartsDocument;

        }
    }
}