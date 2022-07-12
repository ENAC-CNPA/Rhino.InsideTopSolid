using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.DB.Families.Documents;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Pdm;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_PDM
{
    public class TSFamilyDrivers : GH_Component, IGH_VariableParameterComponent
    {
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.FamilyDocument, 24, 24).ToBitmap();

        /// <summary>
        /// Initializes a new instance of the TSFamilyDrivers class.
        /// </summary>
        public TSFamilyDrivers()
          : base("TSFamilyDrivers", "Drivers",
              "Gets the Family Drivers",
              "TopSolid", "TopSolid PDM")
        {
        }

        FamilyDocument familyDocument = null;

        protected override void BeforeSolveInstance()
        {

            IGH_Param ghParam;
            while (Params.Input.Count != 1)
            {
                ghParam = Params.Input[1];
                Params.UnregisterOutputParameter(ghParam);
            }

            Console.WriteLine("No iter has run");
            var x = Params.Input[0].VolatileData;
            var tree = x as GH_Structure<IGH_Goo>;
            List<string> variablelistofNames = GetOutputList(tree);
            foreach (var docName in variablelistofNames)
            {
                var newParam = CreateParameter(GH_ParameterSide.Input, Params.Input.Count) as Param_GenericObject;
                newParam.Name = docName;
                newParam.NickName = docName;
                newParam.Description = $" {docName} Entities";
                newParam.MutableNickName = false;
                newParam.Access = GH_ParamAccess.item;

                newParam.Optional = false;
                Params.RegisterInputParam(newParam);
            }
            base.BeforeSolveInstance();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("TSFamily", "Family", "TopSolid Family or Family Name", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FamilyDrivers", "Drivers", "Contains the corresponding set of drivers", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Setting target document from input, or else take current document by default
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();
            familyDocument = null;
            IDocument res = null;
            if (DA.GetData("TSFamily", ref wrapper))
            {
                if (wrapper.Value is string || wrapper.Value is GH_String)
                {
                    res = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault();
                    familyDocument = res as FamilyDocument;
                }
                else if (wrapper.Value is IDocumentItem)
                    familyDocument = (wrapper.Value as IFamilyDocumentItem).OpenLastValidMinorRevisionDocument() as FamilyDocument;
                else if (wrapper.Value is IDocument)
                    familyDocument = wrapper.Value as FamilyDocument;
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
            var listofDriversNames = new List<string>();

            foreach (var ghGoo in tsProj.AllData(true))
            {

                GH_ObjectWrapper ghObj = new GH_ObjectWrapper();
                ghObj = (GH_ObjectWrapper)ghGoo;
                familyDocument = ((IFamilyDocumentItem)ghObj.Value).OpenLastValidMinorRevisionDocument() as FamilyDocument;

                if (familyDocument != null)
                {
                    foreach (var item in familyDocument.DriversFolderEntity.Constituents)
                    {
                        if (familyDocument.OptionalDriversFolderEntity.Equals(item)) continue;
                        listofDriversNames.Add(item.EditingName);
                    }
                }

            }
            return listofDriversNames;
        }



        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C7D9C63B-5893-4523-A5C4-F90047850112"); }
        }
    }
}