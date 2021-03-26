using Rhino.Runtime.InProcess;
using System;
using System.Windows.Forms;

namespace EPFL.Rhino.Inside.UI
{
    public partial class Form1 : Form
    {
        global::Rhino.Runtime.InProcess.RhinoCore rhinoCore;
        public Form1()
        {
            InitializeComponent();
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
