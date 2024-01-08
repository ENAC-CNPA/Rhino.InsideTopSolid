using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using TopSolid.Kernel.DB.D3.Curves;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.GR.Attributes;
using TopSolid.Kernel.GR.D3;
using TopSolid.Kernel.GR.Displays;
using TopSolid.Kernel.SX.Drawing;
using TopSolid.Kernel.TX.Items;

namespace EPFL.GrasshopperTopSolid.Components
{
    public class TSPreview : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TSPreview class.
        /// </summary>
        public TSPreview()
          : base("TopSolid Preview", "TSPreview",
              "Preview Geometries in TopSolid",
              "TopSolid", "To TopSolid")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometries", "G", "Geometries to display", GH_ParamAccess.item);
            pManager.AddColourParameter("Colour", "C", "Preview Colour in TopSolid", GH_ParamAccess.item);
            pManager[1].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            gd.Clear();
            base.RemovedFromDocument(document);
        }

        private GeometricDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;
        private GeneralDisplay gd = new GeneralDisplay(null);

        protected override void BeforeSolveInstance()
        {
            gd.Clear();
            base.BeforeSolveInstance();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_GeometricGoo geo = null;

            if (!DA.GetData(0, ref geo)) { return; }

            if (geo == null) { return; }
            //if (geo.Count == 0) { return; }


            if (!doc.Display.ContainsDisplay(gd))
            {
                doc.Display.AddDisplay(gd);
            }

            var tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            GH_Colour color = null;
            Color tsColor = Color.Empty;
            Transparency trnsp = Transparency.Empty;

            DA.GetData("Colour", ref color);

            if (color != null)
            {
                float h = color.Value.GetHue();
                float s = color.Value.GetSaturation();
                float l = color.Value.GetBrightness();


                tsColor = Color.FromHLS(h, l, s);
                trnsp = Transparency.FromByte((byte)(byte.MaxValue - color.Value.A));

            }

            //foreach (var g in geo)
            //{
            var g = geo;

            if (g is GH_Point gp)
            {
                var rp = new Point3d();
                GH_Convert.ToPoint3d(gp, ref rp, 0);
                var tp = rp.ToHost();
                MarkerItem mi = new MarkerItem(tp);
                //mi.Color = Color.Green;
                mi.Color = tsColor;
                mi.Transparency = trnsp;
                mi.MarkerStyle = MarkerStyle.ExtraLargePlus;

                gd.Add(mi);
            }
            else if (g is GH_Line gl)
            {
                var rl = new Line();
                GH_Convert.ToLine(gl, ref rl, 0);
                var tl = rl.ToHost();
                LineItem li = new LineItem(tl.Ps, tl.Pe);
                //li.Color = Color.Green;
                li.Color = tsColor;
                li.LineStyle = LineStyle.SolidMedium;
                li.Transparency = trnsp;
                gd.Add(li);
            }
            else if (g is GH_Curve gc)
            {


                Curve rc = null;
                GH_Convert.ToCurve(gc, ref rc, 0);
                Rhino.Geometry.Point3d[] points;

                //if (rc.IsLinear() || rc.Degree == 1)
                //{
                //    LineItem li = new LineItem(rc.PointAtStart.ToHost(), rc.PointAtEnd.ToHost());
                //    li.Color = Color.Green;
                //    li.LineStyle = LineStyle.SolidMedium;
                //    gd.Add(li);
                //}

                if (rc.IsPolyline())
                {
                    var pline = rc.DuplicateSegments();
                    foreach (var seg in pline)
                    {
                        LineItem li = new LineItem(seg.PointAtStart.ToHost(), seg.PointAtEnd.ToHost());
                        li.Color = tsColor;
                        li.LineStyle = LineStyle.SolidMedium;
                        li.Transparency = trnsp;
                        gd.Add(li);
                    }
                }

                else
                {
                    var crvs = rc.DivideByLength(tol * 10, true, out points);
                    if (points == null)
                        return;
                    for (int i = 0; i < points.Length; i++)
                    {
                        if (i < points.Length - 1)
                        {
                            LineItem li = new LineItem(points[i].ToHost(), points[i + 1].ToHost());
                            //li.Color = Color.Green;
                            li.Color = tsColor;
                            li.LineStyle = LineStyle.SolidMedium;
                            li.Transparency = trnsp;

                            gd.Add(li);
                        }
                        else if (i == points.Length - 1 && rc.IsClosed)
                        {
                            LineItem li = new LineItem(points[i].ToHost(), points[0].ToHost());
                            //li.Color = Color.Green;
                            li.Color = tsColor;
                            li.LineStyle = LineStyle.SolidMedium;
                            li.Transparency = trnsp;
                            gd.Add(li);
                        }
                    }
                }

            }

            else if (g is GH_Surface srf)
            {
                Rhino.Geometry.Surface _srf = null;
                GH_Convert.ToSurface(srf, ref _srf, GH_Conversion.Both);

                Mesh[] meshes = Mesh.CreateFromBrep(_srf.ToBrep(), MeshingParameters.Default);

                foreach (Mesh mesh in meshes)
                {
                    mesh.Faces.ConvertQuadsToTriangles();
                    int faceind = 0;
                    foreach (var f in mesh.Faces)
                    {
                        List<TopSolid.Kernel.G.S.D3.Point> vertList = new List<TopSolid.Kernel.G.S.D3.Point>();

                        FaceItemMaker maker = new FaceItemMaker();
                        //maker.Color = Color.Green;
                        maker.Color = tsColor;


                        vertList.Add(new TopSolid.Kernel.G.S.D3.Point(mesh.Vertices[f.A].X, mesh.Vertices[f.A].Y, mesh.Vertices[f.A].Z));
                        vertList.Add(new TopSolid.Kernel.G.S.D3.Point(mesh.Vertices[f.B].X, mesh.Vertices[f.B].Y, mesh.Vertices[f.B].Z));
                        vertList.Add(new TopSolid.Kernel.G.S.D3.Point(mesh.Vertices[f.C].X, mesh.Vertices[f.C].Y, mesh.Vertices[f.C].Z));
                        if (f.D != f.C)
                        {
                            vertList.Add(new TopSolid.Kernel.G.S.D3.Point(mesh.Vertices[f.D].X, mesh.Vertices[f.D].Y, mesh.Vertices[f.D].Z));
                        }


                        mesh.FaceNormals.ComputeFaceNormals();
                        mesh.FaceNormals[faceind].Unitize();
                        FaceItem faceitem = maker.Make(vertList, ItemLabel.Empty, 0, new TopSolid.Kernel.G.S.D3.UnitVector(mesh.FaceNormals[faceind].X, mesh.FaceNormals[faceind].Y, mesh.FaceNormals[faceind].Z));
                        faceitem.LineStyle = LineStyle.SolidMedium;
                        faceitem.Transparency = trnsp;
                        gd.Add(faceitem);
                        faceind++;
                    }
                }




                //var x = Convert.ToHost(_srf.ToNurbsSurface());

                //TopSolid.Kernel.DB.D3.Surfaces.SurfaceEntity srfentity = new TopSolid.Kernel.DB.D3.Surfaces.SurfaceEntity(doc, 0);
                //srfentity.Geometry = x;
                //if (srfentity.Display.Items.Count != 0)
                //{
                //    for (int i = 0; i < srfentity.Display.Items.Count; i++)
                //    { gd.Add(srfentity.Display.Items.ElementAt(i)); }
                //}
            }

            else if (g is GH_Brep brep)
            {
                Rhino.Geometry.Brep _brep = null;
                GH_Convert.ToBrep(brep, ref _brep, GH_Conversion.Both);

                Mesh[] meshes = Mesh.CreateFromBrep(_brep, MeshingParameters.Default);

                foreach (Mesh mesh in meshes)
                {
                    mesh.Faces.ConvertQuadsToTriangles();
                    int faceind = 0;
                    foreach (var f in mesh.Faces)
                    {
                        List<TopSolid.Kernel.G.S.D3.Point> vertList = new List<TopSolid.Kernel.G.S.D3.Point>();

                        FaceItemMaker maker = new FaceItemMaker();
                        //maker.Color = Color.Green;
                        maker.Color = tsColor;


                        vertList.Add(new TopSolid.Kernel.G.S.D3.Point(mesh.Vertices[f.A].X, mesh.Vertices[f.A].Y, mesh.Vertices[f.A].Z));
                        vertList.Add(new TopSolid.Kernel.G.S.D3.Point(mesh.Vertices[f.B].X, mesh.Vertices[f.B].Y, mesh.Vertices[f.B].Z));
                        vertList.Add(new TopSolid.Kernel.G.S.D3.Point(mesh.Vertices[f.C].X, mesh.Vertices[f.C].Y, mesh.Vertices[f.C].Z));
                        if (f.D != f.C)
                        {
                            vertList.Add(new TopSolid.Kernel.G.S.D3.Point(mesh.Vertices[f.D].X, mesh.Vertices[f.D].Y, mesh.Vertices[f.D].Z));
                        }



                        mesh.FaceNormals.ComputeFaceNormals();
                        mesh.FaceNormals[faceind].Unitize();
                        FaceItem faceitem = maker.Make(vertList, ItemLabel.Empty, 0, new TopSolid.Kernel.G.S.D3.UnitVector(mesh.FaceNormals[faceind].X, mesh.FaceNormals[faceind].Y, mesh.FaceNormals[faceind].Z));
                        faceitem.LineStyle = LineStyle.SolidMedium;
                        faceitem.Transparency = trnsp;
                        gd.Add(faceitem);
                        faceind++;
                    }
                }

                //var x = Convert.ToHost(_brep);
                //foreach (var s in x)
                //{
                //    //TODO
                //    TopSolid.Kernel.DB.D3.Shapes.ShapeEntity shapeentity = new TopSolid.Kernel.DB.D3.Shapes.ShapeEntity(doc, 0);
                //    shapeentity.Geometry = s;
                //    if (shapeentity.Display.Items.Count != 0)
                //    {
                //        for (int i = 0; i < shapeentity.Display.Items.Count; i++)
                //        { gd.Add(shapeentity.Display.Items.ElementAt(i)); }
                //    }

                //}



            }
            //}
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.PreviewWindow_Visualize, 24, 24).ToBitmap();



        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("99F6EBE4-7343-4F9E-9715-26E3EE51400B"); }
        }
    }
}
