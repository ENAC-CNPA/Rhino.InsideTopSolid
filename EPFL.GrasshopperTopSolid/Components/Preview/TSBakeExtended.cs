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
        GH_Structure<IGH_Goo> entities = new GH_Structure<IGH_Goo>();
        Entity entity;
        IGH_Structure copyTree;
        bool run = false;
        Param_GenericObject param = new Param_GenericObject();
        DesignDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as DesignDocument;

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
                UndoSequence.Start("Grasshopper Bake", false);

            }
            catch
            {
                UndoSequence.UndoCurrent();
                UndoSequence.Start("Grasshopper Bake", false);
            }


            //base.BeforeSolveInstance();
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

                doc.EnsureIsDirty();
                UndoSequence.UndoCurrent();
                UndoSequence.Start("Bake", false);
                //list.Clear();


                #region Simpler Geometries (Not parts)
                if (geo is GH_Point gp)
                {
                    var rp = new Point3d();
                    GH_Convert.ToPoint3d(gp, ref rp, 0);
                    var tp = rp.ToHost();
                    PointEntity pe = new PointEntity(doc, 0);
                    if (name != null)
                    {
                        pe.Name = name.ToString();
                    }

                    double tol = 0.00001;
                    GH_ObjectWrapper attrWrapper = null;
                    Color tsColor = Color.Empty;
                    Transparency trnsp = Transparency.Empty;
                    ShapeList shape;

                    DA.GetData("Tolerance", ref tol);
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
                    LayerEntity layEnt = new LayerEntity(doc, 0, layer);

                    var layfoldEnt = LayersFolderEntity.GetOrCreateFolder(doc);
                    layEnt = layfoldEnt.SearchLayer(layerName);

                    if (layEnt is null)
                    {
                        layfoldEnt.AddLayer(layer, layerName);
                        layEnt = layfoldEnt.SearchLayer(layerName);
                    }

                    pe.ExplicitColor = tsColor;
                    pe.ExplicitTransparency = trnsp;
                    pe.ExplicitLayer = layer;
                    pe.Geometry = tp;
                    pe.Create(doc.PointsFolderEntity);
                    entity = pe;

                }

                else if (geo is GH_Plane ghPlane)
                {
                    var rhPlane = new Plane();
                    GH_Convert.ToPlane(ghPlane, ref rhPlane, 0);
                    var tp = rhPlane.ToHost();
                    PlaneEntity pe = new PlaneEntity(doc, 0);
                    pe.Geometry = tp;
                    pe.Create(doc.PlanesFolderEntity);
                }
                else if (geo is GH_Curve gc)
                {
                    Curve rc = null;
                    GH_Convert.ToCurve(gc, ref rc, 0);
                    var rn = rc.ToNurbsCurve();
                    var tc = rn.ToHost();
                    CurveEntity ce = new CurveEntity(doc, 0);
                    ce.Geometry = tc;
                    ce.Create(doc.SketchesFolderEntity);
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
                    se.Create(doc.PointsFolderEntity);
                    //list.Add(se);
                }
                #endregion
                else if (geo is GH_Brep gbrep)
                {///*

                    if (doc is AssemblyDocument assemblyDocument)
                    {
                        // Creation local part.
                        LocalPartsCreation localPartCreation = new LocalPartsCreation(assemblyDocument, 0);
                        PartDefinitionPrimitive localPart = new PartDefinitionPrimitive(localPartCreation, assemblyDocument);
                        localPart.NodeEntity.IsDeletable = false;

                        // A partir de la forme G.D3.Shape shape.
                        //ShapeEntity shapeEntity = new ShapeEntity(localPart.OwnerDocument, 0);

                        // si on donne un nom à la forme de la pièce locale, on pourra y accéder plus facilement pour la mise à jour, notamment mettre à jour sa géométrie
                        //shapeEntity.Name = Documents.ElementName.EnvelopeShape;

                        //Retrouver la ShapeEntity via :
                        //ShapeEntity shapeEntity = this.envelopePart.NodeEntity.SearchLocalConstituent(Documents.ElementName.EnvelopeShape) as ShapeEntity;



                        // Ajout de la ChapeEntity à la pièce locale
                        TK.DB.Entities.EntityList entities = new TK.DB.Entities.EntityList();
                        //entities.Add(shapeEntity);
                        //localPartCreation.Create();

                        Brep rs = null;
                        GH_Convert.ToBrep(gbrep, ref rs, 0);
                        double tol = 0.00001;
                        GH_ObjectWrapper attrWrapper = null;
                        Color tsColor = Color.Empty;
                        Transparency trnsp = Transparency.Empty;
                        ShapeList shape;

                        DA.GetData("Tolerance", ref tol);
                        DA.GetData("TSAttributes", ref attrWrapper);
                        string layerName = "";
                        var tsAttributes = attrWrapper.Value as Tuple<Transparency, Color, string>;

                        if (!(tsAttributes is null))
                        {
                            tsColor = tsAttributes.Item2;
                            trnsp = tsAttributes.Item1;
                            layerName = tsAttributes.Item3;

                        }

                        shape = rs.ToHost(tol);

                        EntitiesCreation shapesCreation = new EntitiesCreation(doc, 0);
                        Layer layer = new Layer(-1);
                        LayerEntity layEnt = new LayerEntity(doc, 0, layer);

                        var layfoldEnt = LayersFolderEntity.GetOrCreateFolder(doc);
                        layEnt = layfoldEnt.SearchLayer(layerName);



                    foreach (var ts in shape)
                    {
                        ShapeEntity se = new ShapeEntity(doc, 0);
                        se.Geometry = ts;
                        se.ExplicitColor = tsColor;
                        se.ExplicitTransparency = trnsp;
                        se.ExplicitLayer = layEnt.Layer;


                        se.Create(doc.ShapesFolderEntity);
                        shapesCreation.AddChildEntity(se);
                        shapesCreation.CanDeleteFromChild(se);

                    }
                    SewOperation sewOperation = new SewOperation(doc, 0);
                    //if (sew)
                    //    sewOperation.AddOperation(shapesCreation);

                    shapesCreation.Create();


                    if (sew)
                    {
                        try
                        {
                            layfoldEnt.AddLayer(layer, layerName);
                            layEnt = layfoldEnt.SearchLayer(layerName);
                        }

                        var shapesfolder = doc.ShapesFolderEntity;

                        localPart.NodeEntity.IsDeletable = true;
                        foreach (var ts in shape)
                        {
                            ShapeEntity se = new ShapeEntity(doc, 0);
                            se.SetGeometry(ts, true);
                            //se.ExplicitColor = tsColor;
                            //se.ExplicitTransparency = trnsp;
                            //se.ExplicitLayer = layEnt.Layer;

                            //localPart.NodeEntity.AddGeometry(se);

                            shapesCreation.AddChildEntity(se);
                            //shapesCreation.CanDeleteFromChild(se);
                            //shapesfolder.AddEntity(se);
                        }
                        //if (sew)
                        //    sewOperation.AddOperation(shapesCreation);

                        shapesCreation.Create();


                        if (sew)
                        {
                            SewOperation sewOperation = new SewOperation(doc, 0);
                            try
                            {
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
                                //sewOperation.Display.DefaultLayer = layer;
                                //sewOperation.Display.DefaultColor = tsColor;
                                //sewOperation.Display.DefaultTransparency = trnsp;
                                //sewOperation.Parent = shapesCreation;
                                //sewOperation.AddOperation(shapesCreation);
                                //shapesCreation.Parent = sewOperation;
                                sewOperation.Create();
                                entity = shapesCreation.ChildrenEntities.ElementAt(0);
                                if (name != null)
                                {

                                    entity.Name = name.ToString();
                                    entity.ExplicitLayer = layer;
                                    entity.ExplicitColor = tsColor;
                                    entity.ExplicitTransparency = trnsp;
                                }

                                if (Params.Output.Count > 0)
                                    DA.SetData("TopSolid Entities", shapesCreation.ChildrenEntities.ElementAt(0));
                                //list.Add(shapesCreation.ChildrenEntities.ElementAt(0));
                                entities.Add(entity);
                                localPart.NodeEntity.SetLocalConstituents(entities);

                                // Ajout de la ShapeEntity dans les représentations de la pièce locale
                                localPart.NodeEntity.AddEntityToLocalRepresentation(entity, Design.DB.Documents.ElementName.DetailedRepresentation);
                                localPart.NodeEntity.AddEntityToLocalRepresentation(entity, Design.DB.Documents.ElementName.DesignRepresentation);
                                localPart.NodeEntity.AddEntityToLocalRepresentation(entity, Design.DB.Documents.ElementName.SimplifiedRepresentation);



                                // Set de la géométrie
                                localPart.NodeEntity.NotifyModifying(true);
                            }
                            //TODO Handle exception just in case
                            catch (Exception ex)
                            {
                            }
                            if (Params.Output.Count > 0)
                                DA.SetData("TopSolid Entities", shapesCreation.ChildrenEntities.ElementAt(0));
                            //list.Add(shapesCreation.ChildrenEntities.ElementAt(0));
                        }
                        //TODO Handle exception just in case
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    if (Params.Output.Count > 0)

                        DA.SetData(0, entity); //TODO Generalize

                }
                //*/


            }

        }


        protected override void AfterSolveInstance()
        {

            //base.AfterSolveInstance();
            UndoSequence.End();
            if (doc != null) doc.Update(true, false);
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
