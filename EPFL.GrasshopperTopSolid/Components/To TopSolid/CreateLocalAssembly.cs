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
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.DB.Layers;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.G.D3.Shapes.Creations;
using TopSolid.Kernel.GR.Attributes;
using TopSolid.Kernel.SX;
using TopSolid.Kernel.SX.Drawing;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Items;
using TopSolid.Kernel.TX.Pdm;
using TopSolid.Kernel.TX.Undo;
using TopSolid.Kernel.TX.Units;
using TopSolid.Kernel.UI.Selections;
using TK = TopSolid.Kernel;
using SX = TopSolid.Kernel.SX;
using Cad = TopSolid.Cad;
using DB = TopSolid.Kernel.DB;
using Grasshopper.Kernel.Data;

namespace EPFL.GrasshopperTopSolid.Components.Preview
{
    public class CreateLocalAssembly : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateLocalAssembly class.
        /// </summary>
        public CreateLocalAssembly()
          : base("CreateLocalAssembly", "Nickname",
              "Bakes Rhino Geometry into Local Parts and Assemblies",
              "TopSolid", "To TopSolid")
        {
        }
        bool run = false;
        AssemblyDocument assemblyDocument = TopSolid.Kernel.UI.Application.CurrentDocument as AssemblyDocument;
        EntityList parts;

        protected override void BeforeSolveInstance()
        {
            //volatileData = Params.Input[0].VolatileData;
            //needsLocalAssemblies = volatileData.PathCount > 1;

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
            pManager.AddGeometryParameter("Geometry", "G", "Rhino Geometry to bake", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Assembly Document", "A", "target TopSolid Assembly to bake-in, if none provided will get current assembly", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddTextParameter("Name", "Name", "Name for Local Part Document", GH_ParamAccess.tree);
            pManager[2].Optional = true;
            pManager.AddGenericParameter("TopSolid Attributes", "attributes", "TopSolid's attributes for the created entities", GH_ParamAccess.item);
            //pManager[3].Optional = true;
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
                if (assemblyDocument == null)
                    return;

                SX.Collections.Generic.List<ShapeEntity> entities = new SX.Collections.Generic.List<ShapeEntity>();
                GH_Structure<IGH_GeometricGoo> rhinoBrepTree = new GH_Structure<IGH_GeometricGoo>();
                GH_Structure<GH_String> nameTree = new GH_Structure<GH_String>();

                DA.GetDataTree("Geometry", out rhinoBrepTree);
                DA.GetDataTree("Name", out nameTree);

                if (rhinoBrepTree is null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "null tree");
                    return;
                }

                bool sameBranches = rhinoBrepTree.Branches.Count == nameTree.Branches.Count;
                bool sameDataCount = rhinoBrepTree.DataCount == nameTree.DataCount;

                if (sameBranches && sameDataCount)
                {
                    List<(AssemblyEntity, GH_Path)> assembliesPathMatch = new System.Collections.Generic.List<(AssemblyEntity, GH_Path)>();
                    int longestPath = rhinoBrepTree.LongestPathIndex();
                    int longestPathDimension = rhinoBrepTree.get_Path(longestPath).Length;

                    System.Collections.IList list;

                    GH_Path currentPath;


                    for (int i = 0; i < rhinoBrepTree.PathCount; i++)
                    {
                        currentPath = rhinoBrepTree.get_Path(i);


                        if (rhinoBrepTree.PathExists(currentPath) && rhinoBrepTree.get_Branch(currentPath) != null)

                        {
                            list = rhinoBrepTree.get_Branch(currentPath);
                            int index = 0;
                            EntityList listLevelList = new EntityList();
                            foreach (var geometry in list)
                            {
                                entities.Clear();
                                GH_Brep gh_brep = geometry as GH_Brep;
                                Brep brep = null;
                                GH_Convert.ToBrep(gh_brep, ref brep, GH_Conversion.Both);
                                Shape shape = brep.ToHost();

                                #region Make shape entity

                                // Shape entity : A remplacer par le code Grasshopper

                                ShapeEntity shapeEntity = new ShapeEntity(assemblyDocument, 0);
                                entities.Add(shapeEntity);

                                // Replace shape geometry.

                                shapeEntity.Create();
                                shapeEntity.Geometry = shape;

                                //assemblyDocument.ShapesFolderEntity.AddEntity(shapeEntity); Can be changed later

                                // 
                                TK.DB.Operations.EntitiesCreation entitiesCreation = new TK.DB.Operations.EntitiesCreation(assemblyDocument, 0);
                                entitiesCreation.AddChildEntity(shapeEntity);
                                entitiesCreation.Create();

                                #endregion Make shape entity

                                #region Make local parts

                                SX.Collections.Generic.List<PartEntity> localParts = this.MakeLocalParts(assemblyDocument, entities);
                                LocalPartsCreation localPartCreation = new LocalPartsCreation(assemblyDocument, 0);
                                localPartCreation.SetChildParts(localParts);
                                localPartCreation.Create();

                                #endregion Make local parts


                                #region Make local assembly

                                AssemblyDefinitionCreation assemblyDefinitionCreation = new AssemblyDefinitionCreation(assemblyDocument, 0);
                                assemblyDefinitionCreation.IsModifiable = true;
                                assemblyDefinitionCreation.IsDeletable = true;

                                EntityList children = new EntityList();
                                SX.Collections.Generic.List<PartEntity> localChilren = localPartCreation.GetChildPartEntities(null);
                                children.AddRange(localChilren);

                                assemblyDefinitionCreation.SetOriginals(children);
                                assemblyDefinitionCreation.Create();

                                #region Manage properties (Name, Description, ...).

                                AssemblyEntity assemblyEntity = assemblyDefinitionCreation.ChildEntity;

                                assemblyEntity.NameParameterValue = new TK.SX.Globalization.LocalizableString(nameTree.get_Branch(currentPath)[index++].ToString());
                                assemblyEntity.DescriptionParameterValue = new TK.SX.Globalization.LocalizableString("Super local assembly with local parts");
                                assemblyEntity.PartNumberParameterValue = "GH";

                                listLevelList.Add(assemblyEntity);

                                #endregion Manage properties (Name, Reference, ...).

                                #region Manage representations

                                //

                                Cad.Design.DB.Representations.DesignRepresentationEntity localdesignRepresentation = assemblyDocument.FindOrCreateDesignRepresentation();
                                Cad.Design.DB.Representations.SimplifiedRepresentationEntity localsimplifiedRepresentation = assemblyDocument.SimplifiedRepresentationEntity;

                                if (localsimplifiedRepresentation != null)
                                {
                                    assemblyEntity.CreateLocalSimplifiedRepresentation();
                                }

                                assemblyDocument.DetailedRepresentationEntity.AddLocalRepresentationConstituent(assemblyEntity, Cad.Design.DB.Documents.ElementName.DetailedRepresentation);

                                localdesignRepresentation.AddLocalRepresentationConstituent(assemblyEntity, Cad.Design.DB.Documents.ElementName.SimplifiedRepresentation);

                                if (localsimplifiedRepresentation != null)
                                {
                                    localsimplifiedRepresentation.AddLocalRepresentationConstituent(assemblyEntity, Cad.Design.DB.Documents.ElementName.SimplifiedRepresentation);
                                }

                                #endregion Manage representations.

                                #endregion Make local assembly

                            }

                            #region Make local assembly

                            AssemblyDefinitionCreation localassemblyDefinitionCreation = new AssemblyDefinitionCreation(assemblyDocument, 0);
                            localassemblyDefinitionCreation.IsModifiable = true;
                            localassemblyDefinitionCreation.IsDeletable = true;

                            //EntityList localchildren = new EntityList();
                            //SX.Collections.Generic.List<PartEntity> localChilren = listLocalPartCreation.GetChildPartEntities(null);
                            //children.AddRange(localChilren);

                            localassemblyDefinitionCreation.SetOriginals(listLevelList);
                            localassemblyDefinitionCreation.Create();

                            #region Manage properties (Name, Description, ...).

                            AssemblyEntity localassemblyEntity = localassemblyDefinitionCreation.ChildEntity;

                            localassemblyEntity.NameParameterValue = new TK.SX.Globalization.LocalizableString(currentPath.ToString());
                            localassemblyEntity.DescriptionParameterValue = new TK.SX.Globalization.LocalizableString("Super local assembly with local parts");
                            localassemblyEntity.PartNumberParameterValue = "GH";

                            assembliesPathMatch.Add((localassemblyEntity, currentPath));

                            #endregion Manage properties (Name, Reference, ...).

                            #region Manage representations.

                            //

                            Cad.Design.DB.Representations.DesignRepresentationEntity designRepresentation = assemblyDocument.FindOrCreateDesignRepresentation();
                            Cad.Design.DB.Representations.SimplifiedRepresentationEntity simplifiedRepresentation = assemblyDocument.SimplifiedRepresentationEntity;

                            if (simplifiedRepresentation != null)
                            {
                                localassemblyEntity.CreateLocalSimplifiedRepresentation();
                            }

                            assemblyDocument.DetailedRepresentationEntity.AddLocalRepresentationConstituent(localassemblyEntity, Cad.Design.DB.Documents.ElementName.DetailedRepresentation);

                            designRepresentation.AddLocalRepresentationConstituent(localassemblyEntity, Cad.Design.DB.Documents.ElementName.SimplifiedRepresentation);

                            if (simplifiedRepresentation != null)
                            {
                                simplifiedRepresentation.AddLocalRepresentationConstituent(localassemblyEntity, Cad.Design.DB.Documents.ElementName.SimplifiedRepresentation);
                            }

                            #endregion Manage representations.

                            #endregion Make local assembly
                        }
                    }

                    //int currentLevel = 1;
                    GH_Path previousPath = rhinoBrepTree.Paths[0];
                    int counter;
                    EntityList assemblySubAssemblies = new EntityList();
                    AssemblyEntity assembly;
                    GH_Path path;

                    for (int i = 0; i < longestPathDimension; i++)
                    {
                        counter = 0;

                        foreach (var tuple in assembliesPathMatch)
                        {
                            assembly = tuple.Item1;
                            path = tuple.Item2;

                            if (counter == 0)
                            {
                                assemblySubAssemblies.Add(assembly);
                            }

                            else if (counter > 1 && path.Length < i - 1 && previousPath.Length < i - 1
                                && path[path.Length - i] == previousPath[path.Length - i])
                            {
                                assemblySubAssemblies.Add(assembly);
                            }

                            else
                            {
                                #region Make local assembly

                                AssemblyDefinitionCreation localassemblyDefinitionCreation = new AssemblyDefinitionCreation(assemblyDocument, 0);
                                localassemblyDefinitionCreation.IsModifiable = true;
                                localassemblyDefinitionCreation.IsDeletable = true;

                                //EntityList localchildren = new EntityList();
                                //SX.Collections.Generic.List<PartEntity> localChilren = listLocalPartCreation.GetChildPartEntities(null);
                                //children.AddRange(localChilren);

                                localassemblyDefinitionCreation.SetOriginals(assemblySubAssemblies);
                                localassemblyDefinitionCreation.Create();

                                #region Manage properties (Name, Description, ...).

                                AssemblyEntity localassemblyEntity = localassemblyDefinitionCreation.ChildEntity;

                                localassemblyEntity.NameParameterValue = new TK.SX.Globalization.LocalizableString(path.ToString() + " " + i);
                                localassemblyEntity.DescriptionParameterValue = new TK.SX.Globalization.LocalizableString("Super local assembly with local parts");
                                localassemblyEntity.PartNumberParameterValue = "GH";

                                assembliesPathMatch.Add((localassemblyEntity, path));

                                #endregion Manage properties (Name, Reference, ...).

                                #region Manage representations.

                                //

                                Cad.Design.DB.Representations.DesignRepresentationEntity designRepresentation = assemblyDocument.FindOrCreateDesignRepresentation();
                                Cad.Design.DB.Representations.SimplifiedRepresentationEntity simplifiedRepresentation = assemblyDocument.SimplifiedRepresentationEntity;

                                if (simplifiedRepresentation != null)
                                {
                                    localassemblyEntity.CreateLocalSimplifiedRepresentation();
                                }

                                assemblyDocument.DetailedRepresentationEntity.AddLocalRepresentationConstituent(localassemblyEntity, Cad.Design.DB.Documents.ElementName.DetailedRepresentation);

                                designRepresentation.AddLocalRepresentationConstituent(localassemblyEntity, Cad.Design.DB.Documents.ElementName.SimplifiedRepresentation);

                                if (simplifiedRepresentation != null)
                                {
                                    simplifiedRepresentation.AddLocalRepresentationConstituent(localassemblyEntity, Cad.Design.DB.Documents.ElementName.SimplifiedRepresentation);
                                }

                                #endregion Manage representations.

                                #endregion Make local assembly

                                assemblySubAssemblies.Clear();
                                previousPath = path;
                            }

                            counter++;
                        }


                        counter++;

                    }
                }


                assemblyDocument.Update(true, true);
            }
        }


        protected override void AfterSolveInstance()
        {
            if (UndoSequence.Current != null)
                UndoSequence.End();

            else
            {
                UndoSequence.UndoCurrent();
            }
            base.AfterSolveInstance();
        }

        private void MakeOperation(AssemblyDocument inDocument, SX.Collections.Generic.List<PartEntity> inPartEntities)
        {
            LocalPartsCreation op = new LocalPartsCreation(inDocument, 0);
            op.SetChildParts(inPartEntities);
            op.Create();
        }

        private SX.Collections.Generic.List<PartEntity> MakeLocalParts(AssemblyDocument inAssemblyDocument, SX.Collections.Generic.List<ShapeEntity> inShapeEntities)
        {
            SX.Collections.Generic.List<PartEntity> localParts = new SX.Collections.Generic.List<PartEntity>();

            int i = 1;
            foreach (ShapeEntity originalShape in inShapeEntities)
            {
                ShapeEntity shapeEntity = new ShapeEntity(inAssemblyDocument, 0);
                shapeEntity.UsesUniqueName = false;
                shapeEntity.IsDeletable = false;
                shapeEntity.Name = originalShape.Name;

                TK.DB.Entities.EntityList entities = new TK.DB.Entities.EntityList();
                entities.Add(shapeEntity);

                PartEntity localPart = new PartEntity(inAssemblyDocument, 0);
                localPart.SetLocalConstituents(entities);

                // Make default parameters.

                localPart.MakeDefaultParameters(TK.SX.Version.Current, true);

                localPart.NameParameterValue = new TK.SX.Globalization.LocalizableString("Local Part " + i);
                localPart.AddEntityToLocalRepresentation(shapeEntity, ElementName.DetailedRepresentation);
                localPart.AddEntityToLocalRepresentation(shapeEntity, ElementName.DesignRepresentation);
                localPart.AddEntityToLocalRepresentation(shapeEntity, ElementName.SimplifiedRepresentation);

                // Set geometry.

                shapeEntity.Geometry = (Shape)originalShape.Geometry.Clone(shapeEntity, false, true);

                localParts.Add(localPart);

                i++;
            }

            return localParts;
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
        public override System.Guid ComponentGuid
        {
            get { return new System.Guid("69A7C434-84CE-48A8-8D02-A3767B6035EC"); }
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

            //TODO
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
    }
}