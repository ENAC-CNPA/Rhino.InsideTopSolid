﻿using Rhino.Geometry;
using Rhino.Geometry.Collections;
using Rhino.Runtime.InProcess;
//using Sample_2;
using System;
using TK = TopSolid.Kernel;
using TopSolid.Kernel.WX;

namespace EPFL.RhinoInsideTopSolid.UI.Test
{

    public partial class TestCommandUI : TopSolid.Kernel.UI.Commands.MenuCommand
    {
        // Constructors:

        /// <summary>
        /// Command constructor
        /// </summary>
        public TestCommandUI()
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
        private static Form1 newMyWindow;

        protected override void Invoke()
        {
            RhinoInside.Resolver.Initialize();
            //System.IO:FileNotFoundException : Impossible de charger le fichier ou l'assembly 'RhinoWindows, ...'
            newMyWindow = new Form1();
            
        }       


    }
}
