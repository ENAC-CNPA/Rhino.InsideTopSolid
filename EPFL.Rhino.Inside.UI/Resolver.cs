using System;
using System.Reflection;
using System.IO;
using Rhino.Runtime.InProcess;


// This line is not mandatory, but improves loading performances
//[assembly: ExtensionApplication(typeof(EPFL.RhinoInsideTopSolid.UI))]


namespace EPFL.RhinoInsideTopSolid.UI
{
    public class Resolver
    {
        /// <summary>
        /// Set up an assembly resolver to load RhinoCommon and other Rhino
        /// assemblies from where Rhino is installed
        /// </summary>
        public static void Initialize()
        {
            if (System.IntPtr.Size != 8)
                throw new Exception("Only 64 bit applications can use RhinoInside");
            AppDomain.CurrentDomain.AssemblyResolve += ResolveForRhinoAssemblies;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveForGhAssemblies;
        }

        //static string _rhinoSystemDirectory;

        /// <summary>
        /// Directory used by assembly resolver to attempt load core Rhino assemblies. If not manually set,
        /// this will be determined by inspecting the registry
        /// </summary>
        //public static string RhinoSystemDirectory
        //{
        //    get
        //    {
        //        if (string.IsNullOrWhiteSpace(_rhinoSystemDirectory))
        //            _rhinoSystemDirectory = FindRhinoSystemDirectory();
        //        return _rhinoSystemDirectory;
        //    }
        //    set
        //    {
        //        _rhinoSystemDirectory = value;
        //    }
        //}

        /// <summary>
        /// Whether or not to use the newest installation of Rhino on the system. By default the resolver will only use an
        /// installation with a matching major version.
        /// </summary>
        public static bool UseLatest { get; set; } = false;

        static Assembly ResolveForRhinoAssemblies(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name).Name;
            string path = System.IO.Path.Combine("C:\\Program Files\\Rhino 7\\System", assemblyName + ".dll");
            //string path = System.IO.Path.Combine(RhinoSystemDirectory, assemblyName + ".dll");
            if (System.IO.File.Exists(path))
                return Assembly.LoadFrom(path);

            return null;
        }
        static Assembly ResolveForGhAssemblies(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name).Name;            

            string path = System.IO.Path.Combine("C:\\Program Files\\Rhino 7\\Plug-ins\\Grasshopper", assemblyName + ".dll");           

            //string path = System.IO.Path.Combine(RhinoSystemDirectory, assemblyName + ".dll");
            if (System.IO.File.Exists(path))
                return Assembly.LoadFrom(path);

            return null;
        }

        //static string FindRhinoSystemDirectory()
        //{
        //    var major = Assembly.GetExecutingAssembly().GetName().Version.Major;
        //    string baseName = @"SOFTWARE\McNeel\Rhinoceros";
        //    using (var baseKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(baseName))
        //    {
        //        string[] children = baseKey.GetSubKeyNames();
        //        Array.Sort(children);
        //        string versionName = "";
        //        for (int i = children.Length - 1; i >= 0; i--)
        //        {
        //            // 20 Jan 2020 S. Baer (https://github.com/mcneel/rhino.inside/issues/248)
        //            // A generic double.TryParse is failing when run under certain locales.
        //            if (double.TryParse(children[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d))
        //            {
        //                versionName = children[i];

        //                if (!UseLatest && (int)Math.Floor(d) != major)
        //                    continue;

        //                using (var installKey = baseKey.OpenSubKey($"{versionName}\\Install"))
        //                {
        //                    string corePath = installKey.GetValue("CoreDllPath") as string;
        //                    if (System.IO.File.Exists(corePath))
        //                    {
        //                        return System.IO.Path.GetDirectoryName(corePath);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}
        
    }
}



//namespace EPFL.RhinoInsideTopSolid.UI33
//{
//    // This class is instantiated by AutoCAD once and kept alive for the 
//    // duration of the session. If you don't do any one time initialization 
//    // then you should remove this class.
//    public class Resolver2 //: IExtensionApplication
//    {
//        private Rhino.Runtime.InProcess.RhinoCore m_rhino_core;

//        #region Plugin static constructor
//        static readonly string SystemDir = (string)Microsoft.Win32.Registry.GetValue
//        (
//          @"HKEY_LOCAL_MACHINE\SOFTWARE\McNeel\Rhinoceros\7.0\Install", "Path",
//          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Rhino WIP", "System")
//        );

//        public void Resolver3()
//        {
//            ResolveEventHandler OnRhinoCommonResolve = null;
//            AppDomain.CurrentDomain.AssemblyResolve += OnRhinoCommonResolve = (sender, args) =>
//            {
//                const string rhinoCommonAssemblyName = "RhinoCommon";
//                var assembly_name = new AssemblyName(args.Name).Name;

//                if (assembly_name != rhinoCommonAssemblyName)
//                    return null;

//                AppDomain.CurrentDomain.AssemblyResolve -= OnRhinoCommonResolve;
//                return Assembly.LoadFrom(Path.Combine(SystemDir, rhinoCommonAssemblyName + ".dll"));
//            };
//        }
//        #endregion // Plugin static constructor

//        #region IExtensionApplication Members

//        public void Initialize() //IExtensionApplication.Initialize()
//        {
//            // Load Rhino
//            try
//            {
//                string SchemeName = "Inside-TopSolid";//$"Inside-{HostApplicationServices.Current.Product}-{HostApplicationServices.Current.releaseMarketVersion}";
//                m_rhino_core = new Rhino.Runtime.InProcess.RhinoCore(new[] { $"/scheme={SchemeName}" });
//            }
//            catch
//            {
//                // ignored
//            }

//        }

//        void Terminate() //IExtensionApplication.Terminate()
//        {
//            try
//            {
//                m_rhino_core?.Dispose();
//            }
//            catch
//            {
//                // ignored
//            }
//        }

//        #endregion // IExtensionApplication Members
//    }
//}
