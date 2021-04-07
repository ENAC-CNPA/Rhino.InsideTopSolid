using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TopSolid.Kernel.WX.EnhancedContainers;
using TopSolid.Kernel.WX;
using TopSolid.Kernel.WX.Docking;
using Rhino.Runtime.InProcess;
using Rhino;
using Grasshopper;

namespace EPFL.Rhino.Inside.UI
{
    public partial class UserControl1 : EnhancedContainer
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
        public UserControl1()
        {
            InitializeComponent();
        }
                public void AddOrModifyDockedWindow()
        {
            // Create a new application window and update the list box
            ApplicationWindow window = TopSolid.Kernel.WX.Application.Window;

            //if (window == null)
            //{
            //    UpdateListBox();
            //    return;
            //}

            //ResourceManager res = TopSolid.AdsSamples.Kernel.CustomWindow.WX.Resources.Manager;

            DockedContent = window.AddDockedContent(
                this,
                GetType(),
                DockedContentVisibility.Visible,
                "RhinoInside TopSolid", //res.GetString("MyWindow"),
                typeof(Resources), "MyWindow.ico",
                true,
                DockingState.Floating,
                false);

            // Update the list box
            //UpdateListBox();
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
            //global::Rhino.RhinoDoc.ActiveDoc.Objects.AddSphere(new global::Rhino.Geometry.Sphere(global::Rhino.Geometry.Point3d.Origin, 10));

            global::Rhino.RhinoDoc.Open(@"C:\Sources\RhinoInsideTopSo.3dm", out bool alreadyOpen);

            viewportControl1.Viewport.DisplayMode = global::Rhino.Display.DisplayModeDescription.FindByName("Arctic");
            viewportControl1.Invalidate();
        }

        
        
    }
}
