using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using TopSolid.Kernel.DB.D2.Curves;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.D3.Planes;
using TopSolid.Kernel.DB.D3.Points;
using TopSolid.Kernel.DB.D3.Sketches;
using TopSolid.Kernel.DB.D3.Sketches.Operations;
using TopSolid.Kernel.DB.D3.Sketches.Planar.Operations;
using TopSolid.Kernel.G.D2.Curves;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.G.D3.Sketches;
using TopSolid.Kernel.TX.Items;
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
        int SKNumber = 0;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "crvs", "curves to send", GH_ParamAccess.item);
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

            Rhino.Geometry.Curve rc = null;
            Rhino.Geometry.Plane rhPlane = new Rhino.Geometry.Plane();
            bool run = false;
            string name = "";
            DA.GetData("Base Plane", ref rhPlane);
            DA.GetData("Sketch Name", ref name);

            if (!DA.GetData("Curve", ref rc))
                return;
            if (rc == null) return;

            DA.GetData("Create", ref run);

            //GeometricDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;
            ModelingDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as ModelingDocument;
            UndoSequence.UndoCurrent();
            UndoSequence.Start("Bake", true);

            if (run == true)
            {

                Double tol = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

                PlanarSketchOperationMaker maker = new PlanarSketchOperationMaker(TopSolid.Kernel.SX.Version.Current);
                maker.Plane = new BasicSmartPlane(null, new BoundedPlane(rhPlane.ToHost(), TopSolid.Kernel.G.D2.Extent.UnitCentered));
                //maker.Origin = new BasicSmartPoint(null, r);
                maker.Document = doc;


                maker.Make();
                if (maker.NewSolvingOperation.ChildEntity.Name != name && SKNumber == 0)
                {
                    maker.NewSolvingOperation.ChildEntity.Name = name;
                }

                else
                {
                    maker.NewSolvingOperation.ChildEntity.Name = name + SKNumber.ToString();
                }
                SKNumber++;
                maker.NewSolvingOperation.IsEdited = true;

                //PositionedSketchSolvingOperation sketchOp = new PositionedSketchSolvingOperation((TopSolid.Kernel.DB.D3.Sketches.Documents.GeometricDocument)doc, 0);
                //var TsPlane = rhPlane.ToHost();
                //PositionedSketchEntity skEntity = new PositionedSketchEntity(doc, 0);

                //PositionedSketch sketch = new PositionedSketch(skEntity, ItemOperationKey.BasicKey, false);
                //sketch.Frame = rhPlane.ToHost(rhPlane.XAxis.ToHost(), rhPlane.YAxis.ToHost(), rhPlane.ZAxis.ToHost());

                if (!rc.IsPlanar())
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Non planar Curves, Curves will be projected onto plane");
                    rc = Rhino.Geometry.Curve.ProjectToPlane(rc, rhPlane);
                }

                if (rc.IsPolyline())
                {
                    TopSolid.Kernel.G.D3.Sketches.Planar.PlanarSketch plSketch = maker.NewSolvingOperation.ChildEntity.Geometry;
                    ItemOperationKey key = new ItemOperationKey(maker.NewSolvingOperation.Id);
                    var svex = plSketch.AddVertex(key, rc.PointAtStart.ToHost2d());
                    var abssvex = svex;
                    List<TopSolid.Kernel.G.D2.Sketches.Segment> listseg = new List<TopSolid.Kernel.G.D2.Sketches.Segment>();
                    TopSolid.Kernel.G.D2.Sketches.SegmentList seglist = new TopSolid.Kernel.G.D2.Sketches.SegmentList();
                    int count = 0;
                    var dup = rc.DuplicateSegments();
                    foreach (var seg in dup)
                    {
                        var evex = plSketch.AddVertex(key, seg.PointAtEnd.ToHost2d());
                        if (count == dup.Length - 1)
                            evex = abssvex;

                        Line line = new Line(seg.PointAtStart, seg.PointAtEnd);
                        var segm = plSketch.AddSegment(key, svex, evex, new TopSolid.Kernel.G.D2.Curves.LineCurve(svex.Geometry, evex.Geometry), false);

                        svex = evex;

                        seglist.Add(segm);
                        count++;
                    }

                    plSketch.AddProfile(key, seglist);
                }

                maker.NewSolvingOperation.Update();


                maker.NewSolvingOperation.ChildEntity.AddRollbackMarks();
                maker.NewSolvingOperation.IsEdited = false;
                DA.SetData(0, maker.NewSolvingOperation.ChildEntity);
            }


            UndoSequence.End();

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