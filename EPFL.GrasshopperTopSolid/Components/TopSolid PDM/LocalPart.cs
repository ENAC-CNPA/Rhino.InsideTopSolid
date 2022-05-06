using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Cad.Design.DB.Local.Operations;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.D3.Shapes.Sew;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.DB.Operations;
using TopSolid.Kernel.DB.Parameters;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.TX.Undo;
using TopSolid.Kernel.TX.Units;
using TK = TopSolid.Kernel;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_PDM
{
    public class LocalPart : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the LocalPart class.
        /// </summary>
        public LocalPart()
          : base("LocalPart", "LPart",
              "Creates Local Part in Assembly",
              "TopSolid", "TopSolid PDM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "part name", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Geometry", "Geo", "Geometry To convert to Part", GH_ParamAccess.item);
            //pManager.AddGenericParameter("Geometry", "Geo", "Geometry To Convert", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            UndoSequence.UndoCurrent();
            UndoSequence.Start("part", true);

            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
            IGH_GeometricGoo geo = null;
            if (!DA.GetData(1, ref geo)) return;

            //if (obj == null) return;

            //shape = obj.Value as Shape;

            GH_Brep gbrep = null;
            Brep brep = null;
            string name = "";
            DA.GetData(0, ref name);
            DA.GetData(1, ref gbrep);
            AssemblyDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as AssemblyDocument;
            if (!GH_Convert.ToBrep(gbrep, ref brep, GH_Conversion.Both)) return;
            ShapeList shape = brep.ToHost();
            EntityList list = new EntityList();
            foreach (var s in shape)
            {
                ShapeEntity se = new ShapeEntity(doc, 0);
                se.Geometry = s;
                se.Create();
                list.Add(se);
            }


            var part = CreateLocalPart(doc, list, name);
            doc.PartsFolderEntity.AddEntity(part);

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

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A486E72F-0FF1-47EC-B825-DE499CC3EA6E"); }
        }

        private PartEntity CreateLocalPart(AssemblyDocument inAssemblyDocument, EntityList inEntities, string inName)
        {
            PartEntity localPart = new PartEntity(inAssemblyDocument, 0);
            localPart.SetLocalConstituents(inEntities);

            // Make default parameters.

            //localPart.MakeDefaultParameters(TK.SX.Version.Current, true);
            localPart.MakeDefaultParameters(true);
            if (!string.IsNullOrEmpty(inName))
                localPart.NameParameterValue = inAssemblyDocument.MakeLocalizableString(inName);

            foreach (Entity entity in inEntities)
            {
                localPart.AddEntityToLocalRepresentation(entity, TopSolid.Cad.Design.DB.Documents.ElementName.DetailedRepresentation);
                localPart.AddEntityToLocalRepresentation(entity, TopSolid.Cad.Design.DB.Documents.ElementName.DesignRepresentation);
                localPart.AddEntityToLocalRepresentation(entity, TopSolid.Cad.Design.DB.Documents.ElementName.SimplifiedRepresentation);
            }

            return localPart;
        }
    }
}