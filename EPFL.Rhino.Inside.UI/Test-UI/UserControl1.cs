using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rhino.Runtime.InProcess;

namespace EPFL.Rhino.Inside.UI.Test_UI
{
    public partial class UserControl1 : UserControl
    {
        global::Rhino.Runtime.InProcess.RhinoCore rhinoCore;
        public UserControl1()
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
