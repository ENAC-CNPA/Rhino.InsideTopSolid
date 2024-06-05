using Grasshopper.Kernel;
using Grasshopper;
using Rhino;
using Rhino.Runtime.InProcess;
using System;
using System.Reflection;
using TK = TopSolid.Kernel;

namespace EPFL.RhinoInsideTopSolid.UI.GHTS
{
    public partial class LockSolver : TopSolid.Kernel.UI.Commands.MenuCommand
    {
        // Constructors:

        /// <summary>
        /// Command constructor
        /// </summary>
        public LockSolver()
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
                return true;
            }
        }



        // Methods:
        protected override void Invoke()
        {
            GH_Document.EnableSolutions = !GH_Document.EnableSolutions;

            if (GH_Document.EnableSolutions)
            {
                if (Instances.ActiveCanvas?.Document is GH_Document definition)
                    definition.NewSolution(false);
            }
            else
            {
                //Revit.RefreshActiveView();
            }
        }



    }
}
