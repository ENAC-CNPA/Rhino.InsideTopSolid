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
using DB = TopSolid.Kernel.DB;
using TopSolid.Kernel.G.D3.Surfaces;
using Grasshopper;
using TopSolid.Kernel.TX.Undo;
using TopSolid.Kernel.DB.D3.Surfaces;
using TopSolid.Kernel.DB.D3.Curves;
using System.Diagnostics;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using Grasshopper.Kernel.Data;

namespace EPFL.GrasshopperTopSolid.Components.Test_Debug
{
    public class GetShapeTopology : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetShapeCurves class.
        /// </summary>
        public GetShapeTopology()
          : base("GetShapeTopology", "crvs",
              "Gets TopSolid shape Topology to debug",
              "TopSolid", "Test-Debug")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of Shape", GH_ParamAccess.item);
            pManager.AddBooleanParameter("in TopSolid", "inTS", "Create Surfaces and Curves in TopSolid", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddBooleanParameter("NonRational", "nonRat", "forces non rational bspline", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Rhino3DCurves", "3D", "Converted Rhino 3D Curves", GH_ParamAccess.tree);
            pManager.AddGeometryParameter("Rhino2DCurves", "2D", "Converted Rhino 2D Curves", GH_ParamAccess.list);
            pManager.AddGeometryParameter("RhinoSurface", "Srf", "Converted Rhino Surface", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Control Points", "Cpts", "Converted Control Points", GH_ParamAccess.list);
            pManager.AddGenericParameter("Control Points+weight", "CPtsW", "Control points with Weights", GH_ParamAccess.list);
        }

        protected override void BeforeSolveInstance()
        {
            if (UndoSequence.Current != null)
                UndoSequence.End();
            UndoSequence.Start("Grasshopper GetTopology", true);
            base.BeforeSolveInstance();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string _name = "";
            bool inTs = false;
            bool forcesRational = false;
            DA.GetData("NonRational", ref forcesRational);
            DA.GetData("in TopSolid", ref inTs);
            if (!DA.GetData("Name", ref _name)) return;
            DesignDocument document = TopSolid.Kernel.UI.Application.CurrentDocument as DesignDocument;

            //List<Rhino.Geometry.Curve> list3D = new List<Rhino.Geometry.Curve>();
            DataTree<Rhino.Geometry.Curve> curve3DTree = new DataTree<Rhino.Geometry.Curve>();


            List<Rhino.Geometry.Curve> list2D = new List<Rhino.Geometry.Curve>();
            DataTree<Rhino.Geometry.Curve> curve2dTree = new DataTree<Rhino.Geometry.Curve>();

            List<(double, double, double, double)> cPtsList = new List<(double, double, double, double)>();
            DataTree<(double, double, double, double)> cPTsDataTree = new DataTree<(double, double, double, double)>();


            List<Rhino.Geometry.Surface> listSurfaces = new List<Rhino.Geometry.Surface>();
            DataTree<Rhino.Geometry.Surface> surfacesDataTree = new DataTree<Rhino.Geometry.Surface>();


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
            List<Point3d> pointList = new List<Point3d>();
            OrientedSurface oSurf;
            //UndoSequence.UndoCurrent();
            //UndoSequence.Start("Debug RHiTS", false);
            int faceindex = 0;
            int counter = 0;
            foreach (Face face in shape.Faces)
            {
                var currentPath = new GH_Path(faceindex);

                bool forcesNonPeriodic = false;
                if (face.GeometryType == G.D3.SurfaceGeometryType.Cone || face.GeometryType == G.D3.SurfaceGeometryType.Cylinder)
                    forcesNonPeriodic = true;


                oSurf = face.GetOrientedBsplineTrimmedGeometry(tol, forcesRational, false, forcesNonPeriodic, boolList, list2dprofiles, list3dprofiles, edgeList);


                list2D.AddRange(list2dprofiles.SelectMany(x => x.Segments.Select(y => y.Curve.ToRhino())).ToList());

                //list3D.AddRange(list3dprofiles.SelectMany(x => x.Segments.Select(y => y.GetOrientedCurve().Curve.ToRhino())).ToList());
                foreach (var profile in list3dprofiles)
                {
                    var curves = profile.Segments.Select(y => y.GetOrientedCurve().Curve.ToRhino()).ToList();
                    currentPath = new GH_Path(faceindex, counter++);
                    curve3DTree.AddRange(curves, currentPath);
                }
                counter = 0;

                TopSolid.Kernel.G.D3.Surfaces.Surface surf;
                //if (face.GeometryType == G.D3.SurfaceGeometryType.Cone)
                //    surf = oSurf.Surface as ConeSurface;
                //else
                //    surf = oSurf.Surface;
                listSurfaces.Add(oSurf.Surface.ToRhino());
                BSplineSurface bsplineSurf = oSurf.Surface as BSplineSurface;
                pointList = bsplineSurf.CPts.Select(x => x.ToRhino()).ToList();
                cPtsList = bsplineSurf.CPts.Zip(bsplineSurf.CWts, (x, y) => (x.X, x.Y, x.Z, y)).ToList();


                if (inTs)
                {
                    int curveCounter = 0;
                    SurfaceEntity surfEntity = new SurfaceEntity(document, 0)
                    {
                        Name = face.Name + face.Id,
                        OrientedGeometry = oSurf,

                    };
                    surfEntity.Create(document.ShapesFolderEntity);
                    CurvesFolderEntity curvesFolderEntity = new CurvesFolderEntity(document, 0);
                    curvesFolderEntity.Create(document.RootEntity);

                    DB.D3.Profiles.ProfileEntity profileEntity = null;
                    foreach (var item in list3dprofiles)
                    {
                        profileEntity = new DB.D3.Profiles.ProfileEntity(document, 0)
                        {
                            Name = $"face:{face.Id},curve:{curveCounter++}",
                            Geometry = item,

                        };
                        profileEntity.Create(curvesFolderEntity);
                    }

                    DB.D2.Profiles.ProfileEntity profileEntity2D = null;
                    foreach (var item in list2dprofiles)
                    {
                        profileEntity2D = new DB.D2.Profiles.ProfileEntity(document, 0)
                        {
                            Name = $"face:{face.Id},curve:{curveCounter++}",
                            Geometry = item as G.D2.Curves.GeometricProfile,

                        };
                        profileEntity2D.Create(curvesFolderEntity);
                    }

                }
                faceindex++;
            }

            DA.SetDataTree(0, curve3DTree);
            DA.SetDataList("Rhino2DCurves", list2D);
            DA.SetDataList("RhinoSurface", listSurfaces);
            DA.SetDataList("Control Points", pointList);
            DA.SetDataList("Control Points+weight", cPtsList);
        }

        protected override void AfterSolveInstance()
        {
            if (UndoSequence.Current != null) UndoSequence.End();
            base.AfterSolveInstance();

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