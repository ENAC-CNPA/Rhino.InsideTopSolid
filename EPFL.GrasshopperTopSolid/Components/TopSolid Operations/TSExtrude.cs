using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using TopSolid.Kernel.DB.D3.Directions;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.D3.Profiles;
using TopSolid.Kernel.DB.D3.Sections;
using TopSolid.Kernel.DB.D3.Shapes.Extruded;
using TopSolid.Kernel.DB.D3.Sketches.Planar;
using TopSolid.Kernel.DB.D3.Sketches.Planar.Operations;
using TopSolid.Kernel.DB.Operations;
using TopSolid.Kernel.DB.Parameters;
using TopSolid.Kernel.G.D3.Curves;
using TopSolid.Kernel.G.D3.Shapes.Extruded;
using TopSolid.Kernel.TX.Items;
using TopSolid.Kernel.TX.Undo;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_Operations
{
    public class TSExtrude : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TSExtrude class.
        /// </summary>
        public TSExtrude()
          : base("TSExtrude", "TSExtrude",
              "Creates an Extrusion in TopSolid out of planar Curves",
              "TopSolid", "TopSolid Operations")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Sketch", "Sk", "Sketches to extrude", GH_ParamAccess.item);
            pManager.AddNumberParameter("Extrusion Length", "H", "Length for extrusion", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Extruded Forms", "Forms", "resulting TopSolid Extruded Forms", GH_ParamAccess.item);

        }
        ModelingDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as ModelingDocument;
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            UndoSequence.UndoCurrent();
            UndoSequence.Start("Extrude", true);

            TopSolid.Kernel.DB.D3.Sketches.Planar.PlanarSketchEntity sketchEntity = null;
            double length = 0;
            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
            DA.GetData(0, ref obj);

            if (obj != null)
                sketchEntity = obj.Value as PlanarSketchEntity;
            if (sketchEntity == null) return;
            if (!DA.GetData(1, ref length)) return;
            if (sketchEntity == null) return;





            OperationList list = new OperationList();
            doc.RootOperation.GetDeepOperations(list);
            ExtrudedCreation op = null;
            if (list != null && list.Count != 0)

            {
                op = doc.RootOperation.SearchDeepOperation(typeof(ExtrudedCreation)) as ExtrudedCreation;
                if (op != null)
                {
                    op.FirstSide.Length = new BasicSmartReal(null, length, TopSolid.Kernel.TX.Units.UnitType.Length, doc);
                    return;
                }


            }


            //Creation of the extruded operation
            ExtrudedCreation extruded = new ExtrudedCreation(doc, 0);

            ////Parameters are given to the operation
            //Section
            extruded.Section = new ProvidedSmartSection(null, sketchEntity, ItemLabel.Empty, true);
            extruded.Section = new ProvidedSmartSection(null, sketchEntity, ItemLabel.Empty, true);

            //Direction
            extruded.Direction = new BasicSmartDirection(null, sketchEntity.Geometry.Plane.Vz, sketchEntity.Geometry.Plane.Po);

            //Limitation : Here the first side is set as a length, so it is needed to instanciate it as a length before giving it a value
            extruded.FirstSide.Type = BoundType.Length;


            extruded.FirstSide.Length = new BasicSmartReal(null, length, TopSolid.Kernel.TX.Units.UnitType.Length, doc);



            //The operation is created
            extruded.Create();

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

        protected override void BeforeSolveInstance()
        {



            base.BeforeSolveInstance();
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("413CE252-568B-43ED-BE21-04DF0DCCAEB0"); }
        }
    }
}