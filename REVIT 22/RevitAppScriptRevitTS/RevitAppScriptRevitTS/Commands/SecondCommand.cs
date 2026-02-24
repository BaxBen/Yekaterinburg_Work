using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
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
using System.Windows.Controls;
using System.Windows.Interop;

namespace RevitAppScriptRevitTS.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    public class SecondCommand : CommandBase
    {
        public static List<FamilyInstance> _listFamilyInstance;
        public List<Dictionary<string, object>> _listElement;
        private static ExternalEvent _externalEventDocumentChanged;
        private static SecondCommandEventHandlerDocumentChanged _handlerDocumentChanged = new SecondCommandEventHandlerDocumentChanged();
        public Document _doc;
        private static bool _isWindowOpen = false;
        public Element _elem;
        public bool _isSubscription = true;

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (_isWindowOpen) return Result.Cancelled;
            _isWindowOpen = true;

            UIApplication uiapp = GetUIApplication(commandData);
            UIDocument uidoc = GetUIDocument(commandData);
            Document doc = GetDocument(commandData);

            Reference reference = uidoc.Selection.PickObject(ObjectType.Element, new FilterPipeAccessory());
            _elem = doc.GetElement(reference.ElementId);

            //
            _handlerDocumentChanged._selectedElement = _elem;
            _externalEventDocumentChanged = ExternalEvent.Create(_handlerDocumentChanged);
            //

            _listFamilyInstance = GetPipeAccessory(doc, _elem);
            if (_listFamilyInstance.Count == 1)
            {
                TaskDialog.Show("Уведомление", $"Кран в единственном экземпляре.");
                return Result.Succeeded;
            }

            _listElement = new List<Dictionary<string, object>> {
                ConvertElemenettoDictionary(_elem, GetPointXYZ(_elem), 0),
                GetFarDictionary(),
                GetNearDictionary()};

            _doc = doc;
            if (_isSubscription)
                doc.Application.DocumentChanged += OnDocumentChanged;

            PrintWindow(uiapp);

            return Result.Succeeded;
        }

        /// <summary>
        /// Обрабатывает событие DocumentChanged
        /// </summary>
        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            if (!_isSubscription)
            {
                try
                {
                    _doc.Application.DocumentChanged -= OnDocumentChanged;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.ToString());
                }
                return;
            }
            _externalEventDocumentChanged.Raise();
        }

        /// <summary>
        /// Отображает модальное окно команды, устанавливая его владельцем главное окно Revit
        /// </summary>
        private void PrintWindow(UIApplication uiapp)
        {
            var revitHandle = uiapp.MainWindowHandle;

            var window = new SecondCommandWindow();
            var viewModel = new SecondCommandViewModel(_listElement);
            _handlerDocumentChanged._window = window;
            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.Owner = revitHandle;
            window.Topmost = false;
            viewModel = new SecondCommandViewModel(_listElement);
            window.DataContext = viewModel;
            window.Show();
            window.Closed += (s, e) => OnClosed(false);
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
        /// Обрабатывает событие закрытия окна и обновляет состояние флагов подписки и открытого окна
        /// </summary>
        private void OnClosed(bool param)
        {
            _isWindowOpen = param;
            _isSubscription = param;
        }

        /// <summary>
        /// Поиск самого ближнего эксземпляра и приведения его в Dictionary Id XYZ Distance
        /// </summary>
        private Dictionary<string, object> GetNearDictionary()
        {
            Element elem2 = null;
            XYZ point1 = GetPointXYZ(_elem);
            XYZ point2 = null;
            double distance = 0;
            foreach (var item in _listFamilyInstance)
            {
                if (item.Id.IntegerValue == _elem.Id.IntegerValue) continue;

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
            XYZ point1 = GetPointXYZ(_elem);
            XYZ point2 = null;
            double distance = 0;
            foreach (var item in _listFamilyInstance)
            {
                if (item.Id.IntegerValue == _elem.Id.IntegerValue) continue;
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
