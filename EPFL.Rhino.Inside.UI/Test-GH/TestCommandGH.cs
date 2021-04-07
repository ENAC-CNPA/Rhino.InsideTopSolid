//using Sample_2;
using System;
using Rhino.Runtime.InProcess;
using Grasshopper;

namespace EPFL.Rhino.Inside.UI.Test
{

    public partial class TestCommandGH : TopSolid.Kernel.UI.Commands.MenuCommand
    {
        // Constructors:

        /// <summary>
        /// Command constructor
        /// </summary>
        public TestCommandGH()
        {
            Console.WriteLine("Test Start 0");
        }


        // Properties:


        /// <summary>
        /// Return true to Enable the command button
        /// </summary>
        protected override bool CanInvoke
        {
            get
            {
                //Best practice:
                // return the combinaison to the base & your tests

                //In this case, just return the base CanInvoke
                return base.CanInvoke;
            }
        }

        // Methods:

        /// <summary>
        /// Method call when the command button is pressed
        /// </summary>
        /// 
        static RhinoCore rhinoCore;
        //static readonly Guid GrasshopperGuid = new Guid(0xB45A29B1, 0x4343, 0x4035, 0x98, 0x9E, 0x04, 0x4E, 0x85, 0x80, 0xD9, 0xCF);
        //static GH_Document definition;

        protected override void Invoke()
        {
            if (rhinoCore == null)
                rhinoCore = new global::Rhino.Runtime.InProcess.RhinoCore(new string[] { "/NOSPLASH" }, WindowStyle.Normal);
        }

        //TopSolid.Kernel.TX.Undo.UndoSequence.UndoCurrent();
        //    TopSolid.Kernel.TX.Undo.UndoSequence.Start("Test", true);

        //    //TextParameterEntity texte = document.ParametersFolderEntity.FindEntity("TestparamSpeckle") as TextParameterEntity;
        //    TextParameterEntity texte = new TextParameterEntity(document, 0);
        //texte.Value = (JsonConvert.SerializeObject(state));
        //    //texte.Value = "convertedJson";
        //    texte.Name = "TestparamSpeckle";
        //    document.ParametersFolderEntity.AddEntity(texte);

        //    TopSolid.Kernel.TX.Undo.UndoSequence.End();

    }
}
