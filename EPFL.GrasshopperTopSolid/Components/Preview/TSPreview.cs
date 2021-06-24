using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.GR.Attributes;
using TopSolid.Kernel.GR.D3;
using TopSolid.Kernel.GR.Displays;
using TopSolid.Kernel.SX.Drawing;

namespace EPFL.GrasshopperTopSolid.Components
{
    public class TSPreview : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TSPreview class.
        /// </summary>
        public TSPreview()
          : base("TopSolid Preview", "TSPreview",
              "Preview Geometries in TopSolid",
              "TopSolid", "Preview")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometries", "G", "Geometries to display", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        private GeometricDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;
        private GeneralDisplay gd = new GeneralDisplay(null);

        protected override void BeforeSolveInstance()
        {
            gd.Clear();
            base.BeforeSolveInstance();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<IGH_GeometricGoo> geo = new List<IGH_GeometricGoo>();

            if (!DA.GetDataList(0, geo)) { return; }

            if (geo == null) { return; }
            if (geo.Count == 0) { return; }


            if (!doc.Display.ContainsDisplay(gd))
            {
                doc.Display.AddDisplay(gd);
            }


            foreach (var g in geo)
            {
                if (g is GH_Point gp)
                {
                    var rp = new Point3d();
                    GH_Convert.ToPoint3d(gp, ref rp, 0);
                    var tp = rp.ToHost();
                    MarkerItem mi = new MarkerItem(tp);
                    mi.Color = Color.Green;
                    mi.MarkerStyle = MarkerStyle.ExtraLargePlus;
                    gd.Add(mi);
                }
                else if (g is GH_Line gl)
                {
                    var rl = new Line();
                    GH_Convert.ToLine(gl, ref rl, 0);
                    var tl = rl.ToHost();
                    LineItem li = new LineItem(tl.Ps, tl.Pe);
                    li.Color = Color.Green;
                    li.LineStyle = LineStyle.SolidMedium;
                    gd.Add(li);
                }
                //else if (g is GH_Curve gc)
                //{
                //    Curve rc = null;
                //    GH_Convert.ToCurve(gc, ref rc, 0);
                //    rp = rc.ToPolyline(0.01, 0.01, 0.01, 0.05);
                //    var tc = rc.ToHost();

                //}

                else if (g is GH_Surface srf)
                {
                    Rhino.Geometry.Surface _srf = null;
                    GH_Convert.ToSurface(srf, ref _srf, GH_Conversion.Both);
                    var x = Convert.ToHost(_srf.ToNurbsSurface());
                    TopSolid.Kernel.DB.D3.Surfaces.SurfaceEntity srfentity = new TopSolid.Kernel.DB.D3.Surfaces.SurfaceEntity(doc, 0);
                    srfentity.Geometry = x;
                    if (srfentity.Display.Items.Count != 0)

                    {
                        for (int i = 0; i < srfentity.Display.Items.Count; i++)
                        { gd.Add(srfentity.Display.Items.ElementAt(i)); }
                    }


                }
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
            get { return new Guid("99F6EBE4-7343-4F9E-9715-26E3EE51400B"); }
        }
    }
}
