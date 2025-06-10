using System;
using System.Collections.Generic;
using System.Linq;
using Cirtes.Strato.Cad.DB.Divisions.Slices;
using Cirtes.Strato.Cad.DB.Divisions.Zones;
using Cirtes.Strato.Cad.DB.Documents;
using Cirtes.Strato.Cad.DB.Operations;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.UI;
using TopSolid.Cad.Design.DB;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.SX.Collections.Generic;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Pdm;
using TopSolid.Kernel.UI.D3.Shapes.Controls;

namespace EPFL.GrasshopperTopSolid.Components.STRATO
{
    public class Strats : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Strats class.
        /// </summary>
        public Strats()
          : base("Strats", "S",
              "Gets Stratos layers",
              "TopSolid", "Strato")
        {
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Document", "Doc", "Strato document name or entity", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Slices", "s", "Slices as list", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Thicknesses", "e", "thicknesses as list", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Thin slices", "s", "thin slices as list", GH_ParamAccess.tree);
        }

        SlicePartsDocument SlicePartsDocument;
        bool run = false;
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();//needed for TopSolid types

            //if (!DA.GetData("Bake?", ref run) || !run) return;

            if (DA.GetData("Document", ref wrapper))
            {
                SlicePartsDocument = GetTopSolidDocumentStrato.GetSliceDocument(wrapper);
            }

            if (SlicePartsDocument is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Slice document is null");
                return;
            }

            var zonesFolderEntity = ZonesFolderEntity.GetFolder(SlicePartsDocument);
            if (zonesFolderEntity is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Couldn't find zones folder");
                return;
            }

            if (zonesFolderEntity.Constituents is null || zonesFolderEntity.Constituents.Count() == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "zones folder is empty");
                return;
            }

            //look for targetentities, puis 
            GH_Structure<IGH_Goo> gh_Structure_geometries = new GH_Structure<IGH_Goo>();
            GH_Structure<IGH_Goo> gh_Structure_heights = new GH_Structure<IGH_Goo>();
            //DataTree<object> dataTree = new DataTree<object>();

            GH_Path gh_Path;
            int a = 0, b = 0;
            // List<IGH_GeometricGoo> geometries = new List<IGH_GeometricGoo>();
            //PartEntity partEntity;
            TopSolid.Kernel.SX.Collections.Generic.List<ShapeEntity> shapeEntities = new TopSolid.Kernel.SX.Collections.Generic.List<ShapeEntity>();
            SliceOperation sliceOperation = null;

            foreach (ZoneSetDefinitionEntity zone in zonesFolderEntity.Constituents)
            {
                foreach (var entity in zone.Targets)
                {
                    b = 0;
                    if (entity is SliceSetDefinitionEntity sliceSetDefinitionEntity)
                    {
                        var slices = sliceSetDefinitionEntity.DeepTargets.OfType<PartEntity>();
                        foreach (PartEntity partEntity in sliceSetDefinitionEntity.DeepTargets.OfType<PartEntity>())
                        {
                            shapeEntities.Clear();
                            if (partEntity == null) continue;
                            if (!partEntity.IsAlive) continue;

                            partEntity.GetDeepShapes(partEntity.GetRepresentationTags().FirstOrDefault(), shapeEntities);


                            gh_Path = new GH_Path(a, b);
                            gh_Structure_geometries.AppendRange(shapeEntities.Select(se => new GH_Brep(se.Geometry.ToRhino().FirstOrDefault())), gh_Path);
                            //geometries.Clear();


                            foreach (SliceOperation operation in SlicePartsDocument.SlicingStageOperation.Operations.OfType<SliceOperation>())
                            {
                                SliceManagementOperation managementOperation = operation.Operations.OfType<SliceManagementOperation>().FirstOrDefault();
                                if (managementOperation == null) continue;
                                if (managementOperation.Set == zone)
                                {
                                    sliceOperation = operation;
                                    TopSolid.Kernel.SX.Collections.Generic.List<PartEntity> partEntities
                                            = new TopSolid.Kernel.SX.Collections.Generic.List<PartEntity> { partEntity };

                                    Frame slicingFrame = managementOperation.SlicingAxis.Geometry.MakeFrame();
                                    SliceManagementOperation.MakeTopAndBottomPlanes(partEntities, slicingFrame, out _, out _, out double height);
                                    gh_Structure_heights.AppendRange(new System.Collections.Generic.List<GH_Number> { new GH_Number(height) }, gh_Path);

                                    break;

                                }
                            }

                        }
                        b++;

                    }

                    // SliceOperation sliceOperation = SlicePartsDocument.SlicingStageOperation.Operations.
                    // OfType<SliceOperation>().FirstOrDefault(so => so.ManagementOperation != null && so.ManagementOperation.Set == zone);



                }
                a++;
            }

            DA.SetDataTree(0, gh_Structure_geometries);
            DA.SetDataTree(1, gh_Structure_heights);
            //DA.SetDataList("Thicknesses", SlicePartsDocument.CutsFolderEntity.DeepCuts.Select(x=>x.)
        }

        private void GetSliceDocumentp(GH_ObjectWrapper wrapper)
        {
            IDocument res = null;
            if (wrapper.Value is string || wrapper.Value is GH_String)
            {
                res = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault();
                SlicePartsDocument = res as SlicePartsDocument;
            }
            else if (wrapper.Value is IDocumentItem)
                SlicePartsDocument = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as SlicePartsDocument;
            else if (wrapper.Value is IDocument)
                SlicePartsDocument = wrapper.Value as SlicePartsDocument;

            if (SlicePartsDocument == null)
                SlicePartsDocument = TopSolid.Kernel.UI.Application.CurrentDocument as SlicePartsDocument;
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => new System.Drawing.Icon(Properties.Resources.Cirtes_Strato_Cad_UI_Divisions_Slices_SliceCommand, 24, 24).ToBitmap();


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("B52B6E79-CA8D-49D3-BFD0-3B1B0C7F405D"); }
        }
    }
}