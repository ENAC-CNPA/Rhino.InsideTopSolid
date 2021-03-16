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

namespace EPFL.RhinoInsideTopSolid.UI
{
    public partial class Form1 : Form
    {
        Rhino.Runtime.InProcess.RhinoCore rhinoCore;
        public Form1()
        {
            InitializeComponent();            
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            rhinoCore = new Rhino.Runtime.InProcess.RhinoCore(new string[] { "/NOSPLASH" }, WindowStyle.Hidden, Handle);
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
            Rhino.RhinoDoc.ActiveDoc.Objects.AddSphere(new Rhino.Geometry.Sphere(Rhino.Geometry.Point3d.Origin, 10));
        }
    }
}
