using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.UI;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Cad.Design.DB.Local.Operations;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.DB.Layers;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.GR.Attributes;
using TopSolid.Kernel.SX.Drawing;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Pdm;
using TopSolid.Kernel.TX.Undo;
using TK = TopSolid.Kernel;

namespace EPFL.GrasshopperTopSolid.Components.Preview
{
    public class BakeLocalPart : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TSLocalPart class.
        /// </summary>
        public BakeLocalPart()
          : base("BakeLocalPart", "LP",
              "Bakes Rhino Geometry into Local Part inserted in Assembly",
              "TopSolid", "Preview")
        {
        }

        protected override void BeforeSolveInstance()
        {
            try
            {
                UndoSequence.Start("Create Local Part", false);
            }

            catch
            {
                UndoSequence.UndoCurrent();
                UndoSequence.Start("Create Local Part", false);
            }
            base.BeforeSolveInstance();
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Rhino Geometry to bake", GH_ParamAccess.item);
            pManager.AddGenericParameter("Assembly Document", "A", "target TopSolid Assembly to bake-in, if none provided will get current assembly", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddTextParameter("Name", "Name", "Name for Local Part Document", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddGenericParameter("TSAttributes", "attributes", "TopSolid's attributes for the created entities", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Bake?", "b?", "Set true to bake", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        bool run = false;
        AssemblyDocument assemblyDocument = TopSolid.Kernel.UI.Application.CurrentDocument as AssemblyDocument;


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String name = new GH_String();
            IGH_GeometricGoo rhinoGeometry = null;
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();//needed for TopSolid types

            if (!DA.GetData("Bake?", ref run) || !run) return;
            if (!DA.GetData("Geometry", ref rhinoGeometry)) return;
            if (!DA.GetData("Name", ref name)) return;


            IDocument res = null;
            if (DA.GetData("Assembly Document", ref wrapper))
            {
                if (wrapper.Value is string || wrapper.Value is GH_String)
                {
                    res = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault();
                    assemblyDocument = res as AssemblyDocument;
                }
                else if (wrapper.Value is IDocumentItem)
                    assemblyDocument = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as AssemblyDocument;
                else if (wrapper.Value is IDocument)
                    assemblyDocument = wrapper.Value as AssemblyDocument;
            }

            if (assemblyDocument == null)
                assemblyDocument = TopSolid.Kernel.UI.Application.CurrentDocument as AssemblyDocument;

            //The baking process starts on button
            if (run == true)
            {
                Brep brep = null;
                GH_Convert.ToBrep(rhinoGeometry, ref brep, GH_Conversion.Both);
                Shape topSolidShape = brep.ToHost();

                GH_ObjectWrapper attributesWrapper = null;
                Color topSolidColor = Color.Empty;
                Transparency topSolidtransparency = Transparency.Empty;

                DA.GetData("TSAttributes", ref attributesWrapper);
                string topSolidLayerName = "";
                var topSolidAttributes = attributesWrapper.Value as Tuple<Transparency, Color, string>;

                if (topSolidAttributes != null)
                {
                    topSolidColor = topSolidAttributes.Item2;
                    topSolidtransparency = topSolidAttributes.Item1;
                    topSolidLayerName = topSolidAttributes.Item3;
                }


                LocalPartsCreation localPartCreation = new LocalPartsCreation(assemblyDocument, 0);
                PartDefinitionPrimitive localPart = new PartDefinitionPrimitive(localPartCreation, assemblyDocument);
                localPart.NodeEntity.IsDeletable = true;

                Layer topSolidLayer = new Layer(-1);
                LayerEntity layerEntity = new LayerEntity(assemblyDocument, 0, topSolidLayer);

                var layfoldEnt = LayersFolderEntity.GetOrCreateFolder(assemblyDocument);
                layerEntity = layfoldEnt.SearchLayer(topSolidLayerName);

                if (layerEntity == null)
                {
                    layerEntity = new LayerEntity(assemblyDocument, 0, topSolidLayer);
                    layerEntity.Name = topSolidLayerName;
                }

                // A partir de la forme G.D3.Shape shape.
                ShapeEntity shapeEntity = new ShapeEntity(localPart.OwnerDocument, 0);
                shapeEntity.ExplicitColor = topSolidColor;
                shapeEntity.ExplicitLayer = topSolidLayer;
                shapeEntity.ExplicitTransparency = topSolidtransparency;
                //shapeEntity.Geometry = topSolidShape;
                shapeEntity.SetGeometry(topSolidShape, true, true);

                // si on donne un nom à la forme de la pièce locale, on pourra y accéder plus facilement pour la mise à jour, notamment mettre à jour sa géométrie
                shapeEntity.Name = name.Value;

                //Retrouver la ShapeEntity via :
                //ShapeEntity shapeEntity = this.envelopePart.NodeEntity.SearchLocalConstituent(Documents.ElementName.EnvelopeShape) as ShapeEntity;

                // Ajout de la ChapeEntity à la pièce locale
                EntityList entities = new EntityList();
                entities.Add(shapeEntity);
                localPart.NodeEntity.SetLocalConstituents(entities);
                localPart.NodeEntity.AddEntityToLocalRepresentation(shapeEntity, TopSolid.Cad.Design.DB.Documents.ElementName.DetailedRepresentation);
                localPart.NodeEntity.AddEntityToLocalRepresentation(shapeEntity, TopSolid.Cad.Design.DB.Documents.ElementName.DesignRepresentation);
                localPart.NodeEntity.AddEntityToLocalRepresentation(shapeEntity, TopSolid.Cad.Design.DB.Documents.ElementName.SimplifiedRepresentation);
                localPartCreation.IsDeletable = true;

                localPartCreation.Create();

            }
        }

        protected override void AfterSolveInstance()
        {
            try
            {
                UndoSequence.End();
            }

            catch
            {
                UndoSequence.UndoCurrent();
            }
            base.AfterSolveInstance();
        }

        /// <summary>
        /// Initializes a new instance of the TSDocSelector2 class.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.PartDefinitionEntity, 24, 24).ToBitmap();

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("56F50B6F-7470-450E-9414-060CCC13851F"); }
        }
    }
}