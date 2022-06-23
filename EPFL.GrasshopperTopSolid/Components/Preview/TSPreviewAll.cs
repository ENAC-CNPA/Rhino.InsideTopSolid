using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using TopSolid.Kernel.GR.Displays;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.GR.D3;
using TopSolid.Kernel.GR.Attributes;
using TopSolid.Kernel.SX.Drawing;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.GR.Algorithms;


namespace EPFL.GrasshopperTopSolid.Components
{
    public abstract class TSPreviewAll : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TSPreview class.
        /// </summary>
        public TSPreviewAll()
          : base("TopSolid Preview All", "TSPreviewAll",
              "Preview All Geometries in TopSolid",
              "TopSolid", "Preview")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Preview?", "P?", "Preview active geometries", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        private GeometricDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;
        private GeneralDisplay gd = new GeneralDisplay(null);
        private static GH_Document ActiveDefinition => Instances.ActiveCanvas?.Document;

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

            Boolean flag = false;
            if (!DA.GetData(0, ref flag)) { return; }


            if (!doc.Display.ContainsDisplay(gd))
            {
                doc.Display.AddDisplay(gd);
            }

            if (flag)
            {
                ActiveDefinition.SolutionEnd += ActiveDefinition_SolutionEnd;
            }
            else { gd.Clear(); }
        }

        private void ActiveDefinition_SolutionEnd(object sender, GH_SolutionEventArgs e)
        {
            gd.Clear();
            foreach (var obj in ActiveDefinition.Objects.OfType<IGH_ActiveObject>())
            {
                if (obj.Locked)
                    continue;

                if (obj is IGH_PreviewObject previewObject)
                {
                    if (previewObject.IsPreviewCapable)
                    {
                        if (obj is IGH_Component component)
                        {
                            foreach (var param in component.Params.Output)
                            {
                                if (param is IGH_PreviewObject preview)
                                    DrawData(param.VolatileData);
                            }
                        }
                        else if (obj is IGH_Param param)
                        {
                            DrawData(param.VolatileData);
                        }
                    }
                }
            }
        }

        void DrawData(Grasshopper.Kernel.Data.IGH_Structure volatileData)
        {
            if (!volatileData.IsEmpty)
            {
                foreach (var value in volatileData.AllData(true))
                {
                    // First check for IGH_PreviewData to discard no graphic elements like strings, doubles, vectors...
                    if (value is IGH_PreviewData)
                    {
                        switch (value.ScriptVariable())
                        {
                            case Point3d point:
                                var tp = point.ToHost();
                                MarkerItem mi = new MarkerItem(tp);
                                mi.Color = Color.Green;
                                mi.MarkerStyle = MarkerStyle.ExtraLargePlus;
                                gd.Add(mi);
                                break;
                            case Line line:
                                var tl = line.ToHost();
                                LineItem li = new LineItem(tl.Ps, tl.Pe);
                                li.Color = Color.Green;
                                li.LineStyle = LineStyle.SolidMedium;
                                gd.Add(li);
                                break;
                                //case NurbsSurface srf:
                                //    var ts = srf.ToHost();
                                //    ShapeBuilder sb = new ShapeBuilder();
                                //    ShapeItem si = new ShapeItem();
                                //    si.Shape = ts;
                                //    break;                                
                        }
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
            get { return new Guid("00297A7D-1D7F-4DB5-8B00-7F7A52F64BFA"); }
        }
    }
}