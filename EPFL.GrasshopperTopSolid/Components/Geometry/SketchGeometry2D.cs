using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.D3.Sketches;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Pdm;
using G = TopSolid.Kernel.G;
using DB = TopSolid.Kernel.DB;


namespace EPFL.GrasshopperTopSolid.Components.Geometry
{
    public class SketchGeometry2D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SketchGeometry class.
        /// </summary>
        public SketchGeometry2D()
          : base("SketchGeometry2D", "SKetchGeometry2D",
              "Gets geometries of a 2D Skecth",
              "TopSolid", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Document", "Doc", "TopSolid Document containing Sketch", GH_ParamAccess.item);
            pManager.AddGenericParameter("Sketch", "SK", "2D Sketch to get points and profiles", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Profiles", "Profiles", "Profiles as List", GH_ParamAccess.list);
            pManager.AddGenericParameter("Points", "Pts", "Points as List", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ModelingDocument modellingDocument = null;
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();

            if (DA.GetData("Document", ref wrapper))
            {
                if (wrapper.Value is string || wrapper.Value is GH_String)
                {
                    modellingDocument = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault() as ModelingDocument;
                }
                else if (wrapper.Value is IDocumentItem)
                    modellingDocument = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as ModelingDocument;
                else if (wrapper.Value is IDocument)
                    modellingDocument = wrapper.Value as ModelingDocument;
            }

            if (modellingDocument is null) return;

            G.D2.Sketches.Sketch sketch = null;
            if (DA.GetData("Sketch", ref wrapper))
            {
                if (wrapper.Value is string || wrapper.Value is GH_String)
                {
                    sketch = (modellingDocument.SketchesFolderEntity.SearchDeepEntity(wrapper.Value.ToString()) as DB.D2.Sketches.SketchEntity)?.Geometry;
                }

                else if (wrapper.Value is DB.D2.Sketches.SketchEntity entity)
                    sketch = entity.Geometry;

                else if (wrapper.Value is G.D2.Sketches.Sketch sk)
                    sketch = sk as G.D2.Sketches.Sketch;
            }

            if (sketch == null)
                return;

            DA.SetDataList("Profiles", sketch.Profiles);
            DA.SetDataList("Profiles", sketch.Vertices.Select(x => x.Geometry));

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.PlanarSketchToRollUpBuildingOperation, 24, 24).ToBitmap();

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("78C28185-2480-4203-A57D-DC60351B71B3"); }
        }
    }
}