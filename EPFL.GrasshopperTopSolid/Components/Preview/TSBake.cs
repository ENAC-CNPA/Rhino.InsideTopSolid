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

namespace EPFL.GrasshopperTopSolid.Components
{
    public class TSBake : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the TSBake class.
        /// </summary>
        public TSBake()
          : base("TopSolid Bake", "TSBake",
              "Bake Geometries",
              "TopSolid", "Preview")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometries", "G", "Geometries to bake", GH_ParamAccess.list);
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
            List<IGH_GeometricGoo> geo = new List<IGH_GeometricGoo>();
            bool run = false;

            if (!DA.GetDataList(0, geo)) { return; }

            if (geo == null) { return; }
            if (geo.Count == 0) { return; }

            DA.GetData(1, ref run);

            //GeometricDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;
            ModelingDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as ModelingDocument;


            if (run == true)
            {
                UndoSequence.UndoCurrent();
                UndoSequence.Start("Bake", true);

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
                    }

                    else if (g is GH_Brep gbrep)
                    {
                        Brep rs = null;
                        GH_Convert.ToBrep(gbrep, ref rs, 0);
                        double tol = 0;
                        GH_Colour color = null;
                        Color tsColor = Color.Empty;
                        Transparency trnsp = Transparency.Empty;
                        ShapeList shape;
                        if (Params.Input.Count >= 3)
                        {
                            if (DA.GetData(2, ref tol) && DA.GetData(3, ref color))
                            {
                                shape = rs.ToHost(tol);
                                if (color != null)
                                {
                                    float h = color.Value.GetHue();
                                    float s = color.Value.GetSaturation();
                                    float l = color.Value.GetBrightness();


                                    tsColor = Color.FromHLS(h, l, s);
                                    trnsp = Transparency.FromByte((byte)(byte.MaxValue - color.Value.A));

                                }
                            }

                            else if (DA.GetData(2, ref tol))
                            {
                                shape = rs.ToHost(tol);
                            }
                            else if (DA.GetData(2, ref color))
                            {
                                shape = rs.ToHost();
                                if (color != null)
                                {
                                    float h = color.Value.GetHue();
                                    float s = color.Value.GetSaturation();
                                    float l = color.Value.GetBrightness();


                                    tsColor = Color.FromHLS(h, l, s);
                                    trnsp = Transparency.FromByte((byte)(byte.MaxValue - color.Value.A));

                                }
                            }
                            else return;
                        }
                        //else if (Params.Input.Count == 4)
                        //{
                        //    if (DA.GetData(3, ref color) && (DA.GetData(2, ref tol)))
                        //    {
                        //        shape = rs.ToHost(tol);
                        //        if (color != null)
                        //        {
                        //            float h = color.Value.GetHue();
                        //            float s = color.Value.GetSaturation();
                        //            float l = color.Value.GetBrightness();


                        //            tsColor = Color.FromHLS(h, l, s);
                        //            trnsp = Transparency.FromByte((byte)(byte.MaxValue - color.Value.A));

                        //        }
                        //    }
                        //    else return;
                        //}
                        else
                        {
                            shape = rs.ToHost();
                        }

                        EntitiesCreation shapesCreation = new EntitiesCreation(doc, 0);


                        foreach (var ts in shape)
                        {
                            ShapeEntity se = new ShapeEntity(doc, 0);
                            se.Geometry = ts;
                            se.ExplicitColor = tsColor;
                            se.ExplicitTransparency = trnsp;

                            se.Create(doc.ShapesFolderEntity);
                            shapesCreation.AddChildEntity(se);
                            shapesCreation.CanDeleteFromChild(se);

                        }

                        shapesCreation.Create();

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
                            sewOperation.Create();

                            //Hides other shapes when successfull, otherwise keep them shown
                            bool isInvalid = sewOperation.IsInvalid;
                            if (!isInvalid)
                            {
                                for (int i = 1; i < shapesCreation.ChildEntityCount; i++)
                                {
                                    shapesCreation.ChildrenEntities.ElementAt(i).Hide();
                                    //shapesCreation.ChildrenEntities.ElementAt(i).IsGhost = true;//To Comment in case Debug is needed
                                }
                            }

                        }

                        catch
                        {

                        }

                        //TODO
                        //ShapeEntity se = new ShapeEntity(doc, 0);
                        //se.Geometry = shape;
                        //se.Create(doc.ShapesFolderEntity);

                    }
                }
                UndoSequence.End();
            }
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            if ((side == GH_ParameterSide.Input) && ((index == 2 && Params.Input.Count == 2) || (Params.Input.Count == 3 && index == 3 && Params.Input[2] is Param_Number) || (index == 2 && Params.Input.Count == 3 && Params.Input[2] is Param_Colour)))
                return true;

            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input && index > 1)
                return true;
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            if (index == 2 && (Params.Input.Count == 2 || (Params.Input.Count == 3 && Params.Input[2] is Param_Colour)))
            {
                var param = new Param_Number();
                param.Name = "Tolerance";
                param.Optional = true;
                param.NickName = "Tol";
                param.Description = "Tolerance for TopSolid conversion and sewing";
                return param;
            }

            else if (index == 3)
            {
                var param = new Param_Colour();
                param.Name = "Colour";
                param.Optional = true;
                param.NickName = "Colour";
                param.Description = "Color for TopSolid Baked Geometry";
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
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Geometrie_Old;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8C5F0890-91DE-43CE-A10C-58A26774DF8D"); }
        }
    }
}
