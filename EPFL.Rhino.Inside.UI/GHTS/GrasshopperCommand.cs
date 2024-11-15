//using Sample_2;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;


using Rhino.Commands;

using Rhino.DocObjects;

using Rhino.Input;
using Rhino.PlugIns;
using Rhino.Runtime.InProcess;
using static Rhino.RhinoMath;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using tsPrec = TopSolid.Kernel.G.Precision;
using TopSolid.Kernel.TX.Formulas.Scripting;
using TopSolid.Kernel.TX.Pdm.Globalization;


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



        private static bool _grasshopperLoaded = false;
        public static readonly Grasshopper.Plugin.GH_RhinoScriptInterface Script = new Grasshopper.Plugin.GH_RhinoScriptInterface();

        //static System.Globalization.CultureInfo cultureInfo = TopSolid.Kernel.SX.Globalization.CultureInfo.LocalizationCulture;


        protected override void Invoke()
        {
            Rhinoceros.RhinoStartup();

            if (Script.IsEditorVisible())
                Script.HideEditor();
            else
            {
                Script.ShowEditor();
            }

            //TODO uncomment the following to make a more general solution to adapt units
            //RhinoDoc.NewDocument += OnNewDocument;
            RhinoDoc doc = RhinoDoc.ActiveDoc;
            if (doc != null)
                doc.ModelUnitSystem = UnitSystem.Meters;
            System.Threading.Thread.CurrentThread.CurrentCulture = Rhinoceros.culture;
            TopSolid.Kernel.SX.UI.Application.IsMouseWheelInterceptedByGraphics = false;


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

            Script.LoadEditor();
            rc = Script.IsEditorLoaded();

            _grasshopperLoaded = true;
            return rc;
        }

        #region Document
        //static void OnNewDocument(object sender, DocumentEventArgs e)
        //{
        //    // If a new document is created without template it is updated from Revit.ActiveDBDocument
        //    Debug.Assert(string.IsNullOrEmpty(e.Document.TemplateFileUsed));

        //    if (e.Document is RhinoDoc)
        //    {
        //        UpdateDocumentUnits(e.Document);
        //        //UpdateDocumentUnits(e.Document, Revit.ActiveUIDocument?.Document);
        //    }
        //}


        //static void UpdateDocumentUnits(RhinoDoc rhinoDoc, ModelingDocument topSolidDoc = null)
        //{
        //    bool docModified = rhinoDoc.Modified;
        //    try
        //    {


        //        if (topSolidDoc is null)
        //        {
        //            rhinoDoc.ModelUnitSystem = UnitSystem.Meters;
        //            rhinoDoc.ModelAbsoluteTolerance = tsPrec.LinearPrecision;
        //            rhinoDoc.ModelAngleToleranceRadians = tsPrec.AngularPrecision;
        //        }
        //        else if (rhinoDoc.ModelUnitSystem == UnitSystem.None)
        //        {

        //            rhinoDoc.ModelAngleToleranceRadians = tsPrec.AngularPrecision;
        //            //rhinoDoc.ModelDistanceDisplayPrecision =                
        //            //rhinoDoc.ModelAbsoluteTolerance = UnitScale.Convert(revitTol.VertexTolerance, UnitScale.Internal, UnitScale.GetModelScale(rhinoDoc));



        //        }
        //    }
        //    finally
        //    {
        //        rhinoDoc.Modified = docModified;
        //    }
        //}




        #endregion


    }
}
