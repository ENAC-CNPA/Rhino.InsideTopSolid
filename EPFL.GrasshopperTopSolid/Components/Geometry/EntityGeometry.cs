using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.Documents;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.TX.Documents;
using G = TopSolid.Kernel.G;
using UI = TopSolid.Kernel.UI;

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
            pManager.AddGenericParameter("TSEntity", "Entity", "TopSolid Entity or Entity Name", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("TSGeometry", "Geometry", "TopSolid Geometry", GH_ParamAccess.item);
            pManager.AddGeometryParameter("RhGeometry", "Geometry", "Geometry converted to Rhino", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();
            DA.GetData("TSEntity", ref wrapper);
            Entity res = null;
            Document currentDocument = UI.Application.CurrentDocument as ModelingDocument;
            if (wrapper != null)
            {
                if (wrapper.Value is string || wrapper.Value is GH_String)
                {
                    res = currentDocument.RootEntity.SearchDeepEntity(wrapper.Value.ToString());
                }

                else if (wrapper.Value is Entity)
                {
                    res = wrapper.Value as Entity;
                }
            }

            if (res == null) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Couldn't find entity");

            if (!res.HasGeometry)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Entity has no Geometry");
            else
            {
                DA.SetData("TSGeometry", res.Geometry);
                DA.SetData("RhGeometry", TSGeometryToRhino.ToRhino(res.Geometry));
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