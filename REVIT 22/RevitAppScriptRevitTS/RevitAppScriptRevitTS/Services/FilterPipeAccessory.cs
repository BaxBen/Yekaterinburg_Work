using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAppScriptRevitTS.Services
{
    public class FilterPipeAccessory : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category?.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
