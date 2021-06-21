using Grasshopper.Kernel;
using System;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.Entities;

namespace EPFL.GrasshopperTopSolid.Components
{
    public class GetEntity : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetEntity class.
        /// </summary>
        public GetEntity()
          : base("GetEntity", "Getentity",
              "Gets TopSolid Entity with a specific name",
              "TopSolid", "TopSolid Entities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of the Entity to Get", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Entity", "E", "TopSolid Entity from DB", GH_ParamAccess.item);
            pManager.AddTextParameter("Out", "Out", "Out", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            DA.GetData(0, ref name);
            GeometricDocument document = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;

            Entity entity = document.RootEntity.SearchDeepEntity(name); //as PositionedSketchEntity;
            DA.SetData(0, entity);
            DA.SetData("Out", entity.GetType().ToString());
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
            get { return new Guid("8f58e33a-6197-4164-b12e-38c6c773f909"); }
        }
    }
}
