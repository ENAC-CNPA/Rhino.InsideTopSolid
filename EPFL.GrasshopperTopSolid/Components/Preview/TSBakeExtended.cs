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
using TopSolid.Kernel.DB.D3.Shapes.Creations;
using Grasshopper.Kernel.Data;
using TopSolid.Cad.Design.DB.Documents;
using Design = TopSolid.Cad.Design;
using TK = TopSolid.Kernel;
using TopSolid.Cad.Design.DB.Local.Operations;
using Eto.Forms;
using PLMComponents.Parasolid.PK_.Unsafe;
using Rhino.UI;
using TopSolid.Kernel.DB.D3.Shapes.Unsew;

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
              "TopSolid", "Preview")
        {
        }
        //Class level variables, to be independent from SolveInstance
        //GH_Structure<IGH_Goo> entities = new GH_Structure<IGH_Goo>();
        Entity entity;
        EntityList entities = new EntityList();
        //IGH_Structure copyTree;
        bool run = false;
        //Param_GenericObject param = new Param_GenericObject();
        DesignDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as DesignDocument;
        EntitiesCreation entitiesCreation = null;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddGeometryParameter("Geometries", "G", "Geometries to bake", GH_ParamAccess.item);
            pManager.AddGenericParameter("TSDocument", "TSDoc", "target TopSolid Document to bake-in", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddTextParameter("Name", "Name", "Entity Name to be given", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddGenericParameter("TSAttributes", "attributes", "TopSolid's attributes for the baked entities", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "Tol", "Tolerance for bake", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Sew?", "Sew?", "True to Sew Breps, False to keep faces split", GH_ParamAccess.item);
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
            try
            {
                UndoSequence.End();
                UndoSequence.Start("Grasshopper Bake", false);

            }
            catch
            {
                UndoSequence.UndoCurrent();
                UndoSequence.Start("Grasshopper Bake", false);
            }


            entities.Clear();
            base.BeforeSolveInstance();

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {


            //ent = null;
            GH_String name = new GH_String();
            IGH_GeometricGoo geo = null;

            bool sew = false;
            if (!DA.GetData(0, ref geo)) { return; }
            //if (geo == null) { return; }
            //if (geo.Count == 0) { return; }

            DA.GetData("Bake?", ref run);
            DA.GetData("Sew?", ref sew);
            DA.GetData("Name", ref name);


            //Setting target document from input, or else take current document by default
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();

            IDocument res = null;
            if (DA.GetData("TSDocument", ref wrapper))
            {
                if (wrapper.Value is string || wrapper.Value is GH_String)
                {
                    res = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault();
                    doc = res as DesignDocument;
                }
                else if (wrapper.Value is IDocumentItem)
                    doc = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as DesignDocument;
                else if (wrapper.Value is IDocument)
                    doc = wrapper.Value as DesignDocument;
            }

            if (doc == null)
                doc = TopSolid.Kernel.UI.Application.CurrentDocument as DesignDocument;

            if (run == true)
            {
                if (entitiesCreation is null)
                    entitiesCreation = new EntitiesCreation(doc, 0);

                doc.EnsureIsDirty();
                //UndoSequence.UndoCurrent();
                //UndoSequence.Start("Bake", false);
                //list.Clear();


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
                else if (geo is GH_Brep gbrep)
                {///*

                    //if (doc is AssemblyDocument assemblyDocument)
                    //{
                    // Creation local part.
                    //LocalPartsCreation localPartCreation = new LocalPartsCreation(assemblyDocument, 0);
                    //PartDefinitionPrimitive localPart = new PartDefinitionPrimitive(localPartCreation, assemblyDocument);
                    //localPart.NodeEntity.IsDeletable = false;

                    // A partir de la forme G.D3.Shape shape.
                    //ShapeEntity shapeEntity = new ShapeEntity(localPart.OwnerDocument, 0);

                    // si on donne un nom à la forme de la pièce locale, on pourra y accéder plus facilement pour la mise à jour, notamment mettre à jour sa géométrie
                    //shapeEntity.Name = Documents.ElementName.EnvelopeShape;

                    //Retrouver la ShapeEntity via :
                    //ShapeEntity shapeEntity = this.envelopePart.NodeEntity.SearchLocalConstituent(Documents.ElementName.EnvelopeShape) as ShapeEntity;



                    // Ajout de la ChapeEntity à la pièce locale
                    //TK.DB.Entities.EntityList entities = new TK.DB.Entities.EntityList();
                    //entities.Add(shapeEntity);
                    //localPartCreation.Create();

                    Brep rs = null;
                    GH_Convert.ToBrep(gbrep, ref rs, 0);
                    if (rs is null) return;
                    double tol = 0.00001;
                    GH_ObjectWrapper attrWrapper = null;
                    Color tsColor = Color.Empty;
                    Transparency trnsp = Transparency.Empty;
                    ShapeList shapeList;

                    DA.GetData("Tolerance", ref tol);
                    DA.GetData("TSAttributes", ref attrWrapper);
                    string layerName = "";
                    var tsAttributes = attrWrapper.Value as Tuple<Transparency, Color, string>;

                    if (tsAttributes != null)
                    {
                        tsColor = tsAttributes.Item2;
                        trnsp = tsAttributes.Item1;
                        layerName = tsAttributes.Item3;
                    }

                    shapeList = rs.ToHost(tol);

                    EntitiesCreation shapesCreation = new EntitiesCreation(doc, 0);
                    Layer layer = new Layer(-1);
                    LayerEntity layEnt = new LayerEntity(doc, 0, layer);

                    var layfoldEnt = LayersFolderEntity.GetOrCreateFolder(doc);
                    layEnt = layfoldEnt.SearchLayer(layerName);

                    if (layEnt == null)
                    {

                        layEnt = new LayerEntity(doc, 0, layer);
                        layEnt.Name = layerName;
                    }

                    //localPart.NodeEntity.IsDeletable = true;
                    foreach (var ts in shapeList)
                    {
                        ShapeEntity se = new ShapeEntity(doc, 0);
                        se.Geometry = ts;
                        se.ExplicitColor = tsColor;
                        se.ExplicitTransparency = trnsp;
                        se.ExplicitLayer = layEnt.Layer;

                        //se.Create(doc.ShapesFolderEntity);
                        shapesCreation.AddChildEntity(se);
                        shapesCreation.CanDeleteFromChild(se);

                    }
                    SewOperation sewOperation = new SewOperation(doc, 0);

                    //if (sew)
                    //    sewOperation.AddOperation(shapesCreation);

                    //shapesCreation.Owner = sewOperation;

                    shapesCreation.Create();


                    if (shapesCreation.ChildEntityCount > 1)
                    {
                        var mod = shapesCreation.ChildrenEntities;
                        sewOperation.ModifiedEntity = shapesCreation.ChildrenEntities.First() as ShapeEntity;
                        for (int i = 1; i < shapesCreation.ChildEntityCount; i++)
                        {
                            shapesCreation.ChildrenEntities.ElementAt(i).IsGhost = true;//To Comment in case Debug is needed
                            sewOperation.AddTool(new ProvidedSmartShape(sewOperation, shapesCreation.ChildrenEntities.ElementAt(i)));
                        }

                        if (tol != 0)
                            sewOperation.GapWidth = new BasicSmartReal(sewOperation, tol, UnitType.Length, doc);
                        else
                            sewOperation.GapWidth = new BasicSmartReal(sewOperation, TopSolid.Kernel.G.Precision.ModelingLinearTolerance, UnitType.Length, doc);
                        sewOperation.NbIterations = new BasicSmartInteger(sewOperation, 5);
                        sewOperation.ExplicitLayer = layer;
                        //sewOperation.Parent = shapesCreation;
                        //sewOperation.AddOperation(shapesCreation);
                        //shapesCreation.Parent = sewOperation;
                        try
                        {
                            sewOperation.Create();
                        }

                        catch (Exception ex)
                        {
                            UndoSequence.End();
                            MessageBox.Show(ex.Message, MessageBoxType.Warning);
                        }
                    }

                    if (name != null)
                    {
                        entity = shapesCreation.ChildrenEntities.ElementAt(0);
                        entity.Name = name.ToString();
                        doc.ShapesFolderEntity.AddEntity(entity);
                    }

                    if (Params.Output.Count > 0)
                        DA.SetData("TopSolid Entities", shapesCreation.ChildrenEntities.ElementAt(0));

                    //}

                }


            }
            if (Params.Output.Count > 0)

                DA.SetData(0, entity); //TODO Generalize
                                       //*/


        }


        protected override void AfterSolveInstance()
        {
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
