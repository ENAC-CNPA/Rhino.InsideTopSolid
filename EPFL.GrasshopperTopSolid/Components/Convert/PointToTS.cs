using System;
using System.Collections.Generic;
using TK = TopSolid.Kernel;
//using TK.G = TopSolid.Kernel.G;
using Grasshopper.Kernel;
using Rhino.Geometry;
using TopSolid.Kernel.GR;
using TopSolid.Kernel.GR.Displays;
using TopSolid.Kernel.GR.D3;
using TopSolid.Kernel.GR.Attributes;
using TopSolid.Kernel.SX.Drawing;
using Grasshopper.Kernel.Types;
using TopSolid.Kernel.DB.D3.Documents;

namespace EPFL.GrasshopperTopSolid.Components
{
    public class PointToTS : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public PointToTS()
          : base("PointToTS", "Pt2TS",
              "Convert GH Point to TopSolid Points",
              "TopSolid", "Convert")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Rhino Points", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Points", "P", "TopSolid Points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> pts = new List<Point3d>();

            if (!DA.GetDataList(0, pts)) { return; }

            if (pts == null) { return; }
            if (pts.Count == 0) { return; }


            GeometricDocument document = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;
            GeneralDisplay myGeneralDisplay = new GeneralDisplay(null);
            List<TK.G.D3.Point> TSpts = new List<TK.G.D3.Point>();

            if (myGeneralDisplay != null && document.Display.ContainsDisplay(myGeneralDisplay))
            {
                // Remove the general display to the document display
                document.Display.RemoveDisplay(myGeneralDisplay);
            }

            foreach (Point3d pt in pts)
            {
                var p = pt.ToHost();
                TSpts.Add(p);
                MarkerItem markerTSpoint = new MarkerItem(p);
                markerTSpoint.Color = Color.Green;
                markerTSpoint.MarkerStyle = MarkerStyle.ExtraLargeTriangle;
                myGeneralDisplay.Add(markerTSpoint);
            }

            document.Display.AddDisplay(myGeneralDisplay);
            TopSolid.Kernel.UI.Application.Update();

            DA.SetDataList(0, TSpts);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Geometrie_Old;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FDC62C6D-7C03-412D-8FF8-B76439197730"); }
        }
    }
}