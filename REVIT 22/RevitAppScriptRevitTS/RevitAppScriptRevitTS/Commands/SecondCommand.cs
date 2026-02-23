using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitAppScriptRevitTS.Services;
using RevitAppScriptRevitTS.UI;
using RevitAppScriptRevitTS.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAppScriptRevitTS.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    public class SecondCommand : CommandBase
    {
        public static List<FamilyInstance> _listFamilyInstance;
        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = GetUIApplication(commandData);
            UIDocument uidoc = GetUIDocument(commandData);
            Document doc = GetDocument(commandData);

            Reference reference = uidoc.Selection.PickObject(ObjectType.Element, new FilterPipeAccessory());
            Element elem = doc.GetElement(reference.ElementId);
            XYZ point = GetPointXYZ(elem);
            _listFamilyInstance = ReturnElements.GetPipeAccessory(commandData, elem);

            if (_listFamilyInstance.Count == 1)
            {
                TaskDialog.Show("Уведомление", $"Кран в единственном экземпляре.");
                return Result.Succeeded;
            }

            //XYZ point = GetPointXYZ(selectedElement);
            Dictionary<string, object> selectElement = ConvertElemenettoDictionary(elem, point, 0);
            Dictionary<string, object> farElement = GetFarElement(elem);
            Dictionary<string, object> nearElement = GetNearElement(elem);

            List<Dictionary<string, object>> listElement = new List<Dictionary<string, object>> { selectElement, farElement, nearElement};


            SecondCommandWindow wV = new SecondCommandWindow(uiapp, listElement);
            wV.Show();

            return Result.Succeeded;
        }

        private Dictionary<string, object> GetNearElement(Element elem1)
        {
            Element elem2 = null;
            XYZ point1 = GetPointXYZ(elem1);
            XYZ point2 = null;
            double distance = 0;
            foreach (var item in _listFamilyInstance)
            {
                if (item.Id.IntegerValue == elem1.Id.IntegerValue) continue;
                point2 = GetPointXYZ(item);
                double distance2 = point1.DistanceTo(point2);

                if (elem2 == null)
                {
                    elem2 = item;
                    distance = distance2;
                }
                else if (distance > distance2)
                {
                    distance = distance2;
                    elem2 = item;
                }
            }
            Dictionary<string, object> dict = ConvertElemenettoDictionary(elem2, point2, distance);
            return dict;
        }

        private Dictionary<string, object> GetFarElement(Element elem1)
        {
            Element elem2 = null;
            XYZ point1 = GetPointXYZ(elem1);
            XYZ point2 = null;
            double distance = 0;
            foreach (var item in _listFamilyInstance)
            {
                if (item.Id.IntegerValue == elem1.Id.IntegerValue) continue;
                point2 = GetPointXYZ(item);
                double distance2 = point1.DistanceTo(point2);

                if (elem2 == null)
                {
                    elem2 = item;
                    distance = distance2;
                }
                else if (distance < distance2)
                {
                    distance = distance2;
                    elem2 = item;
                }
            }
            Dictionary<string, object> dict = ConvertElemenettoDictionary(elem2, point2, distance);
            return dict;
        }

        private Dictionary<string, object> ConvertElemenettoDictionary(Element elem, XYZ point, double distance)
        {
            Dictionary<string, object> dictElement = new Dictionary<string, object>();
            dictElement["Id"] = elem.Id.IntegerValue;
            dictElement["XYZ"] = $"X: {Math.Round(point.X, 3)} Y: {Math.Round(point.Y, 3)} Z: {Math.Round(point.Z, 3)}";
            dictElement["Distance"] = Math.Round(UnitUtils.ConvertFromInternalUnits(distance, UnitTypeId.Meters), 2);
            return dictElement;
        }
        private XYZ GetPointXYZ(Element elem)
        {
            LocationPoint locationPoint = elem.Location as LocationPoint;
            XYZ point = locationPoint.Point;

            return point;
        }
    }
}
