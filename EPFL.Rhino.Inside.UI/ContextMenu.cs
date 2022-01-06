using TopSolid.Kernel.WX.Documents;

namespace EPFL.RhinoInsideTopSolid.UI
{
    /// <summary>
    /// Implements the context menu management for this AddIn.
    /// </summary>
    public static class ContextMenu
    {
        /// <summary>
        /// Adds the context menu management for this AddIn.
        /// </summary>
        public static void AddMenu()
        {
            //Browse all the available document types...
            foreach (DocumentWindowFactory factory in DocumentWindowFactoryStore.Factories)
            {
                factory.AddMenuContext(typeof(ContextMenu), "xml");
            }

        }
    }
}
