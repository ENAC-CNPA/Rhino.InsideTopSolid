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
        public TSDocSelector2()
          : base("TSDocSelector2", "Nickname",
              "Description",
              "TopSolid", "TopSolid PDM")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("TSProject", "proj", "Gets the documents inside of a TopSolid Project", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        List<Param_GenericObject> listofparams = new List<Param_GenericObject>();

        protected override void BeforeSolveInstance()
        {
            //if (RunCount == -1)
            //{
                Console.WriteLine("No iter has run");
                var x = Params.Input[0].VolatileData;
                var tree = x as GH_Structure<IGH_Goo>;
            

                var listofNames = GetOutputList(tree);
                foreach (var docName in listofNames)
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
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
            DA.GetData(0, ref obj);
            IProject proj = null;
            ;

            if (obj != null)
                proj = PdmClientStore.CurrentPdmClient.GetAllProjects().Where(x => x.GetName() == obj.Value.ToString()).First();

            if (proj == null) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "project is null");
            int i = 0;


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
                foreach (var doc in proj.DocumentItems)
                {
                    listofDocsNames.Add(doc.GetName());
                }

            }
            return listofDocsNames;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
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