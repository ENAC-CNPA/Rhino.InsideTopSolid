using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.G.D3.Curves;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.G.D3.Shapes.Creations;
using TopSolid.Kernel.SX.Collections;
using SX = TopSolid.Kernel.SX;
using G = TopSolid.Kernel.G;
using TopSolid.Kernel.G.D3.Surfaces;
using Grasshopper;

namespace EPFL.GrasshopperTopSolid.Components.Test_Debug
{
    public class GetShapeTopology : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetShapeCurves class.
        /// </summary>
        public GetShapeTopology()
          : base("GetShapeCurves", "crvs",
              "Gets TopSolid shape curves to debug",
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
            pManager.AddGeometryParameter("Rhino3DCurves", "3D", "Converted Rhino 3D Curves", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Rhino2DCurves", "2D", "Converted Rhino 2D Curves", GH_ParamAccess.list);
            pManager.AddGeometryParameter("RhinoSurface", "Srf", "Converted Rhino Surface", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Control Points", "Cpts", "Converted Control Points", GH_ParamAccess.tree);
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
            //List<Brep> list = new List<Brep>();
            List<Rhino.Geometry.Curve> list3D = new List<Rhino.Geometry.Curve>();
            List<Rhino.Geometry.Curve> list2D = new List<Rhino.Geometry.Curve>();
            List<Rhino.Geometry.Surface> listSurfaces = new List<Rhino.Geometry.Surface>();

            ShapeEntity entity = document.RootEntity.SearchDeepEntity(_name) as ShapeEntity;
            if (entity is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"no valid shapes found");
                return;
            }

            double tol = TopSolid.Kernel.G.Precision.ModelingLinearTolerance;
            Shape shape = entity.Geometry;
            BoolList boolList = new BoolList();
            SX.Collections.Generic.List<G.D3.Curves.IGeometricProfile> list3dprofiles = new SX.Collections.Generic.List<IGeometricProfile>();
            SX.Collections.Generic.List<G.D2.Curves.IGeometricProfile> list2dprofiles = new SX.Collections.Generic.List<G.D2.Curves.IGeometricProfile>();
            SX.Collections.Generic.List<EdgeList> edgeList = new SX.Collections.Generic.List<EdgeList>();
            DataTree<Point3d> pointTree = new DataTree<Point3d>();
            foreach (Face face in shape.Faces)
            {
                OrientedSurface oSurf = face.GetOrientedBsplineTrimmedGeometry(tol, false, false, false, boolList, list2dprofiles, list3dprofiles, edgeList);
                list2D.AddRange(list2dprofiles.SelectMany(x => x.Segments.Select(y => y.Curve.ToRhino())).ToList());
                list3D.AddRange(list3dprofiles.SelectMany(x => x.Segments.Select(y => y.GetOrientedCurve().Curve.ToRhino())).ToList());
                listSurfaces.Add(oSurf.Surface.ToRhino());
                pointTree.Branches.Add((oSurf.Surface as BSplineSurface).CPts.Select(x => x.ToRhino()).ToList());
            }

            DA.SetDataList("Rhino3DCurves", list3D);
            DA.SetDataList("Rhino2DCurves", list2D);
            DA.SetDataList("RhinoSurface", listSurfaces);
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
            get { return new Guid("4D820C5C-84ED-450D-9952-B54BBE95F72B"); }
        }
    }
}