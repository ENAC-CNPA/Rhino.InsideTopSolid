using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry.Delaunay;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.DB.D3.Curves;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.D3.Surfaces;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.TX.Undo;

namespace EPFL.GrasshopperTopSolid.Components.Test_Debug
{
    public class BakeExploded : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BakeExploded class.
        /// </summary>
        public BakeExploded()
          : base("BakeExploded", "BakeExp",
              "Bake expoded entities in TopSolid",
              "TopSolid", "Test-Debug")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "Brep", "Brep to bake", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Go", "Bake Brep exploded in TopSolid", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void BeforeSolveInstance()
        {
            if (UndoSequence.Current != null)
            {
                UndoSequence.End();
            }

            UndoSequence.Start("Grasshopper bake debug", false);
            base.BeforeSolveInstance();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            PartDocument partDocument = TopSolid.Kernel.UI.Application.CurrentDocument as PartDocument;
            Brep brep = new Brep();
            bool run = false;

            if (partDocument is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Current Document is not Part Document");
                return;
            }
            DA.GetData(0, ref brep);
            DA.GetData(1, ref run);

            if (run)
            {

                CreateSurfacesinTopSolid(brep, partDocument);

            }

            UndoSequence.End();
        }

        protected override void AfterSolveInstance()
        {
            if (UndoSequence.Current != null)
                UndoSequence.End();
            base.AfterSolveInstance();
        }

        private void CreateSurfacesinTopSolid(Brep brep, PartDocument partDocument)
        {
            ShapesFolderEntity UntrimmedSurfacesFolder = partDocument.ShapesFolderEntity.SearchEntity("Untrimmed Surfaces") as ShapesFolderEntity;
            if (UntrimmedSurfacesFolder is null)
            {

                UntrimmedSurfacesFolder = new ShapesFolderEntity(partDocument, 0);

                UntrimmedSurfacesFolder.Name = "Untrimmed Surfaces";
                UntrimmedSurfacesFolder.Create(partDocument.ShapesFolderEntity);

            }


            ShapesFolderEntity trimmedSurfacesFolder = partDocument.ShapesFolderEntity.SearchEntity("trimmed Surfaces") as ShapesFolderEntity;
            if (trimmedSurfacesFolder is null)
            {
                trimmedSurfacesFolder = new ShapesFolderEntity(partDocument, 0);

                trimmedSurfacesFolder.Name = "trimmed Surfaces";
                trimmedSurfacesFolder.Create(partDocument.ShapesFolderEntity);
            }



            CurvesFolderEntity curves3DFolderEntity = partDocument.RootEntity.SearchEntity("Curves3D") as CurvesFolderEntity;
            if (curves3DFolderEntity is null)
            {
                curves3DFolderEntity = new CurvesFolderEntity(partDocument, 0);
                curves3DFolderEntity.Name = "Curves3D";
                curves3DFolderEntity.Create(partDocument.RootEntity);
            }

            CurvesFolderEntity curves2DFolderEntity = partDocument.RootEntity.SearchEntity("Curves2D") as CurvesFolderEntity;
            if (curves2DFolderEntity is null)
            {
                curves2DFolderEntity = new CurvesFolderEntity(partDocument, 0);
                curves2DFolderEntity.Name = "Curves2D";
                curves2DFolderEntity.Create(partDocument.RootEntity);
            }

            foreach (var face in brep.Faces)
            {
                Brep faceBRep = face.ToBrep();
                SurfaceEntity surfaceEntity = new SurfaceEntity(partDocument, 0)
                {
                    Geometry = face.UnderlyingSurface().ToHost(),
                    Name = "face" + face.FaceIndex,
                    ExplicitColor = TopSolid.Kernel.SX.Drawing.Color.Red,
                };

                ShapeEntity trimmedSurfaceEntity = new ShapeEntity(partDocument, 0)
                {
                    Geometry = faceBRep.ToHost(),
                    Name = "TrimmedSurface" + face.FaceIndex,
                    ExplicitColor = TopSolid.Kernel.SX.Drawing.Color.Green,
                };

                surfaceEntity.Create(UntrimmedSurfacesFolder);
                trimmedSurfaceEntity.Create(trimmedSurfacesFolder);

                CurvesFolderEntity faceCurves3DFolder = new CurvesFolderEntity(partDocument, 0);
                faceCurves3DFolder.Name = "face " + face.FaceIndex + " 3D";
                faceCurves3DFolder.Create(curves3DFolderEntity);
                CurvesFolderEntity faceCurves2DFolder = new CurvesFolderEntity(partDocument, 0);
                faceCurves2DFolder.Name = "face " + face.FaceIndex + " 2D";
                faceCurves2DFolder.Create(curves2DFolderEntity);

                MakeCurves2D(face, faceBRep, faceCurves2DFolder, partDocument);
                MakeCurves3D(face, faceBRep, faceCurves3DFolder, partDocument);


            }


        }

        private void MakeCurves2D(BrepFace face, Brep brep, CurvesFolderEntity curves2DFolderEntity, PartDocument partDocument)
        {
            foreach (BrepFace brepFace in brep.Faces)
            {
                foreach (BrepLoop loop in brep.Loops)
                {
                    CurvesFolderEntity loopFolder = new CurvesFolderEntity(partDocument, 0);
                    //loopFolder.Name = "face " + face.FaceIndex + " - loop 2D - " + loop.LoopIndex;
                    loopFolder.Create(curves2DFolderEntity);
                    foreach (var trim in loop.Trims)
                    {
                        TopSolid.Kernel.G.D2.Curves.Curve topCurve = trim.TrimCurve.ToHost2d();
                        TopSolid.Kernel.DB.D2.Curves.CurveEntity curveEntity = new TopSolid.Kernel.DB.D2.Curves.CurveEntity(partDocument, 0)
                        {
                            Geometry = topCurve,
                            //Name = "loop " + loop.LoopIndex + " - curve 2D - "+ trim.TrimCurveIndex + '-' + trim.TrimIndex,
                            ExplicitColor = TopSolid.Kernel.SX.Drawing.Color.LightBlue,
                        };

                        curveEntity.Create();
                        loopFolder.AddEntity(curveEntity);
                    }
                }
            }
        }
        private void MakeCurves3D(BrepFace face, Brep brep, CurvesFolderEntity curves3DFolderEntity, PartDocument partDocument)
        {
            foreach (BrepFace brepFace in brep.Faces)
            {
                foreach (BrepLoop loop in brep.Loops)
                {
                    CurvesFolderEntity loopFolder = new CurvesFolderEntity(partDocument, 0);
                    //loopFolder.Name = "face " + face.FaceIndex + " - loop 3D - " + loop.LoopIndex;
                    loopFolder.Create(curves3DFolderEntity);
                    foreach (var trim in loop.Trims)
                    {
                        TopSolid.Kernel.G.D3.Curves.Curve topCurve = trim.TrimCurve.ToHost();
                        TopSolid.Kernel.DB.D3.Curves.CurveEntity curveEntity = new TopSolid.Kernel.DB.D3.Curves.CurveEntity(partDocument, 0)
                        {
                            Geometry = topCurve,
                            //Name = "loop " + loop.LoopIndex + " - curve 3D - " + trim.TrimCurveIndex + '-' + trim.TrimIndex,
                            ExplicitColor = TopSolid.Kernel.SX.Drawing.Color.LightBlue,
                        };

                        curveEntity.Create();
                        loopFolder.AddEntity(curveEntity);
                    }
                }
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
            get { return new Guid("F9E4EF63-00D7-436A-A638-E10DB0F20AEC"); }
        }
    }
}