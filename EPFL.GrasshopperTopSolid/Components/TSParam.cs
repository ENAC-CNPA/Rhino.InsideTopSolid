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
                switch (entity.GetType().ToString())
                {
                    case "TopSolid.Kernel.DB.Parameters.TextParameterEntity":
                        TopSolid.Kernel.TX.Undo.UndoSequence.UndoCurrent();
                        TopSolid.Kernel.TX.Undo.UndoSequence.Start("textparam", true);
                        var _textParam = entity as TextParameterEntity;
                        string _string = "";
                        Grasshopper.Kernel.Types.GH_ObjectWrapper _strobj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
                        DA.GetData(1, ref _strobj);
                        GH_Convert.ToString(_strobj.Value, out _string, GH_Conversion.Both);
                        _textParam.Value = _string;
                        TopSolid.Kernel.TX.Undo.UndoSequence.End();
                        break;

                    case "TopSolid.Kernel.DB.Parameters.BooleanParameterEntity":
                        TopSolid.Kernel.TX.Undo.UndoSequence.UndoCurrent();
                        TopSolid.Kernel.TX.Undo.UndoSequence.Start("boolparam", true);
                        var _boolParam = entity as BooleanParameterEntity;
                        bool _bool = true;
                        Grasshopper.Kernel.Types.GH_ObjectWrapper _boolobj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
                        DA.GetData(1, ref _boolobj);
                        GH_Convert.ToBoolean(_boolobj.Value, out _bool, GH_Conversion.Both);
                        _boolParam.Value = _bool;
                        TopSolid.Kernel.TX.Undo.UndoSequence.End();
                        break;

                    case "TopSolid.Kernel.DB.Parameters.RealParameterEntity":
                        TopSolid.Kernel.TX.Undo.UndoSequence.UndoCurrent();
                        TopSolid.Kernel.TX.Undo.UndoSequence.Start("realparam", true);
                        var _realParam = entity as RealParameterEntity;
                        double _double = 0.0;
                        Grasshopper.Kernel.Types.GH_ObjectWrapper _realobj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
                        DA.GetData(1, ref _realobj);
                        GH_Convert.ToDouble(_realobj.Value, out _double, GH_Conversion.Both);
                        _realParam.Value = _double;
                        TopSolid.Kernel.TX.Undo.UndoSequence.End();
                        break;

                    case "TopSolid.Kernel.DB.Parameters.IntegerParameterEntity":

                        TopSolid.Kernel.TX.Undo.UndoSequence.UndoCurrent();
                        TopSolid.Kernel.TX.Undo.UndoSequence.Start("intparam", true);


                        IntegerParameterEntity _intParam = entity as IntegerParameterEntity;
                        int _int = 0;
                        Grasshopper.Kernel.Types.GH_ObjectWrapper _intobj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
                        DA.GetData(1, ref _intobj);
                        GH_Convert.ToInt32(_intobj.Value, out _int, GH_Conversion.Both);
                        _intParam.Value = _int;

                        TopSolid.Kernel.TX.Undo.UndoSequence.End();
                        break;

                    default:
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Parameter Type not Supported");
                        return;
                }
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
