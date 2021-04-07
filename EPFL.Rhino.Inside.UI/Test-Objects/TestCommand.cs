using Rhino.Geometry;
//using Sample_2;
using System;
using System.Collections.Generic;
using TopSolid.Kernel.GR.Attributes;
using TopSolid.Kernel.GR.D3;
using TopSolid.Kernel.GR.Displays;
using TopSolid.Kernel.SX.Drawing;
using TK = TopSolid.Kernel;
using Rhino.Runtime.InProcess;

namespace EPFL.Rhino.Inside.UI.Test
{

    public partial class TestCommand : TopSolid.Kernel.UI.Commands.MenuCommand
    {
        // Constructors:

        /// <summary>
        /// Command constructor
        /// </summary>
        public TestCommand()
        {
            Console.WriteLine("Test Start 0");
        }


        // Properties:


        /// <summary>
        /// Return true to Enable the command button
        /// </summary>
        protected override bool CanInvoke
        {
            get
            {
                //Best practice:
                // return the combinaison to the base & your tests

                //In this case, just return the base CanInvoke
                return base.CanInvoke;
            }
        }

        // Methods:

        /// <summary>
        /// Method call when the command button is pressed
        /// </summary>
        /// 

        global::Rhino.Runtime.InProcess.RhinoCore rhinoCore;

        protected override void Invoke()
        {
            //Invoke the base command...
            base.Invoke();

            // JFB
            //var sphere = new Sphere(Point3d.Origin, 12);
            //var brep = sphere.ToBrep();
            //var mp = new MeshingParameters(0.5);
            //var mesh = Mesh.CreateFromBrep(brep, mp);
            //TK.SX.UI.Reporting.ReportInformation($"Mesh with {mesh[0].Vertices.Count} vertices created");

            // License problem with new Mesh()!
            //Rhino.Geometry.Mesh myMesh = GetMesh();
            //TK.SX.UI.Reporting.ReportInformation(myMesh.UserData.Count.ToString());


            // RWI           
            //Trying to instance RhinoCore at Command Launch
            //m_rhino_core = new Rhino.Runtime.InProcess.RhinoCore();

            //error -200
            //m_rhino_core = new Rhino.Runtime.InProcess.RhinoCore(new[] { "/nosplash" }, Rhino.Runtime.InProcess.WindowStyle.Hidden);

            //Nothing happens

            //RhinoInside.Resolver.Initialize();

            //m_rhino_core = new Rhino.Runtime.InProcess.RhinoCore();

            //System.IO.FileLoadException: 'Impossible de charger le fichier ou l'assembly 'Microsoft.WindowsAPICodePack.Shell,
            
            if (rhinoCore == null)
                rhinoCore = new global::Rhino.Runtime.InProcess.RhinoCore(new string[] { "/NOSPLASH" }, WindowStyle.Hidden);
            
                var pt1 = new Point3d(0, 0, 0);
                var pt2 = new Point3d(5, 5, 5);
                var vectZ = new Vector3d(0, 0, 1);
                var plane1 = new Plane(pt1, vectZ);
                var plane2 = new Plane(pt2, vectZ);
                var size = new Interval(0, 10);
                var box1 = new Box(plane1, size, size, size);
                var box2 = new Box(plane2, size, size, size);

                //Crashes at that point : Rhino.Runtime.NotLicensedException
                Brep brep1 = Brep.CreateFromBox(box1);
                Brep brep2 = Brep.CreateFromBox(box2);
                Brep[] diff = Brep.CreateBooleanDifference(brep1, brep2, 0.1);
                TK.SX.UI.Reporting.ReportInformation($"Mesh with {diff[0].Vertices.Count} vertices created");
                //}
                //Form1 formqlq = new Form1();
                //formqlq.ShowDialog();


                GeneralDisplay myGeneralDisplay = new GeneralDisplay(null);
                //Color = color.Green
                //LineStyle = Linestyle ...
                var listOfFaces = diff[0].Faces;
                List<Point3d> pointsRH = new List<Point3d>();
                List<TopSolid.Kernel.G.D3.Point> pointsTS = new List<TK.G.D3.Point>();
                Point3d[] rhinoPoints = diff[0].DuplicateVertices();
                foreach (Point3d rhinopoint in rhinoPoints)
                {
                    TopSolid.Kernel.G.D3.Point pointTSequiv = new TopSolid.Kernel.G.D3.Point(rhinopoint.X, rhinopoint.Y, rhinopoint.Z);
                    MarkerItem markerTSpoint = new MarkerItem(pointTSequiv);
                    markerTSpoint.Color = Color.Green;
                    markerTSpoint.MarkerStyle = MarkerStyle.ExtraLargeTriangle;
                    myGeneralDisplay.Add(markerTSpoint);
                }

                var rhinoedges = diff[0].Edges;
                foreach (BrepEdge edge in rhinoedges)
                {

                    Point3d end = edge.EndVertex.Location;
                    Point3d start = edge.StartVertex.Location;
                    TopSolid.Kernel.G.D3.Point endTS = new TK.G.D3.Point(end.X, end.Y, end.Z);
                    TopSolid.Kernel.G.D3.Point startTS = new TK.G.D3.Point(start.X, start.Y, start.Z);
                    LinearEdgeItem edgeTS = new LinearEdgeItem(startTS, endTS);
                    edgeTS.Color = Color.Green;
                    edgeTS.LineStyle = LineStyle.SolidMedium;
                    myGeneralDisplay.Add(edgeTS);


                }


                /*for (int i = 0; i < listOfFaces.Count; i++)
                {
                    var ListOfVerticesfori = listOfFaces[i].ToBrep().DuplicateVertices();

                    for (int j = 0; j < listOfFaces[i].ToBrep().Vertices.Count; j++)
                    {
                        pointsRH.Add(ListOfVerticesfori[j]);
                        pointsTS.Add(new TK.G.D3.Point(pointsRH[j].X, pointsRH[j].Y, pointsRH[j].Z));
                        MarkerItem markerTSpoint = new MarkerItem(pointsTS[j]);
                        markerTSpoint.Color = Color.Green;
                        markerTSpoint.MarkerStyle = MarkerStyle.ExtraLargeTriangle;
                        myGeneralDisplay.Add(markerTSpoint);
                    }


                }*/
                this.CurrentDocument.Display.AddDisplay(myGeneralDisplay);

                //lisOfFaces[0].ToBrep().Vertices[].;


                /*
                Point P1 = new Point(0, 0, 0), P2 = new Point(0, 3, 0), P3 = new Point(1.5, 1.5, 0);
                MarkerItem V1 = new MarkerItem(P1), V2 = new MarkerItem(P2), V3 = new MarkerItem(P3);
                LinearEdgeItem E12 = new LinearEdgeItem(P1, P2), E23 = new LinearEdgeItem(P2, P3), E13 = new LinearEdgeItem(P1, P3);

                FaceItemMaker faceItemMaker = new FaceItemMaker();
                faceItemMaker.Color = Color.Blue;
                FaceItem face = faceItemMaker.Make(new List<TopSolid.Kernel.G.S.D3.Point> { new TopSolid.Kernel.G.S.D3.Point(0f, 0f, 0f), new TopSolid.Kernel.G.S.D3.Point(0f, 3f, 0), new TopSolid.Kernel.G.S.D3.Point(1.5f, 1.5f, 0) }, ItemLabel.Empty, 0, TopSolid.Kernel.G.S.D3.UnitVector.VZ);

                myGeneralDisplay.Add(face);
                myGeneralDisplay.Add(V1);
                myGeneralDisplay.Add(V2);
                myGeneralDisplay.Add(V3);
                myGeneralDisplay.Add(E13);
                myGeneralDisplay.Add(E12);
                myGeneralDisplay.Add(E23);

                this.CurrentDocument.Display.AddDisplay(myGeneralDisplay);
                 */
                rhinoCore.Dispose();
                rhinoCore = null;
            
        }

        //static private Rhino.Runtime.InProcess.RhinoCore m_rhino_core;

        public static global::Rhino.Geometry.Mesh GetMesh()
        {
            global::Rhino.Geometry.Mesh mesh = new global::Rhino.Geometry.Mesh();
            return mesh;
        }


    }
}
