using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using TopSolid.Kernel.DB.D3.Profiles;
using TopSolid.Kernel.DB.D3.Curves;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.D3.Points;
using TopSolid.Kernel.DB.D3.Surfaces;
using TopSolid.Kernel.TX.Undo;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.Parameters;

namespace EPFL.GrasshopperTopSolid.Components
{
    public class TSBake : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TSBake class.
        /// </summary>
        public TSBake()
          : base("TopSolid Bake", "TSBake",
              "Bake Geometries",
              "TopSolid", "Preview")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometries", "G", "Geometries to bake", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Bake?", "b?", "Set true to bake", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<IGH_GeometricGoo> geo = new List<IGH_GeometricGoo>();
            bool run = false;

            if (!DA.GetDataList(0, geo)) { return; }

            if (geo == null) { return; }
            if (geo.Count == 0) { return; }

            DA.GetData(1, ref run);

            GeometricDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;
            ModelingDocument doc2 = TopSolid.Kernel.UI.Application.CurrentDocument as ModelingDocument;


            if (run == true)
            {
                UndoSequence.UndoCurrent();
                UndoSequence.Start("Bake", true);

                foreach (var g in geo)
                {
                    if (g is GH_Point gp)
                    {
                        var rp = new Point3d();
                        GH_Convert.ToPoint3d(gp, ref rp, 0);
                        var tp = rp.ToHost();
                        PointEntity pe = new PointEntity(doc, 0);
                        pe.Geometry = tp;
                        pe.Create(doc.PointsFolderEntity);
                    }
                    else if (g is GH_Curve gc)
                    {
                        Curve rc = null;
                        GH_Convert.ToCurve(gc, ref rc, 0);
                        var rn = rc.ToNurbsCurve();
                        var tc = rn.ToHost();
                        CurveEntity ce = new CurveEntity(doc, 0);
                        ce.Geometry = tc;
                        ce.Create(doc.PointsFolderEntity);
                    }
                    else if (g is GH_Surface gs)
                    {
                        Surface rs = null;
                        GH_Convert.ToSurface(gs, ref rs, 0);
                        var rn = rs.ToNurbsSurface();
                        var ts = rn.ToHost();
                        SurfaceEntity se = new SurfaceEntity(doc, 0);
                        se.Geometry = ts;
                        se.Create(doc.PointsFolderEntity);
                    }

                    else if (g is GH_Brep gbrep)
                    {
                        Brep rs = null;
                        GH_Convert.ToBrep(gbrep, ref rs, 0);

                        var shape = rs.ToHost();


                        foreach (var ts in shape)
                        {
                            ShapeEntity se = new ShapeEntity(doc, 0);
                            se.Geometry = ts;
                            se.Create(doc2.ShapesFolderEntity);
                        }

                        //TODO
                        //ShapeEntity se = new ShapeEntity(doc, 0);
                        //se.Geometry = shape;
                        //se.Create(doc2.ShapesFolderEntity);

                    }
                }
                UndoSequence.End();
            }
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
            get { return new Guid("8C5F0890-91DE-43CE-A10C-58A26774DF8D"); }
        }
    }
}
