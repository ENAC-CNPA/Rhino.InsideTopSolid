﻿using Rhino;
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
                if (!base.CanInvoke)
                    return false;
                return true;
            }
        }



        // Methods:
        protected override void Invoke()
        {
            Rhinoceros.RhinoStartup();
            Rhinoceros.Show();
        }



    }
}
