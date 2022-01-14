using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Kernel.DB.D2.Curves;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.G.D2.Curves;
using TopSolid.Kernel.TX.Undo;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_Operations
{
    public class TSSketch : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TSSketch class.
        /// </summary>
        public TSSketch()
          : base("TS2DSketch", "SK2D",
              "Creates a TopSolid Planar Sketch out of Rhino Profiles",
              "TopSolid", "TopSolid Operations")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "crvs", "curves to send", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Base Plane", "bPlane", "Plane to be Base plane", GH_ParamAccess.item);
            pManager.AddTextParameter("Sketch Name", "Name", "Sketch Name to give to TopSolid", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Create", "Create", "Execute Creation", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Sketch", "SK", "TopSolid Planar Sketch", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<IGH_GeometricGoo> geo = new List<IGH_GeometricGoo>();
            bool run = false;

            Plane rhPlane = new Plane();
            DA.GetData("Base Plane", ref rhPlane);

            if (!DA.GetDataList(0, geo)) { return; }

            if (geo == null) { return; }
            if (geo.Count == 0) { return; }

            DA.GetData(1, ref run);

            //GeometricDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;
            ModelingDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as ModelingDocument;


            if (run == true)
            {
                UndoSequence.UndoCurrent();
                UndoSequence.Start("Bake", true);

                foreach (var g in geo)
                {
                    TSSketch sketchOp = new TSSketch();
                    if (g is GH_Curve gc)
                    {
                        Rhino.Geometry.Curve rc = null;
                        if (!rc.IsPlanar())
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Non planar Curves, Curves will be projected onto plane");
                            rc = Rhino.Geometry.Curve.ProjectToPlane(rc, rhPlane);

                        }
                        GH_Convert.ToCurve(gc, ref rc, 0);
                        if (rc.IsPolyline())
                        {

                        }

                        else
                            var rn = rc.ToNurbsCurve();
                        BSplineCurve tc = rn.ToHost2d();


                        ce.Geometry = tc;
                        ce.Create(doc.PointsFolderEntity);
                    }



                }
                UndoSequence.End();
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
            get { return new Guid("81E61207-1DD7-4CDD-981C-0ECDC63B7838"); }
        }
    }
}