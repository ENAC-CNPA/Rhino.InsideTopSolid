using Rhino.Geometry;
//using Sample_2;
using System;
using TK = TopSolid.Kernel;

namespace EPFL.RhinoInsideTopSolid.UI.Test
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



            //using (new Rhino.Runtime.InProcess.RhinoCore()) //System.IO.FileLoadException: 'Impossible de charger le fichier ou l'assembly 'Microsoft.WindowsAPICodePack.Shell, 
            //{
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
        }
        
        //static private Rhino.Runtime.InProcess.RhinoCore m_rhino_core;

        public static Rhino.Geometry.Mesh GetMesh()
        {
            Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
            return mesh;
        }
    }
}
