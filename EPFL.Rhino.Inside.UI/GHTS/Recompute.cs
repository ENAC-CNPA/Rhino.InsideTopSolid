using Grasshopper.Kernel;
using Grasshopper;
using Rhino;
using Rhino.Runtime.InProcess;
using System;
using System.Reflection;
using TK = TopSolid.Kernel;
using Microsoft.Win32.SafeHandles;

namespace EPFL.RhinoInsideTopSolid.UI.GHTS
{
    public partial class Recompute : TopSolid.Kernel.UI.Commands.MenuCommand
    {
        // Constructors:

        /// <summary>
        /// Command constructor
        /// </summary>
        public Recompute()
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
                if (!base.CanInvoke || Rhinoceros.rhinoCore == null)
                    return false;
                if (Instances.ActiveCanvas?.Document is null)
                    return false;

                return true;
            }
        }



        // Methods:
        protected override void Invoke()
        {
            if (Instances.ActiveCanvas?.Document is GH_Document definition)
            {
                if (GH_Document.EnableSolutions) definition.NewSolution(true);
                else
                {
                    GH_Document.EnableSolutions = true;
                    try { definition.NewSolution(false); }
                    finally { GH_Document.EnableSolutions = false; }
                }

                // If there are no scheduled solutions return control back to Revit now
                if (definition.ScheduleDelay > GH_Document.ScheduleRecursive)
                    WindowHandle.ActiveWindow = Rhinoceros.MainWindow;

            }
        }



    }
}
