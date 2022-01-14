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
                return base.CanInvoke;
            }
        }



        // Methods:

        /// <summary>
        /// Method call when the command button is pressed
        /// </summary>
        protected override void Invoke()
        {
            //Invoke the base command...
            base.Invoke();

            //DO SOMETHING..
        }

    }
}
