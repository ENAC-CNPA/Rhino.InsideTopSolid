using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Points;
using G = TopSolid.Kernel.G;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_Entities
{
    public class GetPoint : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetPoint class.
        /// </summary>
        public GetPoint()
          : base("GetPoint", "Pt",
              "Gets a TopSolid Point Entity",
              "TopSolid", "TopSolid Entities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of Point Entity", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Rhino Point", "RhPt", "Converted Rhino Point", GH_ParamAccess.item);
            pManager.AddGenericParameter("TopSolid Point", "TSPt", "TopSolid Point Entity", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string _name = "";
            DA.GetData("Name", ref _name);
            GeometricDocument Currentdocument = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;
            var ent = Currentdocument.RootEntity.SearchDeepEntity(_name) as PointEntity;
            if (ent is null) return;

            DA.SetData("Rhino Point", ent.Geometry.ToRhino());
            DA.SetData("TopSolid Point", ent);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.PointCommand, 24, 24).ToBitmap();

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("81CE5BAF-CFAC-4F47-BDC9-690E98BB7904"); }
        }
    }
}