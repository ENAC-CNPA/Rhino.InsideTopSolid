using Cirtes.Strato.Cad.DB.Documents;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Pdm;

namespace EPFL.GrasshopperTopSolid
{
    internal class GetTopSolidDocumentStrato
    {
        public static SlicePartsDocument GetSliceDocument(GH_ObjectWrapper wrapper)
        {
            IDocument res = null;
            SlicePartsDocument slicePartsDocument = null;
            if (wrapper.Value is string || wrapper.Value is GH_String)
            {
                res = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault();
                slicePartsDocument = res as SlicePartsDocument;
            }
            else if (wrapper.Value is IDocumentItem)
                slicePartsDocument = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as SlicePartsDocument;
            else if (wrapper.Value is IDocument)
                slicePartsDocument = wrapper.Value as SlicePartsDocument;

            if (slicePartsDocument is null)
                slicePartsDocument = TopSolid.Kernel.UI.Application.CurrentDocument as SlicePartsDocument;

            return slicePartsDocument;
        }
    }
}
