using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.UI;
using TopSolid.Cad.Design.DB;
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
using TopSolid.Kernel.TX.Units;
using TK = TopSolid.Kernel;

namespace EPFL.GrasshopperTopSolid.Components.STRATO
{
    public class BakeLocalPartStrato : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TSLocalPart class.
        /// </summary>
        public BakeLocalPartStrato()
          : base("BakeLocalPartStrato", "LPStrato",
              "Bakes Rhino Geometry into Local Part inserted in Assembly",
              "TopSolid", "Strato")
        {
        }

        bool run = false;
        AssemblyDocument assemblyDocument = TopSolid.Kernel.UI.Application.CurrentDocument as AssemblyDocument;
        List<PartEntity> CreatedPartEntities = new List<PartEntity>();
        bool needsLocalAssemblies = false;
        IGH_Structure volatileData;

        protected override void BeforeSolveInstance()
        {
            volatileData = Params.Input[0].VolatileData;
            needsLocalAssemblies = volatileData.PathCount > 1;

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
            pManager.AddLineParameter("Line", "V", "Line to TS Axis ", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager.AddGenericParameter("TopSolid Attributes", "attributes", "TopSolid's attributes for the created entities", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Bake?", "b?", "Set true to bake", GH_ParamAccess.item);
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
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();//needed for TopSolid types

            if (!DA.GetData("Bake?", ref run) || !run) return;

            if (DA.GetData("Assembly Document", ref wrapper))
            {
                assemblyDocument = GetAssemblyDocument(wrapper);
            }

            if (assemblyDocument == null)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not find valid Assembly");

            //The baking process starts on boolean true
            if (run == true)
            {
                GH_String name = new GH_String();
                IGH_GeometricGoo rhinoGeometry = null;
                EntityList entities = new EntityList();
                Brep brep = null;
                GH_ObjectWrapper attributesWrapper = null;

                if (!DA.GetData("Geometry", ref rhinoGeometry)) return;
                if (!DA.GetData("Name", ref name)) return;
                if (!DA.GetData("TopSolid Attributes", ref attributesWrapper)) return;

                var topSolidAttributes = attributesWrapper.Value as Tuple<Transparency, Color, string>;
                LocalPartsCreation localPartCreation = new LocalPartsCreation(assemblyDocument, 0);

                ShapeEntity shapeEntity = new ShapeEntity(assemblyDocument, 0);
                GH_Convert.ToBrep(rhinoGeometry, ref brep, GH_Conversion.Both);
                Shape topSolidShape = brep.ToHost();

                SetTopSolidEntity(topSolidAttributes, shapeEntity, topSolidShape);

                GH_Line ghLine = new GH_Line();
                if (DA.GetData("Line", ref ghLine))
                {
                    Rhino.Geometry.Line rhinoLine = new Line();
                    GH_Convert.ToLine(ghLine, ref rhinoLine, GH_Conversion.Both);
                    Rhino.Geometry.LineCurve rhinoLineCurve = new LineCurve(rhinoLine);


                    TK.DB.D3.Axes.AxisEntity axisEntity = new TK.DB.D3.Axes.AxisEntity(assemblyDocument, 0);
                    axisEntity.Geometry = new TK.G.D3.Axis(rhinoLineCurve.PointAtStart.ToHost(), rhinoLineCurve.PointAtEnd.ToHost());
                    entities.Add(axisEntity);


                }

                entities.Add(shapeEntity);
                PartEntity partEntity = CreateLocalPart(assemblyDocument, entities, name.Value);

                localPartCreation.AddChildPart(partEntity);
                localPartCreation.IsDeletable = true;
                localPartCreation.Name = "Creation Part : " + name.Value;

                //assemblyDocument.DetailedRepresentationEntity.AddEntity(partEntity);
                //assemblyDocument.DesignRepresentationEntity.AddEntity(partEntity);
                //assemblyDocument.SimplifiedRepresentationEntity.AddEntity(partEntity);

                localPartCreation.Create();
                CreatedPartEntities.Add(partEntity);
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
            get { return new Guid("DA763CED-325B-48FA-B8AE-606A6865F23D"); }

        }


        private PartEntity CreateLocalPart(AssemblyDocument inAssemblyDocument, EntityList inEntities, string inName)
        {
            PartEntity localPart = new PartEntity(inAssemblyDocument, 0);
            localPart.SetLocalConstituents(inEntities);

            localPart.MakeDefaultParameters(TK.SX.Version.Current, true);
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

        private void SetTopSolidEntity(Tuple<Transparency, Color, string> topSolidAttributes, Entity entity, Shape topSolidShape)
        {
            Color topSolidColor = Color.Empty;
            Transparency topSolidtransparency = Transparency.Empty;
            string topSolidLayerName = "";
            Layer topSolidLayer = new Layer(-1);
            LayerEntity layerEntity = new LayerEntity(assemblyDocument, 0, topSolidLayer);

            if (topSolidAttributes != null)
            {
                topSolidColor = topSolidAttributes.Item2;
                topSolidtransparency = topSolidAttributes.Item1;
                topSolidLayerName = topSolidAttributes.Item3;
            }


            var layfoldEnt = LayersFolderEntity.GetOrCreateFolder(assemblyDocument);
            layerEntity = layfoldEnt.SearchLayer(topSolidLayerName);

            if (layerEntity == null)
            {
                layerEntity = new LayerEntity(assemblyDocument, 0, topSolidLayer);
                layerEntity.Name = topSolidLayerName;
            }


            entity.ExplicitColor = topSolidColor;
            entity.ExplicitLayer = topSolidLayer;
            entity.ExplicitTransparency = topSolidtransparency;
            //entity.SetGeometry(topSolidShape, true, true);
            entity.Geometry = topSolidShape;
        }

        private AssemblyDocument GetAssemblyDocument(GH_ObjectWrapper wrapper)
        {
            IDocument res = null;
            if (wrapper.Value is string || wrapper.Value is GH_String)
            {
                res = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault();
                assemblyDocument = res as AssemblyDocument;
            }
            else if (wrapper.Value is IDocumentItem)
                assemblyDocument = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as AssemblyDocument;
            else if (wrapper.Value is IDocument)
                assemblyDocument = wrapper.Value as AssemblyDocument;

            if (assemblyDocument == null)
                assemblyDocument = TopSolid.Kernel.UI.Application.CurrentDocument as AssemblyDocument;

            return assemblyDocument;
        }


        //Local Assemblies VIG
        /*

        /// <summary>

        /// Creates a local assembly and fills representations.

        /// </summary>

        /// <param name="inAssemblyDoc">Input assembly document.</param>

        /// <param name="definitionEntities">Definition entities of the document.</param>

        /// <returns>Assembly Entity.</returns>

        private AssemblyEntity CreateLocalAssembly(AssemblyDocument inAssemblyDoc, EntityList definitionEntities)

                {

                    AssemblyDefinitionCreation assemblyDefinitionOp = new AssemblyDefinitionCreation(inAssemblyDoc, 0);

                    assemblyDefinitionOp.SetOriginals(definitionEntities);

                    assemblyDefinitionOp.Create();



                    AssemblyEntity assemblyEntity = assemblyDefinitionOp.ChildEntity;



                    // Fill representations.



                    DesignRepresentationEntity designRepresentation = inAssemblyDoc.FindOrCreateDesignRepresentation();

                    SimplifiedRepresentationEntity simplifiedRepresentation = inAssemblyDoc.SimplifiedRepresentationEntity;



                    if (simplifiedRepresentation != null)

                    {

                        assemblyEntity.CreateLocalSimplifiedRepresentation();

                    }



                    inAssemblyDoc.DetailedRepresentationEntity.AddLocalRepresentationConstituent(assemblyEntity, Cad.Design.DB.Documents.ElementName.DetailedRepresentation);



                    designRepresentation.AddLocalRepresentationConstituent(assemblyEntity, Cad.Design.DB.Documents.ElementName.SimplifiedRepresentation);



                    if (simplifiedRepresentation != null)

                    {

                        simplifiedRepresentation.AddLocalRepresentationConstituent(assemblyEntity, Cad.Design.DB.Documents.ElementName.SimplifiedRepresentation);

                    }

                    RepresentationReference representationRef = inAssemblyDoc.CurrentRepresentationEntity.SearchEntityRepresentation(assemblyEntity);

                    if (representationRef != null)

                    {

                        representationRef.Activate();

                    }



                    return assemblyEntity;

                }
                         * 
                         * 
                         * 
                         */

    }
}