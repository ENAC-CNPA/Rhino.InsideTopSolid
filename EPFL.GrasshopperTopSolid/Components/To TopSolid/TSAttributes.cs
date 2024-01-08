using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using TopSolid.Kernel.SX.Drawing;
using TopSolid.Kernel.DB.Entities;
using Grasshopper.Kernel.Types;

namespace EPFL.GrasshopperTopSolid.Components.Preview
{
    public class TSAttributes : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TSAttributes class.
        /// </summary>
        public TSAttributes()
          : base("TSAttributes", "Attributes",
              "takes in Colour, Transparency and Layer info to profide an attributes set",
              "TopSolid", "To TopSolid")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Colour", "Colour", "Explicit Color and Transparency", GH_ParamAccess.item);
            pManager.AddTextParameter("Layer", "Layer", "Layer Name", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("TSAttributes", "Attributes", "TopSolid Explicit Attributes", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Colour ghColour = null;
            string layer = "";

            if (!DA.GetData("Colour", ref ghColour) || !DA.GetData("Layer", ref layer))
                return;
            if (ghColour == null)
                return;

            Color color = new Color(ghColour.Value.R, ghColour.Value.G, ghColour.Value.B);
            Transparency transp = Transparency.FromByte((byte)(byte.MaxValue - ghColour.Value.A));

            Tuple<Transparency, Color, string> tuple = new Tuple<Transparency, Color, string>(transp, color, layer);
            DA.SetData("TSAttributes", tuple);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.EntityAttributesCommand, 24, 24).ToBitmap();

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4703eb6f-f7e2-4bdf-8244-c5c2178711a3"); }
        }
    }
}