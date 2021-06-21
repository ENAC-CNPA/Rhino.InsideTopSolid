using Grasshopper.Kernel;
using System;
using System.Linq;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Sketches;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_Entities
{
    public class GetCurve : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetCurve class.
        /// </summary>
        public GetCurve()
          : base("GetCurve", "GetCrv",
              "Gets a Curve out of a TopSolid Sketch",
              "Category", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of Sketch containing Curve", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("RhinoCurve", "RhCrv", "Converted Rhino Curve", GH_ParamAccess.item);
            pManager.AddGenericParameter("TopSolidCurve", "TSCrv", "TopSolid Bspline", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string _name = "";
            DA.GetData("Name", ref _name);
            GeometricDocument document = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;

            PositionedSketchEntity entity = document.RootEntity.SearchDeepEntity(_name) as PositionedSketchEntity;

            if (entity.Geometry.Profiles.Count() != 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Sketch Contains {entity.Geometry.Profiles.Count()} curves, it must contain 1 curve");
                return;
            }
            Rhino.Geometry.Curve crv = Convert.ToRhino(entity.Geometry.Profiles.First());
            DA.SetData("RhinoCurve", crv);
            DA.SetData("TopSolidCurve", entity.Geometry.Profiles.First());
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.TopSolid;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("bcc6e9dc-68ae-45e6-b5a5-f2ae3fc1b16d"); }
        }
    }
}
