using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using TopSolid.Kernel.DB.D3.Profiles;
using TopSolid.Kernel.DB.D3.Curves;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.D3.Points;
using TopSolid.Kernel.DB.D3.Surfaces;
using TopSolid.Kernel.TX.Undo;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.Parameters;

using TopSolid.Kernel.G.D3.Shapes.Sew;
using TopSolid.Kernel.DB.D3.Shapes.Sew;
using System.Linq;
using TopSolid.Kernel.TX.Units;
using TopSolid.Kernel.DB.Operations;
using TopSolid.Kernel.DB.D3.Planes;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.SX.Drawing;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.DB.Layers;
using TopSolid.Kernel.TX.Pdm;
using TopSolid.Kernel.GR.Attributes;
using TopSolid.Cad.Design.DB.Documents;
using TK = TopSolid.Kernel;
using SX = TopSolid.Kernel.SX;

using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.G.D3.Shapes.FacetShapes;

namespace EPFL.GrasshopperTopSolid.Components
{
    public class TSBakeExtended : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the TSBake class.
        /// </summary>
        public TSBakeExtended()
          : base("TopSolid Bake Extended", "TSBakeExt",
              "Bake Geometries with extended options",
              "TopSolid", "To TopSolid")
        {
        }
        //Class level variables, to be independent from SolveInstance
        //GH_Structure<IGH_Goo> entities = new GH_Structure<IGH_Goo>();
        EntitiesCreation entitiesCreation;
        Entity entity;
        EntityList entities = new EntityList();
        //IGH_Structure copyTree;
        bool run = false;
        //Param_GenericObject param = new Param_GenericObject();
        bool hasAttributes = false;
        PartDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as PartDocument;
        //EntitiesCreation entitiesCreation = null;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddGeometryParameter("Geometries", "G", "Geometries to bake", GH_ParamAccess.item);
            pManager.AddGenericParameter("Part Document", "Part", "target TopSolid Part Document to bake-in", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddTextParameter("Name", "Name", "Entity Name to be given", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddGenericParameter("TSAttributes", "attributes", "TopSolid's attributes for the baked entities", GH_ParamAccess.item);
            pManager[3].Optional = true;
            //pManager.AddNumberParameter("Tolerance", "Tol", "Tolerance for bake", GH_ParamAccess.item);
            //pManager.AddBooleanParameter("Sew?", "Sew?", "True to Sew Breps, False to keep faces split", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Bake?", "b?", "Set true to bake", GH_ParamAccess.item);
        }
        //EntityList list = new EntityList();

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddGenericParameter("TopSolid Entity", "TSEntity", "TopSolid Created Entity", GH_ParamAccess.item);
        }

        protected override void BeforeSolveInstance()
        {
            //Search Existing Elements that use desired name
            //Dict<doc,Queue>
            //and fill queue
            try
            {
                if (UndoSequence.Current != null)
                    UndoSequence.End();
                UndoSequence.Start("Grasshopper Bake", false);
            }
            catch
            {
                UndoSequence.UndoCurrent();
                UndoSequence.Start("Grasshopper Bake", false);
            }

            bool hasData = true;
            foreach (var param in Params.Input)
            {
                if (!param.Optional)
                    hasData = hasData && param.VolatileDataCount != 0;
            }

            if (Params.Input[3].VolatileDataCount != 0)
                hasAttributes = true;
            //if (hasData)
            //{
            //    if (entitiesCreation != null && entitiesCreation.IsCreated && !entitiesCreation.IsDeleted)
            //    {
            //        RootOperation.Delete(entitiesCreation);
            //    }
            //}
            entities.Clear();
            base.BeforeSolveInstance();

        }

        Queue<TopSolid.Kernel.DB.Elements.Element> PreviousElements;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String name = new GH_String();
            IGH_GeometricGoo geo = null;

            if (!DA.GetData(0, ref geo)) { return; }
            DA.GetData("Bake?", ref run);
            DA.GetData("Name", ref name);

            GetPartdocument(DA);

            if (run == true)
            {
                Shape topSolidShape;
                ElementList elements = new ElementList();
                doc.GetElements(elements);
                doc.EnsureIsDirty();

                #region Simpler Geometries (Not parts)
                if (geo is GH_Point ghPoint)
                {
                    Point3d rhinoPoint = new Point3d();
                    GH_Convert.ToPoint3d(ghPoint, ref rhinoPoint, 0);
                    TK.G.D3.Point topSolidPoint = rhinoPoint.ToHost();
                    PointEntity pointEntity = null;
                    PointsFolderEntity pointsFolderEntity = doc.PointsFolderEntity;


                    GH_ObjectWrapper attrWrapper = null;
                    Color tsColor = Color.Empty;
                    Transparency trnsp = Transparency.Empty;
                    DA.GetData("TSAttributes", ref attrWrapper);
                    string layerName = "";
                    var tsAttributes = attrWrapper.Value as Tuple<Transparency, Color, string>;

                    if (!(tsAttributes is null))
                    {
                        tsColor = tsAttributes.Item2;
                        trnsp = tsAttributes.Item1;
                        layerName = tsAttributes.Item3;

                    }


                    Layer layer = new Layer(-1);
                    LayerEntity layerEntity = new LayerEntity(doc, 0, layer);

                    LayersFolderEntity layersfolderEntity = LayersFolderEntity.GetOrCreateFolder(doc);
                    layerEntity = layersfolderEntity.SearchLayer(layerName);

                    if (layerEntity is null)
                    {
                        layersfolderEntity.AddLayer(layer, layerName);
                        layerEntity = layersfolderEntity.SearchLayer(layerName);
                    }


                    string entityName = null;
                    if (name != null)
                    {
                        entityName = name.ToString();
                        pointEntity = pointsFolderEntity.SearchDeepEntity(entityName) as PointEntity;

                        if (pointEntity is null)
                        {
                            pointEntity = new PointEntity(doc, 0);
                            pointEntity.Name = entityName;
                            if (entitiesCreation == null)
                                entitiesCreation = new EntitiesCreation(doc, 0);
                            entitiesCreation.AddChildEntity(pointEntity);
                            //pointEntity.Create(pointsFolderEntity);
                        }
                    }

                    pointEntity.ExplicitColor = tsColor;
                    pointEntity.ExplicitTransparency = trnsp;
                    pointEntity.ExplicitLayer = layer;
                    pointEntity.Geometry = topSolidPoint;



                    entity = pointEntity;

                    //UndoSequence.End();

                }
                else if (geo is GH_Plane ghPlane)
                {
                    var rhPlane = new Plane();
                    GH_Convert.ToPlane(ghPlane, ref rhPlane, 0);
                    var tp = rhPlane.ToHost();
                    PlaneEntity planeEntity = new PlaneEntity(doc, 0);
                    planeEntity.Geometry = tp;
                    planeEntity.Create(doc.PlanesFolderEntity);
                }
                else if (geo is GH_Curve gc)
                {
                    Curve rhinoCurve = null;
                    GH_Convert.ToCurve(gc, ref rhinoCurve, 0);
                    TK.G.D3.Curves.Curve topSolidCurve = rhinoCurve.ToHost();
                    CurveEntity curveEntity = new CurveEntity(doc, 0);
                    curveEntity.Geometry = topSolidCurve;
                    curveEntity.Create(ProfilesFolderEntity.GetOrCreateFolder(doc));
                    //list.Add(ce);
                }
                else if (geo is GH_Surface gs)
                {
                    Surface rs = null;
                    GH_Convert.ToSurface(gs, ref rs, 0);
                    var rn = rs.ToNurbsSurface();
                    var ts = rn.ToHost();
                    SurfaceEntity se = new SurfaceEntity(doc, 0);
                    se.Geometry = ts;
                    se.Create(doc.ShapesFolderEntity);
                    //list.Add(se);
                }
                #endregion

                else if (geo is GH_Mesh gh_mesh)
                {
                    Mesh rhinoMesh = null;
                    GH_Convert.ToMesh(gh_mesh, ref rhinoMesh, GH_Conversion.Both);
                    if (rhinoMesh is null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Mesh error, null");
                        return;
                    }

                    rhinoMesh.Faces.qu

                    FacetedShapeMaker maker = new FacetedShapeMaker(TK.SX.Version.Current);

                }

                else if (geo is GH_Brep gbrep)
                {
                    Brep rhinoBrep = null;
                    GH_Convert.ToBrep(gbrep, ref rhinoBrep, 0);
                    if (rhinoBrep is null) return;

                    topSolidShape = rhinoBrep.ToHost();

                    if (name != null)
                    {
                        bool OpCreated = entitiesCreation != null && entitiesCreation.IsCreated && !entitiesCreation.IsDeleted;
                        if (OpCreated)
                        {
                            entitiesCreation.IsEdited = true;
                            entitiesCreation.NeedsExecuting = true;

                            ShapeEntity existingShpaeEntity = entitiesCreation.ChildrenEntities.First() as ShapeEntity;
                            if (existingShpaeEntity != null)
                                entity = existingShpaeEntity;

                            if (entity != null)
                            {
                                entity.Geometry = topSolidShape;
                                SetShapeAttributes(entity, DA, name);
                                (entity.Geometry as Shape).UpdateDisplayItems();
                                entity.NotifyModifying(true);
                            }
                            entitiesCreation.IsEdited = false;
                        }

                        else
                        {
                            entity = new ShapeEntity(doc, 0);
                            entity.Geometry = topSolidShape;
                            SetShapeAttributes(entity, DA, name);


                            entitiesCreation = new EntitiesCreation(doc, 0);
                            entitiesCreation.AddChildEntity(entity);
                            entitiesCreation.Create();
                            doc.ShapesFolderEntity.AddEntity(entity);
                        }

                    }

                }
                if (Params.Output.Count > 0)
                    DA.SetData("TopSolid Entities", entity);
            }
        }

        /// <summary>
        /// Sets target document from input, or else take current document by default
        /// </summary>
        /// <param name="dA"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void GetPartdocument(IGH_DataAccess DA)
        {
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();
            IDocument res = null;
            if (DA.GetData("Part Document", ref wrapper))
            {
                if (wrapper.Value is string || wrapper.Value is GH_String)
                {
                    res = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault();
                    doc = res as PartDocument;
                }
                else if (wrapper.Value is IDocumentItem)
                    doc = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as PartDocument;
                else if (wrapper.Value is IDocument)
                    doc = wrapper.Value as PartDocument;
            }

            if (doc == null)
                doc = TopSolid.Kernel.UI.Application.CurrentDocument as PartDocument;
        }

        private void SetShapeAttributes(Entity entity, IGH_DataAccess DA, GH_String name)
        {
            GH_ObjectWrapper attributesWrapper = null;
            SX.Drawing.Color tsColor = SX.Drawing.Color.Red;
            SX.Drawing.Transparency trnsp = SX.Drawing.Transparency.SemiTransparent;
            Layer topSolidLayer = new Layer(-1);
            LayerEntity layerEntity = new LayerEntity(doc, 0, topSolidLayer);

            if (DA.GetData("TSAttributes", ref attributesWrapper))
            {
                string layerName = "";
                var topSolidAttributes = attributesWrapper.Value as Tuple<Transparency, Color, string>;

                if (topSolidAttributes != null)
                {
                    tsColor = topSolidAttributes.Item2;
                    trnsp = topSolidAttributes.Item1;
                    layerName = topSolidAttributes.Item3;
                }

                var layfoldEnt = LayersFolderEntity.GetOrCreateFolder(doc);
                layerEntity = layfoldEnt.SearchLayer(layerName);

                if (layerEntity == null)
                {
                    layerEntity = new LayerEntity(doc, 0, topSolidLayer);
                    layerEntity.Name = layerName;
                }

            }

            entity.ExplicitColor = tsColor;
            entity.ExplicitTransparency = trnsp;
            entity.ExplicitLayer = layerEntity.Layer;

            if (name.Value != null && name.Value != "")
            {
                var searchEntity = doc.ShapesFolderEntity.SearchEntity(name.Value);
                if (searchEntity == null)
                {
                    entity.Name = name.Value;
                }
            }

        }

        protected override void AfterSolveInstance()
        {
            //TODO
            //foreach (var element in PreviousElements)
            //{
            //    // Delete those elements
            //}

            //PreviousElements.Clear();

            try
            {
                UndoSequence.End();
                if (doc != null) doc.Update(true, false);
            }
            catch
            {
                UndoSequence.UndoCurrent();
                if (doc != null) doc.Update(true, true);
            }

            base.AfterSolveInstance();

        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {

            if ((side == GH_ParameterSide.Output) && Params.Output.Count == 0)
                return true;

            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {

            if (side == GH_ParameterSide.Output)
                return true;
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {

            if (side is GH_ParameterSide.Output)
            {
                var param = new Param_GenericObject();
                param.Name = "TopSolid Entities";
                param.Access = GH_ParamAccess.item;

                param.NickName = "TS Entities";
                param.Description = "TopSolid Created Entities";

                return param;
            }
            return null;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.ShapeEntity, 24, 24).ToBitmap();

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("feb8951c-6c69-44c8-8c8b-a11e02960625"); }
        }
    }
}
