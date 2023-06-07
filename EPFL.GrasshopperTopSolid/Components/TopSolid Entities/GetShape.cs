using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Shapes;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_Entities
{
    public class GetShape : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GetShape()
          : base("GetShape", "GetShape",
              "Gets a TopSolid Shape by Name and retruns a Rhino Brep",
              "TopSolid", "TopSolid Entities")
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
            pManager.AddGenericParameter("TopSolidShape", "TSShape", "TopSolid Shape", GH_ParamAccess.list);
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
                list = entity.Geometry.ToRhino().ToList();
            }

            DA.SetDataList("RhinoBrep", list);
            DA.SetData("TopSolidShape", entity);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.TopSolid;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("99640714-70e0-4dc2-a63c-d9768c1c276f"); }
        }
    }
}
