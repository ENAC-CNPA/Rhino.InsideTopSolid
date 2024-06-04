using Rhino;
using Rhino.Runtime.InProcess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhinoWindows.Forms;
using Microsoft.Win32.SafeHandles;
using System.Reflection;

namespace EPFL.RhinoInsideTopSolid.UI
{
    public class Rhinoceros
    {
        static public RhinoCore rhinoCore;
        internal static WindowHandle MainWindow = WindowHandle.Zero;
        public static bool RhinoStartup()
        {
            //Method that works !!
            if (rhinoCore == null)
            {
                var culture = System.Globalization.CultureInfo.CurrentCulture;
                try
                {
                    rhinoCore = new Rhino.Runtime.InProcess.RhinoCore(new string[] { "/ NOSPLASH", "/ language ={ CultureInfo.CurrentCulture.LCID }" }, WindowStyle.Normal);
                }
                catch
                {
                    return false;
                }
                finally
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                }
            }


            return true;
        }

        internal static bool InitEto(Assembly assembly)
        {
            if (Eto.Forms.Application.Instance is null)
                new Eto.Forms.Application(Eto.Platforms.Wpf).Attach();

            return true;
        }

        public static bool RhinoShutdown()
        {
            if (rhinoCore is object)
            {
                try
                {
                    rhinoCore.Dispose();
                    rhinoCore = null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }


        public static bool RunCommandAbout()
        {
            var docSerial = RhinoDoc.ActiveDoc.RuntimeSerialNumber;
            var result = RhinoApp.RunScript("!_About", false);

            if (result && docSerial != RhinoDoc.ActiveDoc.RuntimeSerialNumber)
            {
                return true;
            }

            return false;
        }

        static internal void Show()
        {
            Exposed = true;
            MainWindow.BringToFront();
        }

        //internal static async void ShowAsync()
        //{
        //    //await External.ActivationGate.Yield();

        //    //Show();
        //}

        internal static bool Exposed
        {
            get => MainWindow.Visible && MainWindow.WindowStyle != ProcessWindowStyle.Minimized;
            set
            {
                MainWindow.Visible = value;

                if (value && MainWindow.WindowStyle == ProcessWindowStyle.Minimized)
                    MainWindow.WindowStyle = ProcessWindowStyle.Normal;

                //Added to test if it works
                if (value && MainWindow.WindowStyle == ProcessWindowStyle.Hidden)
                {
                    MainWindow.WindowStyle = ProcessWindowStyle.Normal;
                    MainWindow.Show();
                }

            }
        }


        class ExposureSnapshot
        {
            readonly bool Visible = MainWindow.Visible;
            readonly ProcessWindowStyle Style = MainWindow.WindowStyle;

            public void Restore()
            {
                MainWindow.WindowStyle = Style;
                MainWindow.Visible = Visible;
            }
        }

        static ExposureSnapshot QuiescentExposure;

    }
}
