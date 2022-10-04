using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Kernel.DB.D2.Sketches;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.Documents;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.G;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Pdm;
using TKG = TopSolid.Kernel.G;
using TKUI = TopSolid.Kernel.UI;
using SK2D = TopSolid.Kernel.G.D2.Sketches;
using SK3D = TopSolid.Kernel.G.D3.Sketches;

namespace EPFL.GrasshopperTopSolid.Components.Geometry
{
    public class EntityGeometry : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EntityGeometry class.
        /// </summary>
        public EntityGeometry()
          : base("EntityGeometry", "Geometry",
              "Gets The Geometry of an Entity",
              "TopSolid", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("TSDocument", "Doc", "Document containing entity (optional, if nothing will take active document", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager.AddGenericParameter("TSEntity", "Entity", "TopSolid Entity or Entity Name", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddGenericParameter("TSGeometry", "Geometry", "TopSolid Geometry", GH_ParamAccess.item);
            pManager.AddGeometryParameter("RhGeometry", "Geometry", "Geometry converted to Rhino as List", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();
            TKG.IGeometry geometry = null;
            Entity res = null;
            Document currentDocument = null;
            if (DA.GetData("TSDocument", ref wrapper))
            {
                if (wrapper.Value != null)
                {
                    if (wrapper.Value is string || wrapper.Value is GH_String)
                    {
                        currentDocument = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault() as ModelingDocument;
                    }
                    else if (wrapper.Value is IDocumentItem)
                        currentDocument = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as ModelingDocument;
                    else if (wrapper.Value is IDocument)
                        currentDocument = wrapper.Value as ModelingDocument;
                }
            }

            if (currentDocument is null)
                currentDocument = TKUI.Application.CurrentDocument as ModelingDocument;

            DA.GetData("TSEntity", ref wrapper);
            if (wrapper != null)
            {
                if (wrapper.Value is string || wrapper.Value is GH_String)
                {
                    res = currentDocument.RootEntity.SearchDeepEntity(wrapper.Value.ToString());
                    if (res != null)
                    {
                        if (!res.HasGeometry)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Entity has no Geometry");
                            return;
                        }
                        else geometry = res.Geometry;
                    }
                }

                else if (wrapper.Value is Entity)
                {
                    res = wrapper.Value as Entity;
                    if (res != null)
                    {
                        if (!res.HasGeometry)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Entity has no Geometry");
                            return;
                        }
                        else geometry = res.Geometry;
                    }
                }

                else if (wrapper.Value is IGeometry geo)
                    geometry = geo;

                //Sketch profiles are structs, not IGeometries, Maybe write a GetIGeomtry method? TODO
                else if (wrapper.Value is SK2D.Profile profile)
                {
                    geometry = profile.MakeGeometricProfile();
                }

                else if (wrapper.Value is SK2D.Vertex vertex)
                {
                    geometry = vertex.Geometry;
                }


                var type = wrapper.Value.GetType();
            }

            if (geometry is null)
                return;

            else
            {
                DA.SetData("TSGeometry", geometry);
                DA.SetDataList("RhGeometry", geometry.ToRhino());
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
            get { return new Guid("64ECB5CA-BB4B-4228-B851-809E21A722B5"); }
        }
    }
}