using Rhino.Runtime.InProcess;
using System;
using System.Windows.Forms;
using TopSolid.Kernel.WX;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.G.D3.Curves;
using TopSolid.Kernel.GR.D3;
using TopSolid.Kernel.GR.Displays;
using TopSolid.Kernel.GR.Items;
using TopSolid.Kernel.SX.Drawing;
using TopSolid.Kernel.SX.Resources;
using TopSolid.Kernel.UI.D3;
using TopSolid.Kernel.UI.Trees;
using TopSolid.Kernel.WX.Docking;
using TopSolid.Kernel.WX.EnhancedContainers;

namespace EPFL.Rhino.Inside.UI
{
    public partial class Form1 : Form
    {
        global::Rhino.Runtime.InProcess.RhinoCore rhinoCore;

        private DockedContent dockedContent = null;
        public DockedContent DockedContent
        {
            get
            {
                return dockedContent;
            }
            set
            {
                dockedContent = value;
            }
        }
        public Form1()
        {
            InitializeComponent();
        }

        public void AddOrModifyDockedWindow()
        {
            // Create a new application window and update the list box
            //ApplicationWindow window = TopSolid.Kernel.WX.Application.Window;
            ApplicationWindow window = new TopSolid.Kernel.WX.ApplicationWindow();


            //DockedContent = window.AddDockedContent(
            //    this,
            //    GetType(),
            //    DockedContentVisibility.Visible,
            //    "Rhino",
            //    typeof(Resources), "MyWindow.ico",
            //    true,
            //    DockingState.Left,
            //    false);

        }
            
        protected override void OnHandleCreated(EventArgs e)
        {
            rhinoCore = new global::Rhino.Runtime.InProcess.RhinoCore(new string[] { "/NOSPLASH" }, WindowStyle.Hidden, Handle);
            base.OnHandleCreated(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            rhinoCore.Dispose();
            rhinoCore = null;
            base.OnHandleDestroyed(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            global::Rhino.RhinoDoc.ActiveDoc.Objects.AddSphere(new global::Rhino.Geometry.Sphere(global::Rhino.Geometry.Point3d.Origin, 10));
        }
    }
}
