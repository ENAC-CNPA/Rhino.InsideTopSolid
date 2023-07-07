﻿using Rhino.Geometry;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.G.D3.Surfaces;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.SX.Collections;

using TSXGen = TopSolid.Kernel.SX.Collections.Generic;
using TKGD2 = TopSolid.Kernel.G.D2;
using TKGD3 = TopSolid.Kernel.G.D3;
using NLog.Fluent;
using TopSolid.Kernel.G.D3.Shapes.Creations;
using TopSolid.Kernel.G;
using TopSolid.Kernel.TX.Items;
using TopSolid.Kernel.WX;
using SX = TopSolid.Kernel.SX;
using TX = TopSolid.Kernel.TX;
using G = TopSolid.Kernel.G;
using TopSolid.Kernel.G.D3.Shapes.Sew;
using Eto.Forms;
using TopSolid.Cad.Design.DB.Constraints;
using TopSolid.Kernel.G.D3.Shapes.Healing;

namespace EPFL.GrasshopperTopSolid
{
    static class BrepConvert
    {

        #region ToRhino
        static internal Brep FaceToBrep(this Face face)
        {
            //Create the *out* variables
            BoolList outer = new BoolList();
            TSXGen.List<TKGD2.Curves.IGeometricProfile> list2D = new TSXGen.List<TKGD2.Curves.IGeometricProfile>();
            TSXGen.List<TKGD3.Curves.IGeometricProfile> list3D = new TSXGen.List<TKGD3.Curves.IGeometricProfile>();
            TSXGen.List<EdgeList> listEdges = new TSXGen.List<EdgeList>();
            //TSXGen.List<TKGD3.Shapes.Vertex> vertexlist = new TSXGen.List<TKGD3.Shapes.Vertex>();
            double tol_Rh = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            double tol_TS = TopSolid.Kernel.G.Precision.ModelingLinearTolerance;
            bool forcesNonPeriodic = false;
            if (face.GeometryType == SurfaceGeometryType.Cone || face.GeometryType == SurfaceGeometryType.Cylinder)
                forcesNonPeriodic = true;

            //Topology indexes ?
            int c_index = 0;

            //Create the Brep Surface
            Brep brepsrf = new Brep();

            //function added on request, gets the 2DCurves, 3dCurves and Edges in the correct order
            OrientedSurface osurf = face.GetOrientedBsplineTrimmedGeometry(tol_TS, false, false, forcesNonPeriodic, FaceTrimmingLoopsConfine.Periph, outer, list2D, list3D, listEdges, true);
            TKGD3.Surfaces.Surface topSolidSurface = osurf.Surface;

            var list2Dflat = list2D.SelectMany(f => f.Segments).ToList();
            var list3Dflat = list3D.SelectMany(f => f.Segments).ToList();
            var listEdgesflat = listEdges.SelectMany(m => m).ToList();

            //For Debug
            bool correct = CheckTopologicalCoherence(list2D, list3D, listEdges, face.LoopCount);
            var geotype = face.GeometryType;

            int countTopo = 0;
            while (!correct && countTopo < 4)
            {
                FaceTrimmingLoopsConfine trimConfine = (FaceTrimmingLoopsConfine)countTopo;
                Console.WriteLine("Error");
                list2D.Clear();
                list3D.Clear();
                listEdges.Clear();
                osurf = face.GetOrientedBsplineTrimmedGeometry(tol_TS, false, false, forcesNonPeriodic, trimConfine, outer, list2D, list3D, listEdges, true);

                list2Dflat = list2D.SelectMany(f => f.Segments).ToList();
                list3Dflat = list3D.SelectMany(f => f.Segments).ToList();
                listEdgesflat = listEdges.SelectMany(m => m).ToList();
                correct = CheckTopologicalCoherence(list2D, list3D, listEdges, face.LoopCount);
                countTopo++;
            }

            //Add Vertices
            int loopIndexforVertex = 0;
            int indexInLoop = 0;
            //int vertexIndex = 0;
            foreach (var Curve in list3D)
            {
                foreach (var seg in Curve.Segments)
                {
                    //if (e.IsEmpty)
                    //{
                    //    var rhinoPoint = list3D[loopIndexforVertex].Segments.ElementAt(indexInLoop).Ps.ToRhino();
                    //    brepsrf.Vertices.Add(rhinoPoint, tol_TS);
                    //    //list3D[loopIndexforVertex].Segments.ElementAt(indexInLoop).Ps
                    //    continue;
                    //}
                    //TODO UNCOMMENT as soon as error with IsReversedwithFin is resolved
                    //if (e.IsReversedWithFin(face))
                    //{
                    //    brepsrf.Vertices.Add(e.EndVertex.GetGeometry().ToRhino(), tol_TS);
                    //}
                    //else
                    //{
                    //    brepsrf.Vertices.Add(e.StartVertex.GetGeometry().ToRhino(), tol_TS);
                    //}
                    //indexInLoop++;
                    //vertexIndex++;

                    brepsrf.Vertices.Add(seg.Ps.ToRhino(), tol_TS);
                }
                loopIndexforVertex++;
                indexInLoop = 0;
            }


            //Get the 3D Curves and convert them to Rhino            
            int ind = 0;
            int indperLoop = 0;
            //bool rev;
            foreach (TKGD3.Curves.IGeometricProfile c in list3D)
            {

                foreach (TKGD3.Curves.IGeometricSegment ic in c.Segments)
                {
                    var orientedcrv = ic.GetOrientedCurve();
                    var crv = orientedcrv.Curve.ToRhino();
                    //var crvi = ic.GetOrientedCurve().Curve;
                    //AHW commented to prevent error, check if it affects good conversion

                    if (orientedcrv.IsReversed)
                    {
                        crv.Reverse();
                    }

                    c_index = brepsrf.AddEdgeCurve(crv);
                    indperLoop++;
                }
                ind++;
                indperLoop = 0;
            }

            //Edges
            int i = 0;
            int iperLoop = 0;
            System.Collections.Generic.List<BrepEdge> rhinoEdges = new System.Collections.Generic.List<BrepEdge>();
            foreach (EdgeList list in listEdges)
            {
                foreach (Edge e in list)
                {
                    if (e.IsEmpty)
                    {
                        rhinoEdges.Add(brepsrf.Edges.Add(i));
                    }

                    else
                    {
                        if (iperLoop + 1 == list.Count)
                        {
                            if (e.VertexCount == 0)
                            {
                                rhinoEdges.Add(brepsrf.Edges.Add(i));
                            }

                            else if (e.VertexCount != 2)
                            {
                                rhinoEdges.Add(brepsrf.Edges.Add(i, i, i, tol_TS));
                            }
                            else
                            {
                                //TODO UNCOMMENT
                                if (e.IsReversedWithFin(face))
                                    rhinoEdges.Add(brepsrf.Edges.Add(i - list.Count + 1, i, i, tol_TS));
                                else
                                    rhinoEdges.Add(brepsrf.Edges.Add(i, i - list.Count + 1, i, tol_TS));
                            }
                        }
                        else
                        {
                            if (e.VertexCount == 0)
                            {
                                rhinoEdges.Add(brepsrf.Edges.Add(i));
                            }
                            else if (e.VertexCount != 2)
                            {
                                rhinoEdges.Add(brepsrf.Edges.Add(i, i, i, tol_TS));
                            }
                            // TODO UnComment
                            else
                            {
                                if (e.IsReversedWithFin(face))
                                    rhinoEdges.Add(brepsrf.Edges.Add(i + 1, i, i, tol_TS));
                                else
                                    rhinoEdges.Add(brepsrf.Edges.Add(i, i + 1, i, tol_TS));
                            }
                        }
                    }
                    i++;
                    iperLoop++;
                }
                iperLoop = 0;

            }



            int loopindex = 0;
            System.Collections.Generic.List<BrepTrim> rhTrim = new System.Collections.Generic.List<BrepTrim>();
            brepsrf.AddSurface(topSolidSurface.ToRhino());
            BrepFace bface = brepsrf.Faces.Add(0);
            BrepLoop rhinoLoop = null;

            //Get the 2D Curves and convert them to Rhino
            int x = 0;
            foreach (TKGD2.Curves.IGeometricProfile c in list2D)
            {
                var tsloop = face.Loops.ElementAt(loopindex);

                if (tsloop.IsOuter)
                    rhinoLoop = brepsrf.Loops.Add(BrepLoopType.Outer, bface);
                else
                    rhinoLoop = brepsrf.Loops.Add(BrepLoopType.Inner, bface);


                foreach (TKGD2.Curves.IGeometricSegment ic in c.Segments)
                {
                    Rhino.Geometry.Curve crv;
                    TKGD2.Curves.BSplineCurve tcrvv = ic.GetOrientedCurve().Curve.GetBSplineCurve(false, false);

                    if (tsloop.IsOuter)
                    {
                        if (ic.IsReversed)
                        {
                            tcrvv.Reverse();
                        }

                        crv = Convert.ToRhino(tcrvv);
                        x = brepsrf.AddTrimCurve(crv);

                        if (x < rhinoEdges.Count)
                            rhTrim.Add(brepsrf.Trims.Add(rhinoEdges[x], ic.IsReversed, rhinoLoop, x));
                        else
                        {
                            BrepEdge brepEdge = brepsrf.Edges.Add(x);
                            rhTrim.Add(brepsrf.Trims.Add(brepEdge, ic.IsReversed, rhinoLoop, x));
                        }
                    }

                    else
                    {
                        //AHW TODO unComment
                        if (ic.IsReversed)
                        {
                            tcrvv.Reverse();
                        }

                        crv = Convert.ToRhino(tcrvv);
                        x = brepsrf.AddTrimCurve(crv);
                        if (x < rhinoEdges.Count)
                        {
                            var trim = brepsrf.Trims.Add(rhinoEdges[x], ic.IsReversed, rhinoLoop, x);

                            rhTrim.Add(trim);
                        }
                        else
                        {
                            BrepEdge brepEdge = brepsrf.Edges.Add(x);
                            rhTrim.Add(brepsrf.Trims.Add(brepEdge, ic.IsReversed, rhinoLoop, x));
                        }

                    }

                    if (x < rhTrim.Count)
                    {
                        rhTrim[x].TrimType = BrepTrimType.Boundary;
                        rhTrim[x].IsoStatus = IsoStatus.None;
                        string log1 = null;
                        rhTrim[x].IsValidWithLog(out log1);
                    }
                    x++;
                }
                loopindex++;
            }


            if (osurf.IsReversed)
            {
                brepsrf.Faces.First().OrientationIsReversed = true;
            }

            string log = null;
            brepsrf.IsValidWithLog(out log);


            bool match = true;
            if (!brepsrf.IsValid)
            {
                brepsrf.Repair(tol_TS);
                brepsrf.IsValidWithLog(out log);
                match = brepsrf.Trims.MatchEnds();
                brepsrf.IsValidWithLog(out log);
            }

            brepsrf.SetTolerancesBoxesAndFlags(false, true, true, true, true, false, false, false);

            if (!match || !brepsrf.IsValid)
            {
                brepsrf.Repair(tol_TS);
                //brepsrf.IsValidWithLog(out log);
                match = brepsrf.Trims.MatchEnds();
            }
            return brepsrf;

        }

        static public Brep[] ToRhino(this Shape shape)
        {
            System.Collections.Generic.List<Brep> listofBrepsrf = new System.Collections.Generic.List<Brep>();
            Brep brep = new Brep();

            foreach (Face f in shape.Faces)
            {
                brep = f.FaceToBrep();

                //if (!brep.IsValid)
                //{
                //    brep = new Brep();
                //}
                listofBrepsrf.Add(brep);
            }

            var result = Brep.JoinBreps(listofBrepsrf, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            foreach (Brep b in result)
            {
                b.Trims.MatchEnds();
                b.Repair(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            }

            for (int i = 0; i < result.Length; i++)
            {
                result[i].Repair(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            }

            return result;

        }

        //for Debug and to prevent errors
        /// <summary>
        /// for debug, returns true if 3D segments = 2D segments = edges
        /// </summary>
        /// <param name="list2D">list of 2D profiles</param>
        /// <param name="list3D">list of 3D profiles</param>
        /// <param name="listEdges">list of EdgeList with loops</param>
        /// <param name="loopCount">Face.LoopCount</param>
        /// <returns></returns>
        static public bool CheckTopologicalCoherence(TSXGen.List<TKGD2.Curves.IGeometricProfile> list2D,
        TSXGen.List<TKGD3.Curves.IGeometricProfile> list3D,
        TSXGen.List<EdgeList> listEdges, int loopCount)
        {
            IEnumerable<TKGD2.Curves.GeometricSegment> list2dSegmentsFlattened = list2D.SelectMany(x => x.Segments);
            IEnumerable<TKGD3.Curves.IGeometricSegment> list3dSgementsFlattened = list3D.SelectMany(x => x.Segments);
            IEnumerable<Edge> listEdgesFlattened = listEdges.SelectMany(x => x);

            int count2dProfiles = list2D.Count;
            int count2dSegmentsFlattened = list2dSegmentsFlattened.Count();

            int count3dProfiles = list3D.Count;
            int count3dSegmentsFlattened = list3dSgementsFlattened.Count();

            int coundEdges = listEdges.Count;
            int countEdgesFlattened = listEdgesFlattened.Count();


            bool profilesEqualsLoopCount = count2dProfiles == count3dProfiles && count2dProfiles == loopCount && count3dProfiles == coundEdges;
            bool flattenedequals = count2dSegmentsFlattened == count3dSegmentsFlattened && count2dSegmentsFlattened == countEdgesFlattened;

            bool result = profilesEqualsLoopCount && flattenedequals;
            if (!result)
                Console.WriteLine("problem !!");
            return result;
        }


        #endregion

        #region ToHost
        /// <summary>
        /// Converts a Rhino Brep to a TopSolid Shape
        /// </summary>
        /// <param name="brep"> Rhino Brep to convert</param>      
        /// <returns></returns>
        static public Shape ToHost(this Brep brep)
        {
            double tol_TS = G.Precision.ModelingLinearTolerance;
            brep.Trims.MatchEnds();
            brep.Repair(tol_TS);
            Shape shape = null;
            ShapeList ioShapes = new ShapeList();

            if (brep.IsValid)
            {
                foreach (BrepFace bface in brep.Faces)
                {
                    shape = null;

                    shape = MakeSheetFrom3d(brep, bface, tol_TS);

                    if (shape == null || shape.IsEmpty)
                        shape = MakeSheetFrom2d(brep, bface, tol_TS);

                    if (shape == null || shape.IsEmpty)
                    {
                        shape = MakeSheet(brep, bface);
                        TopSolid.Kernel.WX.MessageBox.Show("Face not limited.");
                    }

                    if (shape == null || shape.IsEmpty)
                    { }
                    else
                        ioShapes.Add(shape);
                }
            }

            Shape finalShape = ioShapes.SewShapes();
            ShapeSimplifier simplifier = new ShapeSimplifier(SX.Version.Current, tol_TS);
            simplifier.Simplify(finalShape, ItemOperationKey.BasicKey);
            return finalShape;
        }

        private static Shape MakeSheetFrom3d(Brep inBRep, BrepFace inFace, double inLinearPrecision)
        {
            Shape shape = null;

            TrimmedSheetMaker sheetMaker = new TrimmedSheetMaker(SX.Version.Current);
            sheetMaker.LinearTolerance = inLinearPrecision;
            sheetMaker.UsesBRepMethod = false;

            TX.Items.ItemMonikerKey key = new TX.Items.ItemMonikerKey(TX.Items.ItemOperationKey.BasicKey);

            // Get surface and set to maker.
            Rhino.Geometry.Surface surface = inBRep.Surfaces[inFace.FaceIndex];

            // Reverse surface and curves in 3d mode(according to the drilled cylinder crossed by cube in v5_example.3dm).
            //if (inFace.rev)
            //    surface = ImporterHelper.MakeReversed(surface); // Useless.

            // Closed BSpline surfaces must not be periodic for parasolid with 3d curves (according to wishbone.3dm and dinnermug.3dm).
            // If new problems come, see about the periodicity of the curves.

            //TODO check if planar to simplify
            TKGD3.Surfaces.Surface topSolidSurface = surface.ToHost();

            if (topSolidSurface != null && topSolidSurface is BSplineSurface bsplineSurface && (topSolidSurface.IsUPeriodic || topSolidSurface.IsVPeriodic))
            {
                topSolidSurface = (BSplineSurface)bsplineSurface.Clone();

                if (topSolidSurface.IsUPeriodic)
                    bsplineSurface.MakeUNonPeriodic();

                if (topSolidSurface.IsVPeriodic)
                    bsplineSurface.MakeVNonPeriodic();

                sheetMaker.Surface = new OrientedSurface(bsplineSurface, false);
            }

            else
            {
                sheetMaker.Surface = new OrientedSurface(topSolidSurface, false);
            }

            sheetMaker.SurfaceMoniker = new ItemMoniker(false, (byte)ItemType.ShapeFace, key, 1);

            // Get spatial curves and set to maker.
            TopSolid.Kernel.SX.Collections.Generic.List<TKGD3.Curves.CurveList> loops3d = new TSXGen.List<TKGD3.Curves.CurveList>();
            TopSolid.Kernel.SX.Collections.Generic.List<ItemMonikerList> listItemMok = new TSXGen.List<ItemMonikerList>();
            int i = 0;
            int loopIndex = 0;
            foreach (Rhino.Geometry.BrepLoop loop in inFace.Loops)
            {
                loops3d.Add(new TKGD3.Curves.CurveList());
                listItemMok.Add(new ItemMonikerList());
                foreach (var trim in loop.Trims)
                {
                    if (loops3d.Count < loopIndex - 1 || listItemMok.Count < loopIndex - 1) break;
                    loops3d[loopIndex].Add(trim.Edge.EdgeCurve.ToHost());
                    listItemMok[loopIndex].Add(new ItemMoniker(false, (byte)ItemType.SketchSegment, key, i++));
                }
                loopIndex++;
            }

            if (loops3d != null && loops3d.Count != 0)
            {
                // if (inFace.rev == false || ImporterHelper.MakeReversed(loops3d)) // Useless
                {
                    sheetMaker.SetCurves(loops3d, listItemMok);

                    bool valid = sheetMaker.IsValid;

                    try
                    {
                        shape = sheetMaker.Make(null, ItemOperationKey.BasicKey);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + "\n" + e.Data.ToString());
                    }
                }
            }

            return shape;
        }

        private static Shape MakeSheetFrom2d(Brep inBRep, BrepFace inFace, double inLinearPrecision)
        {
            Shape shape = null;

            TrimmedSheetMaker sheetMaker = new TrimmedSheetMaker(SX.Version.Current);
            sheetMaker.LinearTolerance = inLinearPrecision;
            Rhino.Geometry.Surface surface = inFace.DuplicateSurface();

            // Closed BSpline surfaces must be made periodic for parasolid with 2d curves (according to torus and sphere in v5_example.3dm).
            // If new problems come, see about the periodicity of the curves.
            TKGD3.Surfaces.Surface topSolidSurface = Convert.ToHost(surface);
            if (topSolidSurface != null && topSolidSurface is BSplineSurface bsplineSurface && ((bsplineSurface.IsUClosed && bsplineSurface.IsUPeriodic == false) || (bsplineSurface.IsVClosed && bsplineSurface.IsVPeriodic == false)))
            {
                bsplineSurface = (BSplineSurface)bsplineSurface.Clone();

                if (bsplineSurface.IsUClosed)
                    bsplineSurface.MakeUPeriodic();

                if (bsplineSurface.IsVClosed)
                    bsplineSurface.MakeVPeriodic();

                //surface = bsSurface;

                OrientedSurface osurf = new OrientedSurface(bsplineSurface, inFace.OrientationIsReversed);
                sheetMaker.Surface = osurf;
            }
            else
            {
                OrientedSurface osurf = new OrientedSurface(topSolidSurface, inFace.OrientationIsReversed);
                sheetMaker.Surface = osurf;
            }

            TopSolid.Kernel.SX.Collections.Generic.List<TKGD2.Curves.CurveList> loops2d = new TSXGen.List<TKGD2.Curves.CurveList>();
            TopSolid.Kernel.SX.Collections.Generic.List<ItemMonikerList> listItemMok = new TSXGen.List<ItemMonikerList>();
            ItemMonikerKey key = new ItemMonikerKey(ItemOperationKey.BasicKey);
            int i = 0;
            int loopIndex = 0;
            foreach (Rhino.Geometry.BrepLoop loop in inFace.Loops)
            {
                loops2d.Add(new TKGD2.Curves.CurveList());
                listItemMok.Add(new ItemMonikerList());
                foreach (var trim in loop.Trims)
                {
                    if (loops2d.Count < loopIndex - 1 || listItemMok.Count < loopIndex - 1) break;
                    loops2d[loopIndex].Add(trim.TrimCurve.ToHost2d());
                    listItemMok[loopIndex].Add(new ItemMoniker(false, (byte)ItemType.SketchSegment, key, i++));
                }
                loopIndex++;
            }


            //var entity = new TopSolid.Kernel.DB.D2.Sketches.PositionedSketchEntity(TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument, 0);
            //TopSolid.Kernel.G.D2.Sketches.Sketch sk2d = new TKGD2.Sketches.Sketch(entity, null, false);

            sheetMaker.SetCurves(loops2d, listItemMok);
            try
            {
                shape = sheetMaker.Make(null, ItemOperationKey.BasicKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return shape;
        }

        private static Shape MakeSheet(Brep inBRep, BrepFace inFace)
        {
            Shape shape = null;
            SheetMaker sheetMaker = new SheetMaker(SX.Version.Current);
            sheetMaker.Surface = Convert.ToHost(inFace.ToNurbsSurface());

            try
            {
                shape = sheetMaker.Make(null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return shape;
        }

        public static Shape SewShapes(this ShapeList shapeList)
        {
            Shape modifiedShape = shapeList.First();
            SheetsSewer sheetsSewer = new SheetsSewer(SX.Version.Current, modifiedShape);

            for (int i = 1; i < shapeList.Count; i++)
            {
                sheetsSewer.AddTool(shapeList.ElementAt(i), i);
            }

            sheetsSewer.GapWidth = G.Precision.ModelingLinearTolerance;
            sheetsSewer.NbIterations = 8;
            sheetsSewer.CreateNewBodies = true;
            sheetsSewer.ResetEdgesPrecision = true;
            sheetsSewer.Merges = true;

            try
            {
                sheetsSewer.Sew(ItemOperationKey.BasicKey);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return modifiedShape;
        }



        #endregion
    }
}
