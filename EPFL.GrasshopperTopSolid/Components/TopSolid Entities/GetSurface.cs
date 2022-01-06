using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.G.D3.Surfaces;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_Entities
{
    public class GetSurface : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetSurface class.
        /// </summary>
        public GetSurface()
          : base("GetSurface", "GetSrf",
              "Get a TopSolid Surface of given name",
              "TopSolid", "TopSolid Entities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of Shape-Surface", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("RhinoSurface", "RhSrf", "Converted Rhino Surface", GH_ParamAccess.item);
            pManager.AddGenericParameter("TopSolidSurface", "TSSrf", "TopSolid Bspline Surface", GH_ParamAccess.list);
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
            List<NurbsSurface> list = new List<NurbsSurface>();
            List<BSplineSurface> listTs = new List<BSplineSurface>();

            ShapeEntity entity = document.RootEntity.SearchDeepEntity(_name) as ShapeEntity;
            if (entity.Geometry.Faces.Count() == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Shape Contains {entity.Geometry.Faces.Count()} Faces");
                return;
            }
            else
            {
                foreach (Face f in entity.Geometry.Faces)
                {
                    list.Add(Convert.ToRhino(f.GetBsplineGeometry(TopSolid.Kernel.G.Precision.LinearPrecision, false, false, false) as BSplineSurface).ToNurbsSurface());
                    listTs.Add(f.GetBsplineGeometry(TopSolid.Kernel.G.Precision.LinearPrecision, false, false, false) as BSplineSurface);
                }
            }

            DA.SetDataList("RhinoSurface", list);
            DA.SetDataList("TopSolidSurface", listTs);
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
            get { return new Guid("5b44b36a-a29e-4249-969d-5091892f878b"); }
        }
    }
}
