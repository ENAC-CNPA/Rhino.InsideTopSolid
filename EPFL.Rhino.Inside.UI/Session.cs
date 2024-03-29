﻿namespace EPFL.RhinoInsideTopSolid.UI
{

    public static class Session
    {
        static Session()
        {
            //Important to be in static constructor, I don't know why...
            Resolver.Initialize();          


            //Invariant culture issue...
            //if (!Rhinoceros.Startup() || !Rhinoceros.LoadGrasshopperComponents())
            //{
            //    //TopSolid.Kernel.SX.UI.Reporting.ReportInformation($"\nFailed to start Rhino WIP");
            //    return;
            //}
            //Rhinoceros.Startup();
            //Rhinoceros.LoadGrasshopperComponents();

        }


        //static private Rhino.Runtime.InProcess.RhinoCore m_rhino_core;

        /// <summary>
        /// Starts the session.
        /// </summary>
        public static void Start()
       {
            try
            {
                
                // Need to use RhinoCore one time to be able to use Rhino, even if there is nothing into m_rhino_core.
                //m_rhino_core = new Rhino.Runtime.InProcess.RhinoCore(); // Error loading Microsoft.WindowsAPICodePack.dll, but it works!
                //Maybe a conflict with an already loaded DLL needing CodePack...

                // As the Excel sample : Error -200
                // string SchemeName = $"Inside-TopSolid";
                // m_rhino_core = new Rhino.Runtime.InProcess.RhinoCore(new[] { $"/scheme={SchemeName}", "/nosplash" }, Rhino.Runtime.InProcess.WindowStyle.Hidden);
            }
            catch { }
        }
    }
}
