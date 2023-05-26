using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Cad.Design.DB.Local.Operations;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
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
            pManager.AddGenericParameter("Entity", "Ent", "Entities to add to part", GH_ParamAccess.list);
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

            List<Grasshopper.Kernel.Types.GH_ObjectWrapper> obj = new List<Grasshopper.Kernel.Types.GH_ObjectWrapper>();

            if (!DA.GetDataList(1, obj)) return;

            if (obj == null) return;
            EntityList entList = new EntityList();


            foreach (var o in obj)
            {
                var e = o.Value as Entity;
                if (e == null) return;
                entList.Add(e);
            }


            GH_Brep gbrep = null;
            Brep brep = null;
            string name = "";
            DA.GetData(0, ref name);
            AssemblyDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as AssemblyDocument;
            if (doc == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Current TopSolid document must be assembly document");
                return;
            }

            //TODO check this process to transfer Entities from Assembly to LocalPart

            PartEntity localPart = new PartEntity(doc, 0);
            EntityList list = new EntityList();
            foreach (var x in entList)
            {
                list.Add(DuplicateShape(x as ShapeEntity, doc, localPart));
            }

            var part = CreateLocalPart(doc, list, name, localPart);


            //ent.Hide();
            //ent.IsGhost = true;
            //doc.Display.RemoveDisplay(ent.Display);


            TK.SX.Collections.Generic.List<PartEntity> localParts = new TK.SX.Collections.Generic.List<PartEntity>();
            localParts.Add(part);
            LocalPartsCreation op = new LocalPartsCreation(doc, 0);
            op.SetChildParts(localParts);
            op.Create();

            //doc.PartsFolderEntity.AddEntity(part);



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

        private PartEntity CreateLocalPart(AssemblyDocument inAssemblyDocument, EntityList inEntities, string inName, PartEntity localPart)
        {


            localPart.SetLocalConstituents(inEntities);

            // Make default parameters.

            localPart.MakeDefaultParameters(TK.SX.Version.Current, true);
            //localPart.MakeDefaultParameters(true);
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

        private static ShapeEntity DuplicateShape(ShapeEntity shape, AssemblyDocument doc, PartEntity part)
        {
            ShapeEntity newEntity = new ShapeEntity(doc, 0);
            var newShape = shape.Geometry.MakeCopy(part);
            newEntity.Geometry = newShape;
            return newEntity;

        }

        //private unsafe ShapeEntity CreateShapeEntity(int inPart, Transform inTransf, AssemblyDocument inDocument)
        //{
        //    ShapeEntity shapeEntity = new ShapeEntity(inDocument, 0);

        //    PK.ENTITY_t shapeBody;
        //    PK.ENTITY.copy(inPart, &shapeBody);

        //    this.TransformBody(inTransf, shapeBody);

        //    Shape shape = ParasolidTools.CreateShape(shapeEntity, shapeBody);
        //    ParasolidTools.SetPartition(shape);

        //    List<EntitiesByColor> coloredFaces = null;
        //    List<EntitiesByTransparency> transparentFaces = null;
        //    coloredFaces = ParasolidTools.GetBodyFaceColors(shapeBody);
        //    transparentFaces = ParasolidTools.GetBodyFaceTransparencies(shapeBody);
        //    List<EntitiesByName> namedFaces = ParasolidTools.GetBodyFaceNames(shapeBody);

        //    shape.DeleteAttributes();

        //    // Set attributes of the shape.

        //    TK.SX.Drawing.Color maximumReferencedColor;
        //    bool hasGeneralColor = ParasolidTools.SetGeneralAttributes(shape.FaceCount, coloredFaces, namedFaces, out maximumReferencedColor);

        //    ItemOperationKey opKey = new ItemOperationKey(0, this.opIndex++);
        //    ItemMonikerKey monikerKey = new ItemMonikerKey(shape.LevelKey, opKey);
        //    shape.SetDefaultMonikers(monikerKey);

        //    try
        //    {
        //        shapeEntity.Geometry = shape;
        //        shape.Owner = shapeEntity; // JeR 24/01/2011.

        //        // Set the explicit color of the shape entity.

        //        if (hasGeneralColor)
        //            shapeEntity.ExplicitColor = maximumReferencedColor;

        //        // Restore the transparency of the part.

        //        if (transparentFaces != null && transparentFaces.Count > 0)
        //        {
        //            int i;

        //            for (i = 0; i < transparentFaces.Count; i++)
        //            {
        //                if (transparentFaces[i].transparency > 0.0)
        //                    shapeEntity.ExplicitTransparency = TK.SX.Drawing.Transparency.FromFloat(transparentFaces[i].transparency);
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        Entity.Delete(shapeEntity);
        //        shapeEntity = null;
        //    }

        //    return shapeEntity;
        //}
    }
}