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
using TopSolid.Kernel.DB.Families.Drivers.Substitutions.Rules;
using TopSolid.Kernel.DB.Families.Parameters;
using TopSolid.Kernel.DB.Parameters;
using TopSolid.Kernel.DB.SmartObjects;
using TopSolid.Kernel.G;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Families;
using TopSolid.Kernel.TX.Items;
using TopSolid.Kernel.TX.Pdm;
using TopSolid.Kernel.TX.Properties;
using TopSolid.Kernel.TX.Undo;
using TopSolid.Kernel.TX.Units;

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
            if (RunCount != -1) return;
            IGH_Param ghParam;
            while (Params.Input.Count > 3)
            {
                if (!Params.Input[0].VolatileData.IsEmpty) break;
                ghParam = Params.Input[3];
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
            pManager.AddBooleanParameter("Run", "Run", "Run Family inclusion and occurence creation", GH_ParamAccess.item);
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
            bool run = false;
            if (!DA.GetData("Run", ref run)) return;
            if (!run) return;

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

            foreach (var driver in smartFamily.FamilyDocument.DriversFolderEntity.DriverDefinitionEntities)
            {
                GH_ObjectWrapper inputValue = null;

                DA.GetData(driver.EditingName, ref inputValue);
                InstanceDriverType driverType = (InstanceDriverType)driver.DrivenEntity.DriverType;

                SmartObject val = null;
                if (driver.IsGeometricDriver)
                {
                    switch (driverType)
                    {
                        case InstanceDriverType.Point:
                            if (inputValue.Value is IGeometry geometry)
                            {
                                //val = new BasicSmartPoint(null, (TopSolid.Kernel.G.D3.Point)geometry);
                                val = ProvidedSmartPoint.Make(geometry.Owner as Element, ItemLabel.Empty); //TODO Verify
                                //driver.SetGeometry(geometry, false);
                            }
                            else if (inputValue.Value is PointEntity pointEntity)
                            {
                                //val = new BasicSmartPoint(null, pointEntity.Geometry);
                                //driver.SetGeometry(pointEntity.Geometry, false);
                                val = ProvidedSmartPoint.Make(pointEntity, ItemLabel.Empty);
                            }
                            else if (inputValue.Value is SmartPoint smartpoint)
                            {
                                val = smartpoint;
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
                            var real = (GH_Number)inputValue.Value;
                            val = new BasicSmartReal(null, new Real((double)real.Value, new UnitFormat(UnitType.Length)));
                            break;
                        case InstanceDriverType.Integer:
                            val = new BasicSmartInteger(null, (int)((GH_Number)inputValue.Value).Value);
                            break;
                            //case InstanceDriverType.Point:
                            //    val = new BasicSmartPoint(null, (inputValue.Value as PointEntity)?.Geometry);
                            //    if (val is null)
                            //        val = new BasicSmartPoint(null, (IGeometry)inputValue.Value);
                            //    break;
                    }

                }


                BasisInstanceDriverValue basisdriver = new BasisInstanceDriverValue(null, driver);
                basisdriver.DriverValue = val;

                var manages = basisdriver.ManagesPositioning;
                //var drivenval = driver.DrivenEntity.DriverValue;
                //driver.DrivenEntity.SetDriverValue(val);
                driverValues.Add(basisdriver);
            }

            Document instance = null;
            bool minorCreated = false;
            InstanceMaker instanceMaker = new InstanceMaker(null, smartFamily);
            instanceMaker.SmartCode = null;
            instanceMaker.SubstitutionRules = null;


            instanceMaker.DriverValues = driverValues;


            instanceMaker.IsSignatureCheckStrict = true;
            instanceMaker.ManagesPositioning = true;

            //var driveposition = instanceMaker.ContainsDriversManagingPositioning;



            instance = instanceMaker.MakePublicOrPrivateInstanceDocument(false, assemblyDocument, out minorCreated);

            assemblyDocument.EnsureIsDirty();

            InclusionOperation inclusionOperation = new InclusionOperation(assemblyDocument, 0, instance as DesignDocument, instanceMaker, (instance as DesignDocument).CurrentRepresentationEntity, false, null, null);
            //assemblyDocument.PositionInclusion(inclusionOperation, true, false, TopSolid.Cad.Design.DB.Constraints.FixedAddingMode.FirstInclusion, -1, null);
            assemblyDocument.PositionInclusion(
                inclusionOperation, true, NewInclusionPositioningMode.NewPositioning, null,
                TopSolid.Cad.Design.DB.Constraints.FixedAddingMode.FirstInclusion, -1, null);
            if (inclusionOperation.IsInvalid)
                inclusionOperation.TryRepairInvalid();


            DA.SetData(0, instance);
            UndoSequence.End();
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

                //TODO cause not working correctly with string input
                if (ghGoo is GH_String ghString)
                {
                    var docs = PdmClientStore.CurrentPdmClient.GetAllProjects().Select(x => x.DocumentItems).SelectMany(y => y).Where(d => d is IFamilyDocumentItem);
                    foreach (var doc in docs)
                    {
                        familyDocument = doc.OpenLastValidMinorRevisionDocument() as FamilyDocument;
                        if (familyDocument != null)
                        {
                            if (familyDocument.Name.ToString() == ghString.ToString() || ghString.ToString() == familyDocument.LocalizedName) break;
                            else familyDocument = null;
                        }

                    }

                }


                else if (ghGoo is IDocument iDoc)
                {
                    familyDocument = iDoc as FamilyDocument;
                }

                else if (ghGoo is GH_String text)
                {
                    var docs = PdmClientStore.CurrentPdmClient.GetAllProjects().Select(x => x.DocumentItems).SelectMany(y => y).Where(d => d is IFamilyDocumentItem);
                    foreach (var doc in docs)
                    {
                        familyDocument = doc.OpenLastValidMinorRevisionDocument() as FamilyDocument;
                        if (familyDocument != null)
                        {
                            if (familyDocument.Name.ToString() == text.ToString() || text.ToString() == familyDocument.LocalizedName) break;
                            else familyDocument = null;
                        }

                    }
                }

                else if (ghGoo is GH_ObjectWrapper wrapper)
                {
                    if (wrapper.Value is GH_String text2)
                    {
                        var docs = PdmClientStore.CurrentPdmClient.GetAllProjects().Select(x => x.DocumentItems).SelectMany(y => y).Where(d => d is IFamilyDocumentItem);
                        foreach (var doc in docs)
                        {
                            familyDocument = doc.OpenLastValidMinorRevisionDocument() as FamilyDocument;
                            if (familyDocument != null)
                            {
                                if (familyDocument.Name.ToString() == text2.ToString() || text2.ToString() == familyDocument.LocalizedName) break;
                                else familyDocument = null;
                            }

                        }
                    }
                    else
                        familyDocument = ((IFamilyDocumentItem)wrapper.Value).OpenLastValidMinorRevisionDocument() as FamilyDocument;
                }

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