using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Pdm;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_PDM
{
    public class TSEntityExpand : GH_Component, IGH_VariableParameterComponent
    {
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.EntitiesView, 24, 24).ToBitmap();

        /// <summary>
        /// Initializes a new instance of the TSEntityTree class.
        /// </summary>
        public TSEntityExpand()
          : base("TSEntityExpand", "Expand",
              "Expands a document folder entity",
              "TopSolid", "TopSolid PDM")
        {
        }

        protected override void BeforeSolveInstance()
        {

            IGH_Param ghParam;
            int paramCount = Params.Output.Count;
            while (Params.Output.Count != 0)
            {
                ghParam = Params.Output[0];
                Params.UnregisterOutputParameter(ghParam);
            }

            //for (int i = 0; i < paramCount; i++)
            //{
            //    if (Params.Output[i] != null)
            //    {
            //        ghParam = Params.Output[i];
            //        Params.UnregisterOutputParameter(ghParam);
            //    }
            //}

            if (Params.Output.Count == 0)
            {
                Params.Output.Clear();
            }

            Console.WriteLine("No iter has run");
            var x = Params.Input[0].VolatileData;
            var tree = x as GH_Structure<IGH_Goo>;
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

            //}
            base.BeforeSolveInstance();
            //fixedlistofNames = variablelistofNames;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("TSEnity", "Ent", "TopSolid's Entity to expand", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
            DA.GetData(0, ref obj);
            if (obj == null) return;
            CompositeEntity compEntity = (CompositeEntity)obj.Value;

            foreach (var tsObj in compEntity.Constituents)
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
                CompositeEntity compEntity = (CompositeEntity)ghObj.Value;

                if (compEntity != null)
                {
                    foreach (var item in compEntity.Constituents)
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


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d670565f-8ae9-482b-b3e4-a97806fd8352"); }
        }
    }
}