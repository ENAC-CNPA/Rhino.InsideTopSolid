//using Sample_2;
using Rhino.Runtime.InProcess;
using System;
using System.IO;
using System.Reflection;

namespace EPFL.RhinoInsideTopSolid.UI.GHTS
{

    public partial class GrasshopperCommand : TopSolid.Kernel.UI.Commands.MenuCommand
    {
        // Constructors:

        /// <summary>
        /// Command constructor
        /// </summary>
        public GrasshopperCommand()
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
        static RhinoCore rhinoCore;
        private static bool _grasshopperLoaded = false;
        public static Grasshopper.Plugin.GH_RhinoScriptInterface Script { get; private set; }

        protected override void Invoke()
        {
            //Method that works !!
            if (rhinoCore == null)
                rhinoCore = new Rhino.Runtime.InProcess.RhinoCore(new string[] { "/NOSPLASH" }, WindowStyle.Normal);

            if (!LoadGrasshopperComponents())
            {
                TopSolid.Kernel.SX.UI.Reporting.ReportInformation($"\nFailed to start Rhino");
                return;
            }

            if (Script.IsEditorVisible())
                Script.HideEditor();
            else
            {
                Script.ShowEditor();
            }
        }

        internal static bool Shutdown()
        {
            if (rhinoCore is object)
            {
                try
                {
                    rhinoCore.Dispose();
                    rhinoCore = null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool LoadGrasshopperComponents()
        {
            if (_grasshopperLoaded)
                return true;

            var LoadGHAProc = Grasshopper.Instances.ComponentServer.GetType().GetMethod("LoadGHA", BindingFlags.NonPublic | BindingFlags.Instance);
            if (LoadGHAProc == null)
                return false;

            var bCoff = Grasshopper.Instances.Settings.GetValue("Assemblies:COFF", true);
            Grasshopper.Instances.Settings.SetValue("Assemblies:COFF", false);
            var location = Assembly.GetExecutingAssembly().Location;
            location = Path.Combine(Path.GetDirectoryName(location) + "\\EPFL.GrasshopperTopSolid.gha");

            var rc = (bool)LoadGHAProc.Invoke
            (
              Grasshopper.Instances.ComponentServer,
              new object[] { new Grasshopper.Kernel.GH_ExternalFile(location), false }
            );

            Grasshopper.Instances.Settings.SetValue("Assemblies:COFF", bCoff);

            if (rc)
                Grasshopper.Kernel.GH_ComponentServer.UpdateRibbonUI();

            var GrasshopperGuid = new Guid(0xB45A29B1, 0x4343, 0x4035, 0x98, 0x9E, 0x04, 0x4E, 0x85, 0x80, 0xD9, 0xCF);
            rc = Rhino.PlugIns.PlugIn.LoadPlugIn(GrasshopperGuid);

            Script = new Grasshopper.Plugin.GH_RhinoScriptInterface();
            Script.LoadEditor();
            rc = Script.IsEditorLoaded();

            _grasshopperLoaded = true;
            return rc;
        }

    }
}
