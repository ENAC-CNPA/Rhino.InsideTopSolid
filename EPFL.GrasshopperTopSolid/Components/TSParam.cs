using Grasshopper.Kernel;
using System;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.DB.Parameters;

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
            string name = "";
            DA.GetData(0, ref name);
            GeometricDocument document = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;
            Entity entity = document.RootEntity.SearchDeepEntity(name);

            if (entity == null)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No entity was found");
            else
            {
                TopSolid.Kernel.TX.Undo.UndoSequence.UndoCurrent();
                TopSolid.Kernel.TX.Undo.UndoSequence.Start("intparam", true);
                Grasshopper.Kernel.Types.GH_ObjectWrapper _obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
                DA.GetData(1, ref _obj);
                switch (entity.GetType().ToString())
                {
                    case "TopSolid.Kernel.DB.Parameters.TextParameterEntity":
                        var _textParam = entity as TextParameterEntity;
                        string _string = "";
                        if (GH_Convert.ToString(_obj.Value, out _string, GH_Conversion.Both))
                            _textParam.Value = _string;
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "input format not string");
                        break;

                    case "TopSolid.Kernel.DB.Parameters.BooleanParameterEntity":
                        var _boolParam = entity as BooleanParameterEntity;
                        bool _bool = true;
                        if (GH_Convert.ToBoolean(_obj.Value, out _bool, GH_Conversion.Both))
                            _boolParam.Value = _bool;
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "input format not bool");
                        break;

                    case "TopSolid.Kernel.DB.Parameters.RealParameterEntity":
                        var _realParam = entity as RealParameterEntity;
                        double _double = 0.0;
                        if (GH_Convert.ToDouble(_obj.Value, out _double, GH_Conversion.Both))
                            _realParam.Value = _double;
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "input format not float");
                        break;

                    case "TopSolid.Kernel.DB.Parameters.IntegerParameterEntity":
                        IntegerParameterEntity _intParam = entity as IntegerParameterEntity;
                        int _int = 0;
                        if (GH_Convert.ToInt32(_obj.Value, out _int, GH_Conversion.Both))
                            _intParam.Value = _int;
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "input format not integer");
                        break;

                    default:
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Parameter Type not Supported");
                        return;
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
            get { return new Guid("5569f147-3d74-443f-a9ba-1b04bf47287f"); }
        }
    }
}
