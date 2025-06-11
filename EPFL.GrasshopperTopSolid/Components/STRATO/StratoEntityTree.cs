using System;
using System.Collections.Generic;
using System.Linq;
using Cirtes.Strato.Cad.DB.Documents;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Pdm;

namespace EPFL.GrasshopperTopSolid.Components.STRATO
{
    public class StratoEntityTree : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the StratoEntityTree class.
        /// </summary>
        public StratoEntityTree()
          : base("StratoEntityTree", "Strato",
              "Description",
              "TopSolid", "Strato")
        {
        }
        protected override void BeforeSolveInstance()
        {
            IGH_Param ghParam;
            while (Params.Output.Count != 0)
            {
                ghParam = Params.Output[0];
                Params.UnregisterOutputParameter(ghParam);
            }

            if (Params.Output.Count == 0)
            {
                Params.Output.Clear();
            }

            Console.WriteLine("No iter has run");
            var x = Params.Input[0].VolatileData;

            var tree = x as GH_Structure<IGH_Goo>;

            if (tree != null)
            {
                List<string> variablelistofNames = GetOutputList(tree);
                foreach (var docName in variablelistofNames)
                {
                    var newParam = CreateParameter(GH_ParameterSide.Output, Params.Output.Count) as Param_GenericObject;
                    newParam.Name = docName;
                    newParam.NickName = docName;
                    newParam.Description = $" {docName} Entities";
                    newParam.MutableNickName = false;
                    newParam.Access = GH_ParamAccess.list;
                    //newParam.Detachable = isDetached;
                    newParam.Optional = false;
                    Params.RegisterOutputParam(newParam);
                }
            }

            //}
            base.BeforeSolveInstance();
            //fixedlistofNames = variablelistofNames;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Name", "N", "gets a Strato Document Tree", GH_ParamAccess.item);
        }
        SlicePartsDocument document;
        GH_ObjectWrapper wrapper = new GH_ObjectWrapper();

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Entities", "E", "Documents entities", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
            DA.GetData(0, ref obj);
            SlicePartsDocument slicePartsDocument = obj.Value as SlicePartsDocument;

            if (slicePartsDocument is null && obj.Value is IDocumentItem docItem)
            {
                slicePartsDocument = docItem.OpenLastValidMinorRevisionDocument() as SlicePartsDocument;
            }

            foreach (var tsObj in slicePartsDocument.RootEntity.Constituents.Where(x => !x.IsGhost))
            {
                DA.SetData(tsObj.EditingName, tsObj);
            }

        }
        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            var myParam = new Param_GenericObject
            {
                Name = GH_ComponentParamServer.InventUniqueNickname("ABCD", Params.Input),
                MutableNickName = true,
                Optional = true,
            };
            myParam.NickName = myParam.Name;

            return myParam;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {

        }

        private List<string> GetOutputList(GH_Structure<IGH_Goo> tsProj)
        {
            // Get the full list of output parameters
            var listofDocsNames = new List<string>();

            foreach (var ghGoo in tsProj.AllData(true))
            {

                GH_ObjectWrapper ghObj = new GH_ObjectWrapper();
                ghObj = (GH_ObjectWrapper)ghGoo;
                
                SlicePartsDocument pDoc = null;

                if (ghObj.Value is IDocumentItem docItem)
                {
                    
                    pDoc = docItem.OpenLastValidMinorRevisionDocument() as SlicePartsDocument;
                }
                else if (ghObj.Value is SlicePartsDocument sliceDoc)
                {

                    pDoc = sliceDoc;
                }           
               
                if (pDoc != null)
                {
                    foreach (var item in pDoc.RootEntity.Constituents.Where(x => !x.IsGhost))
                    {
                        listofDocsNames.Add(item.EditingName);
                    }
                }

            }
            return listofDocsNames;
        }

        private List<string> GetOutputList(GH_Structure<GH_String> tsProj)
        {
            // Get the full list of output parameters
            var listofDocsNames = new List<string>();

            foreach (var ghGoo in tsProj.AllData(true))
            {

                GH_ObjectWrapper ghObj = new GH_ObjectWrapper();
                ghObj = (GH_ObjectWrapper)ghGoo;
                IDocumentItem docitem = (IDocumentItem)ghObj.Value;
                TopSolid.Kernel.DB.Documents.Document pDoc = (TopSolid.Kernel.DB.Documents.Document)docitem.OpenLastValidMinorRevisionDocument();
                if (pDoc != null)
                {
                    foreach (var item in pDoc.RootEntity.Constituents.Where(x => !x.IsGhost))
                    {
                        listofDocsNames.Add(item.EditingName);
                    }
                }

            }
            return listofDocsNames;
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.Resources_ExpandAll, 24, 24).ToBitmap();


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8A20944F-57F0-4C1F-8548-A94EE913D741"); }
        }


    }
}