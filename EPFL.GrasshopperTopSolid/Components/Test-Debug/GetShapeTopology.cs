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
using Grasshopper.Kernel.Geometry.Delaunay;

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
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Rhino3DCurves", "3D", "Converted Rhino 3D Curves", GH_ParamAccess.tree);
            pManager.AddGeometryParameter("Rhino2DCurves", "2D", "Converted Rhino 2D Curves", GH_ParamAccess.tree);
            pManager.AddGeometryParameter("RhinoSurface", "Srf", "Converted Rhino Surface", GH_ParamAccess.tree);
            pManager.AddGeometryParameter("Control Points", "Cpts", "Converted Control Points", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Control Points+weight", "CPtsW", "Control points with Weights", GH_ParamAccess.tree);
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
            //Output Data Trees
            DataTree<Rhino.Geometry.Curve> curve3DTree = new DataTree<Rhino.Geometry.Curve>();
            DataTree<Rhino.Geometry.Curve> curve2dTree = new DataTree<Rhino.Geometry.Curve>();
            DataTree<(double, double, double, double)> cPTsDataTree = new DataTree<(double, double, double, double)>();
            DataTree<Rhino.Geometry.Surface> surfacesDataTree = new DataTree<Rhino.Geometry.Surface>();
            DataTree<Rhino.Geometry.Point3d> pointsDataTree = new DataTree<Point3d>();

            string _name = "";
            if (!DA.GetData("Name", ref _name)) return;
            bool inTs = false;
            DA.GetData("in TopSolid", ref inTs);
            bool forcesRational = false;
            DA.GetData("NonRational", ref forcesRational);

            DesignDocument document = TopSolid.Kernel.UI.Application.CurrentDocument as DesignDocument;

            ShapeEntity entity = document.RootEntity.SearchDeepEntity(_name) as ShapeEntity;
            if (entity is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"no valid shapes found");
                return;
            }

            int faceindex = 0;
            foreach (G.D3.Shapes.Face face in entity.Geometry.Faces)
            {
                SX.Collections.Generic.List<G.D3.Curves.IGeometricProfile> list3dprofiles = new SX.Collections.Generic.List<IGeometricProfile>();
                SX.Collections.Generic.List<G.D2.Curves.IGeometricProfile> list2dprofiles = new SX.Collections.Generic.List<G.D2.Curves.IGeometricProfile>();
                SX.Collections.Generic.List<G.D3.Shapes.EdgeList> edgeList = new SX.Collections.Generic.List<G.D3.Shapes.EdgeList>();
                BoolList boolList = new BoolList();
                OrientedSurface oSurf;

                bool forcesNonPeriodic = false;
                if (face.GeometryType == G.D3.SurfaceGeometryType.Cone || face.GeometryType == G.D3.SurfaceGeometryType.Cylinder)
                    forcesNonPeriodic = true;

                oSurf = face.GetOrientedBsplineTrimmedGeometry(G.Precision.ModelingLinearTolerance, forcesRational, false, forcesNonPeriodic, boolList, list2dprofiles, list3dprofiles, edgeList);

                FillInOutputDataTrees(oSurf, curve2dTree, curve3DTree, surfacesDataTree, pointsDataTree, cPTsDataTree, list3dprofiles, list2dprofiles, faceindex);

                //To create topological elements as TopSolid Entities
                if (inTs)
                    CreateinTopSolid(oSurf, list3dprofiles, list2dprofiles, face, document);

                faceindex++;
            }

            DA.SetDataTree(0, curve3DTree);
            DA.SetDataTree(1, curve2dTree);
            DA.SetDataTree(2, surfacesDataTree);
            DA.SetDataTree(3, pointsDataTree);
            DA.SetDataTree(4, cPTsDataTree);
        }

        private void FillInOutputDataTrees(OrientedSurface oSurf, DataTree<Rhino.Geometry.Curve> curve2dTree, DataTree<Rhino.Geometry.Curve> curve3DTree, DataTree<Rhino.Geometry.Surface> surfacesDataTree, DataTree<Point3d> pointsDataTree, DataTree<(double, double, double, double)> cPTsDataTree, SX.Collections.Generic.List<IGeometricProfile> list3dprofiles, SX.Collections.Generic.List<G.D2.Curves.IGeometricProfile> list2dprofiles, int faceindex)
        {
            int counter = 0;
            var currentPath = new GH_Path(faceindex);
            foreach (var profile in list2dprofiles)
            {
                var curves = profile.Segments.Select(x => x.Curve.ToRhino());
                currentPath = new GH_Path(faceindex, counter++);
                curve2dTree.AddRange(curves, currentPath);
            }
            counter = 0;

            foreach (var profile in list3dprofiles)
            {
                var curves = profile.Segments.Select(y => y.GetOrientedCurve().Curve.ToRhino());
                currentPath = new GH_Path(faceindex, counter++);
                curve3DTree.AddRange(curves, currentPath);
            }
            counter = 0;

            currentPath = new GH_Path(faceindex);
            surfacesDataTree.AddRange(new[] { oSurf.Surface.ToRhino() }, currentPath);

            BSplineSurface bsplineSurf = oSurf.Surface as BSplineSurface;
            pointsDataTree.AddRange(bsplineSurf.CPts.Select(x => x.ToRhino()), currentPath);

            IEnumerable<double> weights;
            if (bsplineSurf != null && !bsplineSurf.CWts.IsEmpty && bsplineSurf.CWts.Count != 0)
            {
                weights = bsplineSurf.CWts.Select(x => x);
            }

            else
            {
                weights = bsplineSurf.CPts.Select(x => 1.0);
            }

            cPTsDataTree.AddRange(bsplineSurf.CPts.Zip(weights, (x, y) => (x.X, x.Y, x.Z, y)), currentPath);
        }

        private void CreateinTopSolid(OrientedSurface oSurf, SX.Collections.Generic.List<IGeometricProfile> list3dprofiles, SX.Collections.Generic.List<G.D2.Curves.IGeometricProfile> list2dprofiles, G.D3.Shapes.Face face, DesignDocument document)
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

            DB.D3.Profiles.ProfileEntity profileEntity;
            foreach (var item in list3dprofiles)
            {
                profileEntity = new DB.D3.Profiles.ProfileEntity(document, 0)
                {
                    Name = $"face:{face.Id},curve:{curveCounter++}",
                    Geometry = item,

                };
                profileEntity.Create(curvesFolderEntity);
            }

            DB.D2.Profiles.ProfileEntity profileEntity2D;
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