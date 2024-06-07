using TopSolid.Kernel.UI.Parameters;
using TopSolid.Kernel.WX.Controls;
using TK = TopSolid.Kernel;


namespace EPFL.GrasshopperTopSolid.Components.Dialogs
{
    partial class ParameterShapeDialog
    {


        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.shapeListEdit1 = new TopSolid.Kernel.UI.D3.Shapes.Controls.ShapeListEdit();
            this.SuspendLayout();
            // 
            // shapeListEdit1
            // 
            this.shapeListEdit1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.shapeListEdit1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.shapeListEdit1.Location = new System.Drawing.Point(0, 0);
            this.shapeListEdit1.Name = "shapeListEdit1";
            this.shapeListEdit1.Size = new System.Drawing.Size(150, 156);
            this.shapeListEdit1.TabIndex = 0;
            this.shapeListEdit1.UnselectItemsOnEmptyAreaClick = true;
            // 
            // ParameterShapeDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.shapeListEdit1);
            this.Name = "ParameterShapeDialog";
            this.Size = new System.Drawing.Size(150, 156);
            this.ResumeLayout(false);

        }
        #endregion

        private TK.UI.D3.Shapes.Controls.ShapeListEdit shapeListEdit1;
    }
}
