using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAppScriptRevitTS.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public abstract class CommandBase : IExternalCommand
    {
        public abstract Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements);
        protected Document GetDocument(ExternalCommandData commandData)
        {
            return commandData.Application.ActiveUIDocument.Document;
        }
        protected UIDocument GetUIDocument(ExternalCommandData commandData)
        {
            return commandData.Application.ActiveUIDocument;
        }
        protected UIApplication GetUIApplication(ExternalCommandData commandData)
        {
            return commandData.Application;
        }
    }
}
