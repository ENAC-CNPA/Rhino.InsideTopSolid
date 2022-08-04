using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.G.D3.Shapes;
using TKG = TopSolid.Kernel.G;

namespace EPFL.GrasshopperTopSolid.Components.Geometry
{
    public class TSDeconstructShape : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TSDeconstructShape class.
        /// </summary>
        public TSDeconstructShape()
          : base("TSDeconstructShape", "TSDeconstruct",
              "Deconstructs a TopSolid Shape into its Topological components",
              "TopSolid", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("TSShape", "TSShape", "TopSolid Shape to deconstruct", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Faces", "Faces", "Faces as List", GH_ParamAccess.list);
            pManager.AddGenericParameter("Edges", "Edges", "Edges as List", GH_ParamAccess.list);
            pManager.AddGenericParameter("Vertices", "Vertices", "Vertices as List", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();
            DA.GetData("TSShape", ref wrapper);
            TKG.IGeometry tsGeometry = null;
            ModelingDocument currentDocument = TopSolid.Kernel.UI.Application.CurrentDocument as ModelingDocument;
            if (wrapper.Value is Entity ent) tsGeometry = ent.Geometry;
            else if (wrapper.Value is TKG.IGeometry geometry) tsGeometry = geometry;
            else if (wrapper.Value is GH_String || wrapper.Value is string)
            {
                tsGeometry = currentDocument.RootEntity.SearchDeepEntity(wrapper.Value.ToString())?.Geometry;
            }

            else
            {
                tsGeometry = wrapper.Value as TKG.IGeometry;
            }

            if (tsGeometry == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Couldn't get Geometry, make sure input is either TS Shape or Shape Entity");
                return;
            }

            if (!(tsGeometry is TKG.D3.Shapes.Shape)) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Geometry is not a shape, make sure input is either TS Shapes or Shape Entity");

            Shape shape = tsGeometry as Shape;
            if (shape is null) return;

            DA.SetDataList("Vertices", shape.Vertices.Select(x => x.GetGeometry().ToRhino()));
            DA.SetDataList("Edges", shape.Edges.Select(x => x.GetGeometry(true).ToRhino()));

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
            get { return new Guid("31445A82-25F6-490F-ABD8-A1FDED5522B9"); }
        }
    }
}