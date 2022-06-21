using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using TopSolid.Kernel.TX.Pdm;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_PDM
{
    public class TSDocumentPicker : GH_ValueList
    {
        public override Guid ComponentGuid => new Guid("f79c93ae-0e84-4652-8a10-f357929d5598");
        public TSDocumentPicker()
        {
            Name = "TSDocument Picker";
            NickName = "Doc Picker";
            Category = "TopSolid";
            SubCategory = "TopSolid PDM";

            ListMode = GH_ValueListMode.DropDown;
            ListItems.Clear();
            var y = new Param_GenericObject();

            var x = Sources;
            x.Add(y);
            bool b = y.IsDataProvider;


        }



        protected override void CollectVolatileData_FromSources()
        {
            base.CollectVolatileData_FromSources();

            NickName = string.Empty;
        }

    }
}
