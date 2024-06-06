using Grasshopper.Kernel.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TopSolid.Kernel.UI.D3.Shapes.Controls;

namespace EPFL.GrasshopperTopSolid.Components.Parameters
{
    public class ShapeParameter : Param_GenericObject
    {
        public ShapeParameter()
        {
            Name = "Shape";
            NickName = "Shape";
            Description = "Contains a collection of TS Shapes";
            Category = "TopSolid";
            SubCategory = "Primitive";

        }

        public override Guid ComponentGuid => new Guid("CD8D4B20-7D5D-40CE-84F4-E866D85B15D0");


        protected override void Menu_AppendManageCollection(ToolStripDropDown menu)
        {
            base.Menu_AppendManageCollection(menu);

            var shapeListEdit = new ShapeListEdit();
            shapeListEdit.CanHideSelections = true;
            //shapeListEdit.Dock = System.Windows.Forms.DockStyle.Bottom;
            shapeListEdit.IsPickingMultipleWithControl = false;
            //resources.ApplyResources(shapeListEdit, "elementListEdit");
            shapeListEdit.MaximumVisibleItemCount = 10;
            shapeListEdit.Name = "elementListEdit";
            shapeListEdit.NeutralSelectionAcceptance = TopSolid.Kernel.WG.Controls.NeutralSelectionAcceptance.First;
            shapeListEdit.RequiresValue = false;
            shapeListEdit.TabIndex = 1;


            Menu_AppendCustomItem(menu, shapeListEdit);
        }


    }
}
