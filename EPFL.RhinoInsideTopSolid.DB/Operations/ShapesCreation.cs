using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.Documents;
using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.DB.Operations;
using TopSolid.Kernel.DB.References;
using TopSolid.Kernel.DB.SmartObjects;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.SX.IO;

namespace EPFL.RhinoInsideTopSolid.DB.Operations
{
    /// <summary>
    /// ShapesCreation class
    /// </summary>
    [GuidAttribute("174cbfd5-7975-478a-b6b4-ae6aaea4af7b")]
    public class ShapesCreation : CreationsOperation
    {
        // Constructors:
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapesCreation"/> class.
        /// </summary>
        /// <param name="inDocument">Container document (referenced).</param>
        /// <param name="inId">Element Index, or zero for automatic.</param>
        public ShapesCreation(Document inDocument, int inId)
        : base(inDocument, inId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapesCreation"/> class by reading
        /// data from a stream.
        /// </summary>
        /// <param name="inReader">Reader to use.</param>
        /// <param name="inDocument">Container document (referenced).</param>
        private ShapesCreation(IReader inReader, object inDocument)
        : base(inReader, inDocument)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapesCreation"/> class by copy.
        /// </summary>
        /// <param name="inOriginal">Original instance to copy.</param>
        /// <param name="inDocument">Container document (referenced).</param>
        /// <param name="inId">Element index, or zero for automatic.</param>
        private ShapesCreation(ShapesCreation inOriginal, Document inDocument, int inId)
        : base(inOriginal, inDocument, inId)
        {
        }
        #endregion

        #region Properties
        public ShapeList Shapes;

        public new ModelingDocument Document
        {
            get
            {
                return base.Document as ModelingDocument;
            }
        }
        #endregion


        public override Element Clone(Document inDocument, int inId)
        {
            return new ShapesCreation(this, inDocument, inId);
        }

        protected override void Execute()
        {
            foreach (var ts in Shapes)
            {
                ShapeEntity se = new ShapeEntity(this.Document, 0);
                se.Geometry = ts;
                se.Create(this.Document.ShapesFolderEntity);
                this.AddChildEntity(se);

            }
        }

        /// <inheritdoc/>
        public override void GetReferences(ReferenceList outRefs)
        {
            base.GetReferences(outRefs);
        }

        /// <inheritdoc/>
        public override void GetChildren(bool inIgnoresOperations, ElementList outChildren)
        {
            base.GetChildren(inIgnoresOperations, outChildren);

        }

        /// <inheritdoc/>
        public override void GetSmartObjectHandles(SmartObjectHandleList outHandles)
        {
            base.GetSmartObjectHandles(outHandles);
        }

        /// <inheritdoc/>
        public override void Write(IWriter inWriter)
        {
            base.Write(inWriter);
        }
    }
}
