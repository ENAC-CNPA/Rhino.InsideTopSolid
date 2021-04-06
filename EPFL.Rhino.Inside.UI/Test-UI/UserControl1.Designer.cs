
namespace EPFL.Rhino.Inside.UI.Test_UI
{
    partial class UserControl1
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.viewportControl1 = new RhinoWindows.Forms.Controls.ViewportControl();
            this.SuspendLayout();
            // 
            // viewportControl1
            // 
            this.viewportControl1.Location = new System.Drawing.Point(3, 3);
            this.viewportControl1.Name = "viewportControl1";
            this.viewportControl1.Size = new System.Drawing.Size(144, 144);
            this.viewportControl1.TabIndex = 0;
            this.viewportControl1.Text = "viewportControl1";
            // 
            // UserControl1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.viewportControl1);
            this.Name = "UserControl1";
            this.ResumeLayout(false);

        }

        #endregion

        private RhinoWindows.Forms.Controls.ViewportControl viewportControl1;
    }
}
