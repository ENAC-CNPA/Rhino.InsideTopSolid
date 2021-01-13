﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using TK = TopSolid.Kernel;

namespace EPFL.Rhino.Inside.Addin
{
	/// <summary>
	/// Represents sample command by add-in.
	/// </summary>
	[GuidAttribute("62b69852-5ad7-4ae1-9470-b7cac5cef940")]
	public class AddIn : TK.TX.AddIns.AddIn
	{
		/// <summary>
		/// Overrides <see cref="TK.TX.AddIns.AddIn.Name"/>
		/// Name of Application. 
		/// </summary>
		public override string Name
		{
			get { return Resources.Manager.GetString("$Name"); }
		}

		/// <summary>
		/// Overrides <see cref="TK.TX.AddIns.AddIn.Description"/>.
		/// </summary>
		public override string[] Description
		{
			get
			{
				string[] description = new string[1];
				description[0] = Resources.Manager.GetString("$Description");
				return description;
			}
		}

		/// <summary>
		/// Overrides <see cref="TK.TX.AddIns.AddIn.Manufacturer"/>.
		/// Name of Manufacturer.
		/// </summary>
		public override string Manufacturer
		{
			get
			{
				return Resources.Manager.GetString("$Manufacturer");
			}
		}

		/// <summary>
		/// Overrides <see cref="TK.TX.AddIns.AddIn.RequiredAddIns"/>.
		/// single Key to reference Addin.
		/// </summary>
		public override Guid[] RequiredAddIns
		{
			get { return new Guid[0]; }
		}

		static readonly string SystemDir = (string)Microsoft.Win32.Registry.GetValue
		(
		  @"HKEY_LOCAL_MACHINE\SOFTWARE\McNeel\Rhinoceros\7.0\Install", "Path",
		  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Rhino 7", "System")
		);

		static AddIn()
		{
			// Force the loading of RhinoCommon.dll to not have the unknown DLL message.
			const string rhinoCommonAssemblyName = "RhinoCommon";
			Assembly.LoadFrom(Path.Combine(SystemDir, rhinoCommonAssemblyName + ".dll"));
		}

		/// <summary>
		/// Overrides <see cref="TK.TX.AddIns.AddIn.InitializeSession"/>.
		/// Initialization of Session for display in Topsolid.
		/// </summary>
		public override void InitializeSession()
		{
			RhinoInsideTopSolid.UI.ContextMenu.AddMenu();
		}

		/// <summary>
		/// Overrides <see cref="TK.TX.AddIns.AddIn.StartSession"/>.
		/// Start the session
		/// </summary>
		public override void StartSession()
		{
			TK.SX.SessionManager.Start(typeof(EPFL.RhinoInsideTopSolid.UI.Session));
		}

		/// <summary>
		/// Overrides <see cref="TK.TX.AddIns.AddIn.EndSession"/>.
		/// Close the session.
		/// </summary>
		public override void EndSession()
		{

		}

		/// <summary>
		/// Overrides <see cref="TK.TX.AddIns.AddIn.GetRegistrationCertificate"/>.
		/// </summary>
		public override string GetRegistrationCertificate()
		{
			return TK.SX.String.ReadResourceTextFile(typeof(AddIn), "EPFL.Rhino.Inside.Addin.xml");

		}
	}
}
