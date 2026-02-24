using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitAppScriptRevitTS.Services;
using RevitAppScriptRevitTS.UI;
using RevitAppScriptRevitTS.Wrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace RevitAppScriptRevitTS.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    public class SecondCommand : CommandBase
    {
        public static List<FamilyInstance> _listFamilyInstance;
        public static List<Dictionary<string, object>> _listElement;
        private static ExternalEvent _externalEvent;
        private static SecondCommandEventHandler _handler = new SecondCommandEventHandler();

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = GetUIApplication(commandData);
            UIDocument uidoc = GetUIDocument(commandData);
            Document doc = GetDocument(commandData);


            Reference reference = uidoc.Selection.PickObject(ObjectType.Element, new FilterPipeAccessory());
            Element elem = doc.GetElement(reference.ElementId);

            _listFamilyInstance = ReturnElements.GetPipeAccessory(commandData, elem);

            if (_listFamilyInstance.Count == 1)
            {
                TaskDialog.Show("Уведомление", $"Кран в единственном экземпляре.");
                return Result.Succeeded;
            }

            Dictionary<string, object> selectElement = ConvertElemenettoDictionary(elem, GetPointXYZ(elem), 0);
            Dictionary<string, object> farElement = GetFarDictionary(elem);
            Dictionary<string, object> nearElement = GetNearDictionary(elem);

            _listElement = new List<Dictionary<string, object>> { selectElement, farElement, nearElement};

            _handler.activeView = doc.ActiveView;
            _handler.selectedElement = elem;
            _externalEvent = ExternalEvent.Create(_handler);

            PrintWindow(uiapp);

            return Result.Succeeded;
        }

        private void PrintWindow(UIApplication uiapp)
        {
            SecondCommandWindow wV = new SecondCommandWindow();
            //Нужно для отображения нашего окна поверх Revit
            var revitHandle = uiapp.MainWindowHandle;
            WindowInteropHelper helper = new WindowInteropHelper(wV);
            helper.Owner = revitHandle;
            wV.Topmost = false;
            SecondCommandViewModel vm = new SecondCommandViewModel(_listElement);
            wV.DataContext = vm;
            wV.Activated += (s,e)=> OnColoringElement(true);
            wV.Closed += (s, e) => OnColoringElement(false);
            wV.Show();
        }

        private void OnColoringElement(bool param)
        {
            _handler.coloring = param;
            _externalEvent.Raise();
        }

        private Dictionary<string, object> GetNearDictionary(Element elem1)
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
            _handler.nearElement = elem2;
            Dictionary<string, object> dict = ConvertElemenettoDictionary(elem2, point2, distance);
            return dict;
        }

        private Dictionary<string, object> GetFarDictionary(Element elem1)
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
            _handler.farElement = elem2;
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
