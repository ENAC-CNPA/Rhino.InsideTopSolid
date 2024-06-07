using System;
using System.Windows.Forms;
using TK = TopSolid.Kernel;

namespace EPFL.GrasshopperTopSolid.Components.Dialogs
{
    public partial class ParameterShapeDialog : TopSolid.Kernel.UI.Dialogs.DialogPane
    {
        //Constructors:

        /// <summary>
        /// Dialog constructor
        /// </summary>
        /// <param name="inCommand">Command linked to this DialogBox</param>
        /// <param name="inDocument">Document linked to this DialogBox</param>
        public ParameterShapeDialog(TK.WX.Commands.Command inCommand, TK.DB.Documents.Document inDocument)
            : base(inCommand, inDocument)
        {
            //Initialize the GUI components
            InitializeComponent();
        }
    }
}
