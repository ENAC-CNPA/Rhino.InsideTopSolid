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
using Grasshopper;
using Grasshopper.GUI.Canvas;
using System.IO;
using EPFL.RhinoInsideTopSolid.UI.GHTS;

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
                    rhinoCore = new Rhino.Runtime.InProcess.RhinoCore(new string[] { "/NOSPLASH", $"/language={culture.LCID}" }, WindowStyle.Hidden);
                    MainWindow = (WindowHandle)RhinoApp.MainWindowHandle();
                    MainWindow.ExtendedWindowStyles |= ExtendedWindowStyles.AppWindow;

                    Instances.CanvasCreated += EditorLoaded;
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

        //Can Delete, only useful if needed to use Eto before launching Rhino, Because Rhino will do it otherwise
        //internal static bool InitEto(Assembly assembly)
        //{
        //    if (Eto.Forms.Application.Instance is null)
        //        new Eto.Forms.Application(Eto.Platforms.Wpf).Attach();

        //    return true;
        //}

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

        private static void EditorLoaded(GH_Canvas canvas)
        {
            Instances.CanvasCreated += EditorLoaded;

            var message = string.Empty;
            try
            {
                if (!GrasshopperCommand.LoadGrasshopperComponents())
                    message = "Failed to load Revit Grasshopper components.";
            }
            catch (FileNotFoundException e) { message = $"{e.Message}{Environment.NewLine}{e.FileName}"; }
            catch (Exception e) { message = e.Message; }

            if (!string.IsNullOrEmpty(message))
            {
                System.Windows.Forms.MessageBox.Show
                (
                  System.Windows.Forms.Form.ActiveForm,
                  message, "Error",
                  System.Windows.Forms.MessageBoxButtons.OK,
                  System.Windows.Forms.MessageBoxIcon.Error
                );
            }
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
