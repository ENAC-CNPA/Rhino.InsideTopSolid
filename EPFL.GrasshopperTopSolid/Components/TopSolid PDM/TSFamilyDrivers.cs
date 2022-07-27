using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB.Configurations;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Cad.Design.DB.Inclusion;
using TopSolid.Kernel.DB.D3.Points;
using TopSolid.Kernel.DB.Documents;
using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.DB.Families;
using TopSolid.Kernel.DB.Families.Documents;
using TopSolid.Kernel.DB.Families.Drivers;
using TopSolid.Kernel.DB.Families.Parameters;
using TopSolid.Kernel.DB.Parameters;
using TopSolid.Kernel.DB.SmartObjects;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Families;
using TopSolid.Kernel.TX.Pdm;
using TopSolid.Kernel.TX.Properties;
using TopSolid.Kernel.TX.Undo;

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
            while (Params.Input.Count > 2)
            {
                if (!Params.Input[0].VolatileData.IsEmpty) break;
                ghParam = Params.Input[2];
                //if (ghParam.VolatileData != null) continue;
                Params.UnregisterInputParameter(ghParam);
            }

            Console.WriteLine("No iter has run");
            var x = Params.Input[0].VolatileData;
            var tree = x as GH_Structure<IGH_Goo>;
            List<string> variablelistofNames = GetOutputList(tree);
            foreach (var docName in variablelistofNames)
            {
                if (Params.Input.Where(y => y.Name == docName).ToList().Count != 0) continue;
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
            pManager.AddGenericParameter("TSFamily", "Family", "TopSolid Family Document or Family Name", GH_ParamAccess.item);
            pManager.AddGenericParameter("TSAssembly", "Assembly", "TopSolid Assembly document or Assembly Name", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("instance", "Drivers", "Family Instance Created", GH_ParamAccess.item);
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
            AssemblyDocument assemblyDocument = null;
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

            if (DA.GetData("TSAssembly", ref wrapper))
            {
                if (wrapper.Value is string || wrapper.Value is GH_String)
                {
                    res = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault();
                    assemblyDocument = res as AssemblyDocument;
                }
                else if (wrapper.Value is IDocumentItem)
                    assemblyDocument = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as AssemblyDocument;
                else if (wrapper.Value is IDocument)
                    assemblyDocument = wrapper.Value as AssemblyDocument;
            }


            if (familyDocument == null || assemblyDocument == null)
                return;

            LogicalBasicSmartFamily logicalBasicSmartFamily = new LogicalBasicSmartFamily(null, familyDocument);
            SmartFamily smartFamily = logicalBasicSmartFamily.Clone(null) as SmartFamily;
            UndoSequence.UndoCurrent();
            UndoSequence.Start("fam", true);

            InstanceDriverValueList driverValues = new InstanceDriverValueList(null);

            foreach (var driver in familyDocument.DriversFolderEntity.DriverDefinitionEntities)
            {
                GH_ObjectWrapper inputValue = null;

                DA.GetData(driver.EditingName, ref inputValue);
                InstanceDriverType driverType = driver.DrivenEntity.DriverType;
                SmartObject val = null;
                if (driver.IsGeometricDriver)
                {

                    switch (driverType)
                    {
                        case InstanceDriverType.Point:
                            val = new BasicSmartPoint(null, (inputValue.Value as PointEntity).Geometry);
                            if (val == null)
                            {

                            }
                            break;

                    }
                }
                else
                {
                    switch (driverType)
                    {
                        case InstanceDriverType.Text:
                            val = new BasicSmartText(null, (inputValue.Value as GH_String).ToString());
                            break;
                        case InstanceDriverType.Real:
                            val = inputValue.Value as SmartReal;
                            break;
                        case InstanceDriverType.Integer:
                            val = new BasicSmartInteger(null, (int)((GH_Number)inputValue.Value).Value);
                            break;
                    }

                }


                BasisInstanceDriverValue basisdriver = new BasisInstanceDriverValue(null, driver);
                basisdriver.DriverValue = val;
                driverValues.Add(basisdriver);
            }

            Document instance = null;
            bool minorCreated = false;
            InstanceMaker instanceMaker = new InstanceMaker(null, smartFamily);
            instanceMaker.SmartCode = null;
            instanceMaker.SubstitutionRules = null;
            instanceMaker.DriverValues = driverValues;
            instanceMaker.IsSignatureCheckStrict = true;



            instance = instanceMaker.MakeInstanceDocument(false, out minorCreated);

            assemblyDocument.EnsureIsDirty();

            InclusionOperation inclusionOperation = new InclusionOperation(assemblyDocument, 0, instance as DesignDocument, instanceMaker, (instance as DesignDocument).CurrentRepresentationEntity, false, new ConfigurationEntity(assemblyDocument, 0), null);

            inclusionOperation.Create();


            DA.SetData(0, instance);

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