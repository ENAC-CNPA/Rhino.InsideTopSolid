using Rhino;
using Rhino.Runtime.InProcess;
using System;
using System.Reflection;
using TK = TopSolid.Kernel;

namespace EPFL.RhinoInsideTopSolid.UI.GHTS
{
    public partial class RhinoCommand : TopSolid.Kernel.UI.Commands.MenuCommand
    {
        // Constructors:

        /// <summary>
        /// Command constructor
        /// </summary>
        public RhinoCommand()
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
                //return base.CanInvoke;

                if (!base.CanInvoke)
                    return false;
                return false;
            }
        }



        // Methods:

        /// <summary>
        /// Method call when the command button is pressed
        /// </summary>


        /// <summary>
        /// Method call when the command button is pressed
        /// </summary>
        /// 
        static RhinoCore rhinoCore;
        //private static bool _grasshopperLoaded = false;
        public static Grasshopper.Plugin.GH_RhinoScriptInterface Script { get; private set; }

        protected override void Invoke()
        {
            //Method that works !!
            if (rhinoCore == null)
                rhinoCore = new Rhino.Runtime.InProcess.RhinoCore(new string[] { "/NOSPLASH" }, WindowStyle.Normal);
            else
            {
                Rhino.UI.RhinoEtoApp.MainWindow.Visible = true;
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

    }
}
