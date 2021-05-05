using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.UI;
using System.Linq;
using TopSolid.Kernel.DB.Entities;

namespace EPFL.GrasshopperTopSolid
{
    public class GetFolderEntity : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GetFolderEntity()
          : base("GetFolderEntity", "Folders",
            "Get all Folder Entities",
            "TopSolid", "TopSolid Entities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {            
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("GetFolders", "F", "FoldersEntities", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometricDocument document = TopSolid.Kernel.UI.Application.CurrentDocument as GeometricDocument;
            TopSolid.Kernel.TX.Undo.UndoSequence.UndoCurrent();
            TopSolid.Kernel.TX.Undo.UndoSequence.Start("Test", true);
                        
            var L = document.RootEntity.Entities.Where(x => x is FolderEntity);
            
            TopSolid.Kernel.TX.Undo.UndoSequence.End();

            DA.SetData(0, L);

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Geometrie_Old;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7FBC3970-FE81-4435-9412-2907B8E824E9");
    }
}