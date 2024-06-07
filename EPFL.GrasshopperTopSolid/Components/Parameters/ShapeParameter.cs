using EPFL.GrasshopperTopSolid.Components.Dialogs;
using Grasshopper.Kernel.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.UI.D3.Shapes.Controls;
using TopSolid.Kernel.UI.Documents;

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
        DesignDocument doc = TopSolid.Kernel.UI.Application.CurrentDocument as DesignDocument;

        ShapeListEdit shapeListEdit = new ShapeListEdit();
        ShapeEntity shapeEntity;
        protected override void Menu_AppendManageCollection(ToolStripDropDown menu)
        {
            base.Menu_AppendManageCollection(menu);

            Button myButton = new Button();

            myButton.Click += MyButton_Click;

            shapeListEdit.CanHideSelections = true;
            //shapeListEdit.Dock = System.Windows.Forms.DockStyle.Bottom;
            shapeListEdit.IsPickingMultipleWithControl = false;
            //resources.ApplyResources(shapeListEdit, "elementListEdit");
            shapeListEdit.MaximumVisibleItemCount = 10;
            shapeListEdit.Name = "elementListEdit";
            shapeListEdit.NeutralSelectionAcceptance = TopSolid.Kernel.WG.Controls.NeutralSelectionAcceptance.First;
            shapeListEdit.RequiresValue = false;
            shapeListEdit.TabIndex = 1;
            shapeListEdit.PickingEnter += ShapeListEdit_PickingEnter;
            shapeListEdit.PickingSelectionAdded += ShapeListEdit_PickingSelectionAdded;

            //shapeListEdit.AddSelectionFilter()

            //DocumentUserInterface documentInterface = (DocumentUserInterface)doc.UserInterface;
            //TopSolid.Kernel.GR.Selections.SelectionManager selectionManager = documentInterface.ActiveSelectionManager;
            //selectionManager.PickingSelectionAdded += SelectionManager_PickingSelectionAdded;

            Menu_AppendCustomItem(menu, myButton);
        }

        private void MyButton_Click(object sender, EventArgs e)
        {
            ParameterShapeDialog paramShapeDialog = new ParameterShapeDialog(TopSolid.Kernel.UI.Application.ActiveCommand, doc);
            paramShapeDialog.Show();
        }

        private void ShapeListEdit_PickingEnter(object sender, EventArgs e)
        {
            shapeListEdit.FocusPicking();
        }

        private void SelectionManager_PickingSelectionAdded(object inSender, TopSolid.Kernel.GR.Selections.SelectionsEventArgs inE)
        {
            var lastSelection = (inSender as TopSolid.Kernel.GR.Selections.SelectionManager).Selections.Last();

            //if (lastSelection.Hit.GetType is ShapeEntity shape1)
            //{

            //    MessageBox.Show("Test");
            //    this.shapeListEdit.AddSelectedShape(shape1);

            //}
        }

        private void ShapeListEdit_PickingSelectionAdded(object inSender, TopSolid.Kernel.GR.Selections.SelectionsEventArgs inE)
        {
            MessageBox.Show("shape added");

        }
    }
}
