using Rhino.Runtime.InProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPFL.RhinoInsideTopSolid.UI
{
    public class Rhinoceros
    {
        static RhinoCore rhinoCore;
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

    }
}
