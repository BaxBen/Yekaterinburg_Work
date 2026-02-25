using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitAppScriptRevitTS.Commands;
using RevitAppScriptRevitTS.Services;
using RevitAppScriptRevitTS.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace RevitAppScriptRevitTS.Wrapper
{
    
    [Transaction(TransactionMode.Manual)]
    public class SecondCommandEventHandlerDocumentChanged : IExternalEventHandler
    {
        public SecondCommandWindow _window { get; set; }

        public Element _selectedElement { get; set; }
        public static List<FamilyInstance> _listFamilyInstance { get; set; }

        /// <summary>
        ///Выполняет обновление данных в окне при изменении документа
        /// Вызывается через ExternalEvent при срабатывании DocumentChanged
        /// </summary>
        public void Execute(UIApplication app)
        {

            Document doc = app.ActiveUIDocument.Document;
            try
            {
                _listFamilyInstance = GetPipeAccessory(doc, _selectedElement);

                _listFamilyInstance = GetPipeAccessory(doc, _selectedElement);
                if (_listFamilyInstance.Count == 1)
                {
                    TaskDialog.Show("Уведомление", $"Кран в единственном экземпляре.");
                    _window.Close();
                }
                else
                {
                    List<Dictionary<string, object>> listElement = new List<Dictionary<string, object>> {
                        ConvertElemenettoDictionary(_selectedElement, GetPointXYZ(_selectedElement), 0),
                        GetFarDictionary(),
                        GetNearDictionary()};

                    if (_window != null)
                    {
                        _window.DataContext = new SecondCommandViewModel(listElement);
                    }
                }

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", ex.ToString());
            }
        }

        /// <summary>
        /// Просто есть
        /// </summary>
        public string GetName()
        {
            return "Second Command EventHandler Document Changed";
        }

        /// <summary>
        /// Сбор всех экземпляров семейств трубопроводной арматуры имеющих тот же тип, что и указанный элемент
        /// </summary>
        public static List<FamilyInstance> GetPipeAccessory(Document doc, Element elem)
        {
            List<FamilyInstance> list_Levels = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_PipeAccessory)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>().Where(x => x.GetTypeId().IntegerValue == elem.GetTypeId().IntegerValue)
                .ToList();

            return list_Levels;
        }

        /// <summary>
        /// Поиск самого ближнего эксземпляра и приведения его в Dictionary Id XYZ Distance
        /// </summary>
        private Dictionary<string, object> GetNearDictionary()
        {
            Element elem2 = null;
            XYZ point1 = GetPointXYZ(_selectedElement);
            XYZ point2 = null;
            double distance = 0;
            foreach (var item in _listFamilyInstance)
            {
                if (item.Id.IntegerValue == _selectedElement.Id.IntegerValue) continue;

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
            return ConvertElemenettoDictionary(elem2, point2, distance);
        }

        /// <summary>
        /// Поиск самого дальнего эксземпляра и приведения его в Dictionary Id XYZ Distance
        /// </summary>
        private Dictionary<string, object> GetFarDictionary()
        {
            Element elem2 = null;
            XYZ point1 = GetPointXYZ(_selectedElement);
            XYZ point2 = null;
            double distance = 0;
            foreach (var item in _listFamilyInstance)
            {
                if (item.Id.IntegerValue == _selectedElement.Id.IntegerValue) continue;
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
            return ConvertElemenettoDictionary(elem2, point2, distance);
        }

        /// <summary>
        /// Конвертирование входных данных в Dictionary Id XYZ Distance
        /// </summary>
        private Dictionary<string, object> ConvertElemenettoDictionary(Element elem, XYZ point, double distance)
        {
            return new Dictionary<string, object>
            {
                ["Id"] = elem.Id.IntegerValue,
                ["XYZ"] = $"X: {point.X:F3} Y: {point.Y:F3} Z: {point.Z:F3}",
                ["Distance"] = Math.Round(UnitUtils.ConvertFromInternalUnits(distance, UnitTypeId.Meters), 2)
            };
        }

        /// <summary>
        /// Возвращает XYZ
        /// </summary>
        private XYZ GetPointXYZ(Element elem)
        {
            LocationPoint locationPoint = elem.Location as LocationPoint;
            return locationPoint.Point;
        }
    }
}
