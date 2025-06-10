using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.DB.D2.Sketches;
using TopSolid.Kernel.DB.D3.Curves;
using TopSolid.Kernel.DB.D3.Points;
using TopSolid.Kernel.DB.D3.Points.Operations;
using TopSolid.Kernel.DB.D3.Profiles;
using TopSolid.Kernel.DB.D3.Sketches.Planar;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.G.D3.Curves;
using GD3 = TopSolid.Kernel.G.D3;
using DBD3 = TopSolid.Kernel.DB.D3;
using TopSolid.Kernel.TX.Undo;

namespace EPFL.GrasshopperTopSolid.Components.Geometry
{
    public class TSNearestPoint : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TSNearestPoint class.
        /// </summary>
        public TSNearestPoint()
          : base("TSNearestPoint", "NPoint",
              "Gets Curve Nearest Point",
              "TopSolid", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Document", "Doc", "Document where curve is, nothing if current", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager.AddGenericParameter("Curve", "Crv", "TopSolid Curve on which to find point", GH_ParamAccess.item);
            pManager.AddGenericParameter("Point", "pt", "TopSolid Point", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Point", "pt", "Point on Curve", GH_ParamAccess.item);
        }

        protected override void BeforeSolveInstance()
        {
            UndoSequence.UndoCurrent();
            UndoSequence.Start("Grasshopper projection", false);
            base.BeforeSolveInstance();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();
            DesignDocument document = null;
            if (DA.GetData(0, ref wrapper) && wrapper != null)
                document = GetTopSolidDocument.GetDesignDocument(wrapper);
            if (document is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Couldn't set TopSolid Document");
                return;
            }

            ProjectedPointCreation pointCreation = new ProjectedPointCreation(document, 0);

            if (DA.GetData("Point", ref wrapper) && wrapper != null && wrapper.Value is PointEntity pEntity)
            {
                GeometricProfile geometricProfile = null;

                if (DA.GetData("Curve", ref wrapper) && wrapper != null)
                {
                    GD3.Point pointOnCurve = new GD3.Point();

                    if (wrapper.Value is GeometricProfile)
                    {
                        geometricProfile = (GeometricProfile)wrapper.Value;
                    }

                    if (wrapper.Value is PlanarSketchEntity planarSketchEntity)
                    {
                        geometricProfile = planarSketchEntity.GetGeometricProfile() as GeometricProfile;
                    }

                    else if (wrapper.Value is ProfileEntity profileEntity)
                    {
                        geometricProfile = profileEntity.GetGeometricProfile() as GeometricProfile;
                    }

                    else if (wrapper.Value is DBD3.Sketches.PositionedSketchEntity sketchEntity)
                    {
                        geometricProfile = sketchEntity.GetGeometricProfile() as GeometricProfile;
                    }

                    if (geometricProfile is null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Couldn't set profile");
                    }

                    double param;
                    pointOnCurve = geometricProfile.FindNearestPoint(pEntity.Geometry, out param);
                    pEntity.Geometry = pointOnCurve;


                    //catch (Exception ex)
                    //{
                    //    Console.WriteLine(ex);
                    //    UndoSequence.Stop();
                    //}

                    DA.SetData(0, pEntity);
                }
            }




        }

        protected override void AfterSolveInstance()
        {
            if (UndoSequence.Current != null)
                UndoSequence.End();
            base.AfterSolveInstance();
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.PointExportOptionsControl_Point, 24, 24).ToBitmap();


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("59A9B0B3-D8A7-423A-9328-94E96CCDE8C7"); }
        }
    }
}