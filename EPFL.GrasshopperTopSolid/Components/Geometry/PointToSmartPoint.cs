using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.DB.D3.Points;
using G = TopSolid.Kernel.G;

namespace EPFL.GrasshopperTopSolid.Components.Geometry
{
    public class PointToSmartPoint : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PointToSmartPoint class.
        /// </summary>
        public PointToSmartPoint()
          : base("PointToSmartPoint", "toSmartPt",
              "Replaces Point Geometry by smart point",
              "TopSolid", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Point", "P", "Point to replace geometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("SmartGeometry", "Geo", "SmartGeometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
            DA.GetData(0, ref obj);
            if (obj == null) return;
            var currentDocument = TopSolid.Kernel.UI.Application.CurrentDocument as DesignDocument;
            if (currentDocument != null)
            {

            }

            //switch (obj.Value)
            //{

            //}

            if (obj.Value is PointEntity pointEntity)
            {

            }
            else if (obj.Value is G.D3.Point point)
            {

            }

            else if (obj.Value is GH_Point rhPoint)
            {
                Rhino.Geometry.Point3d point1;
                rhPoint.CastTo(out point1);
                SmartPoint smartPoint = new BasicSmartPoint(null, point1.ToHost());
                DA.SetData(0, smartPoint);
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
            get { return new Guid("B85FBAED-40F6-4931-A5D4-7D7629157671"); }
        }
    }
}