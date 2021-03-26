//using Sample_2;
using System;

namespace EPFL.Rhino.Inside.UI.Test
{

    public partial class TestCommandGH : TopSolid.Kernel.UI.Commands.MenuCommand
    {
        // Constructors:

        /// <summary>
        /// Command constructor
        /// </summary>
        public TestCommandGH()
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
            //Must include simple_def.gh to project
            //System.IO.FileNotFoundException => Grasshopper.dll

            //using (var core = new Rhino.Runtime.InProcess.RhinoCore())
            //{
            //    RunHelper();
            //}


        }

        //    static void RunHelper()
        //    {
        //        // Extract definition to sample location as executable
        //        var assembly = typeof(Application).Assembly;
        //        string dir = System.IO.Path.GetDirectoryName(assembly.Location);
        //        string filePath = System.IO.Path.Combine(dir, "simple_def.gh");

        //        using (var resStream = assembly.GetManifestResourceStream("RunGrasshopper.simple_def.gh"))
        //        using (var outStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
        //        {
        //            resStream.CopyTo(outStream);
        //        }

        //        // Start grasshopper in "headless" mode
        //        var pluginObject = Rhino.RhinoApp.GetPlugInObject("Grasshopper") as Grasshopper.Plugin.GH_RhinoScriptInterface;
        //        pluginObject.RunHeadless();

        //        var io = new Grasshopper.Kernel.GH_DocumentIO();
        //        if (!io.Open(filePath))
        //            Console.WriteLine("File loading failed.");
        //        else
        //        {
        //            var doc = io.Document;

        //            // Documents are typically only enabled when they are loaded
        //            // into the Grasshopper canvas. In this case we -may- want to
        //            // make sure our document is enabled before using it.
        //            doc.Enabled = true;

        //            foreach (var obj in doc.Objects)
        //                if (obj is Grasshopper.Kernel.IGH_Param param)
        //                    if (param.NickName == "CollectMe")
        //                    {
        //                        param.CollectData();
        //                        param.ComputeData();

        //                        foreach (var item in param.VolatileData.AllData(true))
        //                            if (item.CastTo(out Line line))
        //                                Console.WriteLine($"Got a line: {line:0.000}");
        //                            else
        //                                Console.WriteLine($"Unexpected data of type: {item.TypeName}");

        //                        break;
        //                    }
        //        }

        //        Console.WriteLine("Done... press any key to exit");
        //        Console.ReadKey();
        //    }
        //}

    }
}
