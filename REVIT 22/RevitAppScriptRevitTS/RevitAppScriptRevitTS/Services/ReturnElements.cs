using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAppScriptRevitTS.Commands
{
    public class ReturnElements
    {
        public static List<FamilyInstance> GetPipeAccessory(ExternalCommandData commandData, Element elem)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            List<FamilyInstance> list_Levels = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_PipeAccessory)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>().Where(x=> x.GetTypeId().IntegerValue == elem.GetTypeId().IntegerValue)
                .ToList();

            return list_Levels;
        }
    }
}
