using System;
using System.Collections.Generic;
using System.Linq;
using Cirtes.Strato.Cad.DB.Divisions.Slices;
using Cirtes.Strato.Cad.DB.Divisions.Zones;
using Cirtes.Strato.Cad.DB.Documents;
using Cirtes.Strato.Cad.UI.Controls;
using Grasshopper.Documentation;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.UI;
using TopSolid.Cad.Design.DB;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.Sets;
using TopSolid.Kernel.G.D3;

namespace EPFL.GrasshopperTopSolid.Components.STRATO
{
    public class GetSlice : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetSlice class.
        /// </summary>
        public GetSlice()
          : base("GetSlice", "Slice",
              "Gets a Strato Slice out in a Zone",
              "TopSolid", "Strato")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("StratoDocument", "doc", "TopSolid Strato Document", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager.AddTextParameter("Zone", "Z", "name of desired Zone", GH_ParamAccess.item);
            pManager.AddTextParameter("Slice", "s", "name of desired Slice", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("geometry", "g", "slice rhino geometry", GH_ParamAccess.list);
            pManager.AddNumberParameter("height", "h", "slice height", GH_ParamAccess.item);
        }



        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            SlicePartsDocument slicePartsDocument = null;
            GH_ObjectWrapper wrapper = new GH_ObjectWrapper();
            if (!DA.GetData(0, ref wrapper))
            {
                slicePartsDocument = TopSolid.Kernel.UI.Application.CurrentDocument as SlicePartsDocument;
                if (slicePartsDocument is null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "could not find Strato document");
                    return;
                }

            }


            slicePartsDocument = GetTopSolidDocumentStrato.GetSliceDocument(wrapper);

            ZonesFolderEntity zonesFolderEntity = ZonesFolderEntity.GetFolder(slicePartsDocument);
            if (zonesFolderEntity is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Couldn't find zones folder");
                return;
            }
            string zoneName = "";
            string sliceName = "";
            PartEntity partEntity = null;
            if (!DA.GetData(1, ref zoneName)) return;
            if (!DA.GetData(2, ref sliceName)) return;

            List<Brep> breps = new List<Brep>();

            var zone = zonesFolderEntity.SearchEntity(zoneName) as ZoneSetDefinitionEntity;
            if (zone != null)
            {
                var list = zone.Targets.Where(s => s.LocalizedName == sliceName)
                    .Select(x => x as SliceSetDefinitionEntity).FirstOrDefault().Targets;
                foreach (var slice in list)
                {
                    partEntity = slice as PartEntity;
                    if (partEntity is null) continue;
                    if (!partEntity.IsAlive) continue;

                    var shapes = partEntity.CurrentRepresentationConstituents.OfType<ShapeEntity>().Select(x => x.Geometry.ToRhino().FirstOrDefault());
                    breps.AddRange(shapes);

                    SliceOperation sliceOperation = null;
                    foreach (SliceOperation operation in slicePartsDocument.SlicingStageOperation.Operations.OfType<SliceOperation>())
                    {
                        SliceManagementOperation managementOperation = operation.Operations.OfType<SliceManagementOperation>().FirstOrDefault();
                        if (managementOperation == null) continue;
                        if (managementOperation.Set == zone)
                        {
                            sliceOperation = operation;
                            TopSolid.Kernel.SX.Collections.Generic.List<PartEntity> partEntities
                                    = new TopSolid.Kernel.SX.Collections.Generic.List<PartEntity>(list.Select(x => x as PartEntity), false);

                            Frame slicingFrame = managementOperation.SlicingAxis.Geometry.MakeFrame();
                            SliceManagementOperation.MakeTopAndBottomPlanes(partEntities, slicingFrame, out _, out _, out double height);
                            DA.SetData(1, height);
                            break;

                        }
                    }
                }



            }

            DA.SetDataList(0, breps);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("33ED84B8-49B8-4F46-B256-586732640A0C"); }
        }
    }
}