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

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometries", "G", "Geometries to bake", GH_ParamAccess.list);
            pManager.AddGenericParameter("TSDocument", "TSDoc", "target TopSolid Document to bake-in", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddGenericParameter("TSAttributes", "attributes", "TopSolid's attributes for the baked entities", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "Tol", "Tolerance for bake", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Sew?", "Sew?", "True to Sew Breps, False to keep faces split", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Bake?", "b?", "Set true to bake", GH_ParamAccess.item);

        }
        EntityList list = new EntityList();

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
            List<IGH_GeometricGoo> geo = new List<IGH_GeometricGoo>();
            bool run = false;
            bool sew = false;
            if (!DA.GetDataList(0, geo)) { return; }
            if (geo == null) { return; }
            if (geo.Count == 0) { return; }

            DA.GetData("Bake?", ref run);
            DA.GetData("Sew?", ref sew);


            //Setting target document from input, or else take current document by default
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();
            ModelingDocument doc = null;
            if (DA.GetData("TSDocument", ref wrapper))
            {
                if (wrapper.Value is string)
                    doc = TopSolid.Kernel.TX.Documents.DocumentStore.Documents.Where(x => x.FileName == wrapper.Value.ToString()).FirstOrDefault() as ModelingDocument;
                if (wrapper.Value is IDocumentItem)
                    doc = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as ModelingDocument;
                if (doc is null)
                    doc = wrapper.Value as ModelingDocument;
            }

            if (doc == null)
                doc = TopSolid.Kernel.UI.Application.CurrentDocument as ModelingDocument;

            if (run == true)
            {
                UndoSequence.UndoCurrent();
                UndoSequence.Start("Bake", true);
                list.Clear();

                foreach (var g in geo)
                {
                    if (g is GH_Point gp)
                    {
                        var rp = new Point3d();
                        GH_Convert.ToPoint3d(gp, ref rp, 0);
                        var tp = rp.ToHost();
                        PointEntity pe = new PointEntity(doc, 0);
                        pe.Geometry = tp;
                        pe.Create(doc.PointsFolderEntity);
                        list.Add(pe);
                    }

                    if (g is GH_Plane ghPlane)
                    {
                        var rhPlane = new Plane();
                        GH_Convert.ToPlane(ghPlane, ref rhPlane, 0);
                        var tp = rhPlane.ToHost();
                        PlaneEntity pe = new PlaneEntity(doc, 0);
                        pe.Geometry = tp;
                        pe.Create(doc.PlanesFolderEntity);
                    }
                    else if (g is GH_Curve gc)
                    {
                        Curve rc = null;
                        GH_Convert.ToCurve(gc, ref rc, 0);
                        var rn = rc.ToNurbsCurve();
                        var tc = rn.ToHost();
                        CurveEntity ce = new CurveEntity(doc, 0);
                        ce.Geometry = tc;
                        ce.Create(doc.PointsFolderEntity);
                        list.Add(ce);
                    }
                    else if (g is GH_Surface gs)
                    {
                        Surface rs = null;
                        GH_Convert.ToSurface(gs, ref rs, 0);
                        var rn = rs.ToNurbsSurface();
                        var ts = rn.ToHost();
                        SurfaceEntity se = new SurfaceEntity(doc, 0);
                        se.Geometry = ts;
                        se.Create(doc.PointsFolderEntity);
                        list.Add(se);
                    }

                    else if (g is GH_Brep gbrep)
                    {
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

                        if (layEnt is null)
                        {
                            layfoldEnt.AddLayer(layer, layerName);
                            layEnt = layfoldEnt.SearchLayer(layerName);
                        }


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

                        shapesCreation.Create();

                        if (sew)
                        {
                            try
                            {
                                SewOperation sewOperation = new SewOperation(doc, 0);
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
                                sewOperation.Create();
                                list.Add(shapesCreation.ChildrenEntities.ElementAt(0));
                            }
                            //TODO Handle exception just in case
                            catch
                            {
                            }
                        }


                        //TODO
                        //ShapeEntity se = new ShapeEntity(doc, 0);
                        //se.Geometry = shape;
                        //se.Create(doc.ShapesFolderEntity);



                    }
                }
                UndoSequence.End();

            }
            if (Params.Output.Count > 0)
                DA.SetDataList("TopSolid Entities", list.ToList());
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            if ((side == GH_ParameterSide.Input) && ((index == 2 && Params.Input.Count == 2)
                || (Params.Input.Count == 3 && index == 3 && Params.Input[2] is Param_Number)
                || (index == 2 && Params.Input.Count == 3 && Params.Input[2] is Param_Colour)))
                return true;
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
                param.Access = GH_ParamAccess.list;

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
