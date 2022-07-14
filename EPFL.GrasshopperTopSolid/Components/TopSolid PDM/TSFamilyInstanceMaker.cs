using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TopSolid.Cad.Design.DB;
using TopSolid.Cad.Design.DB.Configurations;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Cad.Design.DB.Inclusion;
using TopSolid.Cad.Design.DB.Representations;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.Documents;
using TopSolid.Kernel.DB.Families;
using TopSolid.Kernel.DB.Families.Documents;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Families;
using TopSolid.Kernel.TX.Pdm;
using TopSolid.Kernel.TX.Undo;

namespace EPFL.GrasshopperTopSolid.Components.TopSolid_PDM
{
    public class TSFamilyInstanceMaker : GH_Component
    {
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.FamilyDocument, 24, 24).ToBitmap();

        /// <summary>
        /// Initializes a new instance of the TSFamilyInstanceMaker class.
        /// </summary>
        public TSFamilyInstanceMaker()
          : base("TSFamilyInstanceMaker", "TSFamily",
              "Instanciates Family in an Assembly",
              "TopSolid", "TopSolid PDM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FamilyDoc", "Family", "Family Document to include", GH_ParamAccess.item);
            pManager.AddGenericParameter("AssemblyDoc", "Assembly", "Assembly Document to include the family in", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void BeforeSolveInstance()
        {

            base.BeforeSolveInstance();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Setting target document from input, or else take current document by default
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();
            FamilyDocument family = null;
            AssemblyDocument assembly = null;
            IDocument res = null;
            if (DA.GetData("FamilyDoc", ref wrapper))
            {
                if (wrapper.Value is string || wrapper.Value is GH_String)
                {
                    res = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault();
                    family = res as FamilyDocument;
                }
                else if (wrapper.Value is IDocumentItem)
                    family = (wrapper.Value as IFamilyDocumentItem).OpenLastValidMinorRevisionDocument() as FamilyDocument;
                else if (family is IDocument)
                    family = wrapper.Value as FamilyDocument;
            }

            if (DA.GetData("AssemblyDoc", ref wrapper))
            {
                if (wrapper.Value is string || wrapper.Value is GH_String)
                {
                    res = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault();
                    assembly = res as AssemblyDocument;
                }
                else if (wrapper.Value is IDocumentItem)
                    assembly = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as AssemblyDocument;
                else if (wrapper.Value is IDocument)
                    assembly = wrapper.Value as AssemblyDocument;
            }

            UndoSequence.UndoCurrent();
            UndoSequence.Start("fam", true);

            assembly.EnsureIsDirty();


            DocumentInstanceSignature sig = new DocumentInstanceSignature(null, null);
            string msg = "";
            InstanceMaker maker = InstanceMaker.Make(family, sig, out msg);

            var values = maker.DriverValues;
            Document instance = null;
            bool succes = maker.MakeInstanceDocument(assembly, out instance, out msg);
            values = maker.DriverValues;
            var designDoc = instance as DesignDocument;
            InclusionOperation inclusionOperation = new InclusionOperation(assembly, 0, instance as DesignDocument, maker, (instance as DesignDocument).CurrentRepresentationEntity, false, new ConfigurationEntity(assembly, 0), null);

            inclusionOperation.Create();

            //TopSolid.Cad.Design.DB.RigidGroups.RigidGroupDefinitionEntity rgde = null;
            //assembly.IncludeMechanicalDocument(family, null, null, false, null, true, null, true, TopSolid.Cad.Design.DB.Constraints.FixedAddingMode.None, rgde);





            UndoSequence.End();

        }


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F45A661C-0436-41E8-B955-02FF78E555B0"); }
        }
    }
}