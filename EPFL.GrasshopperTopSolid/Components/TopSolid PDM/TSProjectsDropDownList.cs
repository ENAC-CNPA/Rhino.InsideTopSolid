using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using TopSolid.Kernel.TX.Pdm;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_PDM
{
    public class TSProjectsDropDownList : GH_ValueList
    {
        public override Guid ComponentGuid => new Guid("f151bfa4-d60b-4267-9eb0-184edc0ae091");

        protected override System.Drawing.Bitmap Icon => Properties.Resources.TSProjectDocument.ToBitmap();
        public TSProjectsDropDownList()
        {
            Category = "TopSolid";
            SubCategory = "TopSolid PDM";
            NickName = "TSDocs";
            MutableNickName = false;
            Name = $"{NickName} Categories Picker";
            Description = $"Provides a {NickName} Category picker";

            ListMode = GH_ValueListMode.DropDown;


            ListItems.Clear();
            var projects = PdmClientStore.CurrentPdmClient.GetAllProjects();
            foreach (var project in projects)
            {
                var item = new GH_ValueListItem(project.GetName(), '"' + project.GetName() + '"');
                item.Selected = project.GetName().StartsWith("A");
                ListItems.Add(item);
            }


            base.CollectVolatileData_Custom();
        }





    }
}
