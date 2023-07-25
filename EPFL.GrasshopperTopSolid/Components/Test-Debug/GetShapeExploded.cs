using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.DB.D3.Shapes;

namespace EPFL.GrasshopperTopSolid.Components.Test_Debug
{
    public class GetShapeExploded : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetShapeExploded class.
        /// </summary>
        public GetShapeExploded()
          : base("GetShapeExploded", "expShape",
              "Gets TopSolid shape into exploded Brep before join to debug",
              "TopSolid", "Test-Debug")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of Shape", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("RhinoBrep", "RhBrep", "Converted Rhino Brep", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string _name = "";
            if (!DA.GetData("Name", ref _name)) return;
            DesignDocument document = TopSolid.Kernel.UI.Application.CurrentDocument as DesignDocument;
            List<Brep> list = new List<Brep>();

            ShapeEntity entity = document.RootEntity.SearchDeepEntity(_name) as ShapeEntity;
            if (entity is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"no valid shapes found");
                return;
            }
            else
            {
                DA.SetDataList("RhinoBrep", entity.Geometry.Faces.Select(x => x.FaceToBrep()));
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
            get { return new Guid("00A8A9D6-0FFB-4788-98C9-94DEADC1A7B8"); }
        }
    }
}