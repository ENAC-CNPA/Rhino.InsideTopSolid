using Grasshopper.Kernel;
using System;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.DB.Parameters;
using TopSolid.Kernel.SX;

namespace EPFL.GrasshopperTopSolid.Components
{
    public class TSParam : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TSParam class.
        /// </summary>
        public TSParam()
          : base("TSParam", "TSParam",
              "Controls the Value of a TopSolid Parameter",
              "TopSolid", "TopSolid Entities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of the Parameter to Get", GH_ParamAccess.item);
            pManager.AddGenericParameter("Value", "V", "Value to feed", GH_ParamAccess.item);
            Params.Input[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Value", "V", "Parameter Value", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            DA.GetData(0, ref name);
            GeometricDocument document = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;
            ParameterEntity parameterEntity = document.RootEntity.SearchDeepEntity(name) as ParameterEntity;
            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
            

            if (parameterEntity == null)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No entity was found");
            else 
            {
                DA.GetData(1, ref obj);
                TopSolid.Kernel.TX.Undo.UndoSequence.UndoCurrent();
                TopSolid.Kernel.TX.Undo.UndoSequence.Start("intparam", true);
                
                if (parameterEntity is TextParameterEntity textparam)
                {
                    textparam.Value = obj.Value.ToString();
                    DA.SetData(0, textparam.Value);
                }
                else if (parameterEntity is BooleanParameterEntity boolparam)
                {
                    if (GH_Convert.ToBoolean(obj.Value, out bool value, GH_Conversion.Both))
                    {
                        boolparam.Value = value;
                        DA.SetData(0, boolparam.Value);
                    }

                }

                else if (parameterEntity is RealParameterEntity realparam)
                {
                    if (GH_Convert.ToDouble(obj.Value, out double value, GH_Conversion.Both))
                    {
                        realparam.Value = value;
                        DA.SetData(0, realparam.Value);
                    }
                }
                else if (parameterEntity is IntegerParameterEntity integerParameter)
                {
                    if (GH_Convert.ToInt32(obj.Value, out int value, GH_Conversion.Both))
                    {
                        integerParameter.Value = value;
                        DA.SetData(0, integerParameter.Value);
                    }
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Parameter Type not Supported");                    
                }

                TopSolid.Kernel.UI.Application.Update();
                TopSolid.Kernel.TX.Undo.UndoSequence.End();
            }

            
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.TopSolid;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override System.Guid ComponentGuid
        {
            get { return new System.Guid("5569f147-3d74-443f-a9ba-1b04bf47287f"); }
        }
    }
}
