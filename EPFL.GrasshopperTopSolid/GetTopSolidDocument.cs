

using EPFL.GrasshopperTopSolid;
using Grasshopper.Kernel.Types;
using System;
using System.Linq;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Pdm;

static class GetTopSolidDocument
{

    public static DesignDocument GetDesignDocument(GH_ObjectWrapper wrapper)
    {
        IDocument res = null;
        DesignDocument designDocument = null;
        TestifStrato(wrapper, designDocument);
        if (designDocument != null) return designDocument;

        if (wrapper.Value is string || wrapper.Value is GH_String)
        {
            res = DocumentStore.Documents.Where(x => x.Name.ToString() == wrapper.Value.ToString()).FirstOrDefault();
            designDocument = res as DesignDocument;
        }
        else if (wrapper.Value is IDocumentItem)
            designDocument = (wrapper.Value as IDocumentItem).OpenLastValidMinorRevisionDocument() as DesignDocument;
        else if (wrapper.Value is IDocument)
            designDocument = wrapper.Value as DesignDocument;

        if (designDocument is null)
            designDocument = TopSolid.Kernel.UI.Application.CurrentDocument as DesignDocument;

        return designDocument;
    }

    private static void TestifStrato(GH_ObjectWrapper wrapper, DesignDocument designDocument)
    {
        designDocument = GetTopSolidDocumentStrato.GetSliceDocument(wrapper);

    }
}