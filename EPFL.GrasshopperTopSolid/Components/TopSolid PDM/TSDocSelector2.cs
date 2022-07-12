using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Kernel.TX.Pdm;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_PDM
{
    public class TSDocSelector2 : GH_Component, IGH_VariableParameterComponent
    {


        /// <summary>
        /// Initializes a new instance of the TSDocSelector2 class.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.TSProjectDocument, 24, 24).ToBitmap();

        public TSDocSelector2()
          : base("TSDocSelector2", "DocSelector",
              "Gets the documents inside of a TopSolid Project",
              "TopSolid", "TopSolid PDM")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("TSProject", "proj", "Topsolid Project to get constituents", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        //List<string> fixedlistofNames = new List<string>();
        List<string> variablelistofNames = new List<string>();
        //IGH_Structure structure;

        protected override void BeforeSolveInstance()
        {
            IGH_Param ghParam;
            while (Params.Output.Count != 0)

            {
                ghParam = Params.Output[0];
                Params.UnregisterOutputParameter(ghParam);

            }

            Console.WriteLine("No iter has run");
            var x = Params.Input[0].VolatileData;
            var tree = x as GH_Structure<IGH_Goo>;

            variablelistofNames = GetOutputList(tree);

            foreach (var docName in variablelistofNames)
            {
                var newParam = CreateParameter(GH_ParameterSide.Output, Params.Output.Count) as Param_GenericObject;
                newParam.Name = docName;
                newParam.NickName = docName;
                newParam.Description = $"document {docName}";
                newParam.MutableNickName = false;
                newParam.Access = GH_ParamAccess.list;
                //newParam.Detachable = isDetached;
                newParam.Optional = false;
                Params.RegisterOutputParam(newParam);
            }

            //}
            base.BeforeSolveInstance();
            //fixedlistofNames = variablelistofNames;
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
            DA.GetData(0, ref obj);
            var projname = obj.Value.ToString();

            var objs = PdmClientStore.CurrentPdmClient.GetAllProjects().Where(x => x.GetName() == projname).First().Objects;
            foreach (var tsObj in objs)
            {
                DA.SetData($"{tsObj.GetType().Name} : {tsObj.GetName()}", tsObj);
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

        public bool DestroyParameter(GH_ParameterSide side, int index) => side == GH_ParameterSide.Output;

        protected override void AfterSolveInstance()
        {
            //IGH_DataAccess DA;

            //foreach (var param in Params.Output)
            //{
            //    DA.SetData(param.Name, )
            //}


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
                var proj = PdmClientStore.CurrentPdmClient.GetAllProjects().Where(x => x.GetName() == ghGoo.ToString()).First();
                var objs = proj.Objects;
                foreach (var item in objs)
                {                    
                    listofDocsNames.Add($"{item.GetType().Name} : {item.GetName()}");
                }

            }
            return listofDocsNames;
        }


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F200AC88-7CE8-4060-B94E-B186B99BBAE9"); }
        }
    }
}