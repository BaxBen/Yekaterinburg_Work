using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitAppScriptRevitTS.Services;
using RevitAppScriptRevitTS.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAppScriptRevitTS.Wrapper
{
    public class SecondCommandEventHandler : IExternalEventHandler
    {

        public void Execute(UIApplication app)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = app.ActiveUIDocument.Document;



        }

        public string GetName()
        {
            return "Second Command Event Handler";
        }
    }
}
