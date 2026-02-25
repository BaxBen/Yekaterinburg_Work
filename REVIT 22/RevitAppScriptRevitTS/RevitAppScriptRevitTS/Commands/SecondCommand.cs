using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitAppScriptRevitTS.Services;
using RevitAppScriptRevitTS.Services.SecondCommand;
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
        private static ExternalEvent _externalEventDocumentChanged;
        private static SecondCommandEventHandlerDocumentChanged _handlerDocumentChanged = new SecondCommandEventHandlerDocumentChanged();
        private static bool _isWindowOpen = false;
        public bool _isSubscription = true;

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = GetUIApplication(commandData);
            UIDocument uidoc = GetUIDocument(commandData);
            Document doc = GetDocument(commandData);

            if (_isWindowOpen) return Result.Cancelled;
            _isWindowOpen = true;

            if (_isSubscription)
                doc.Application.DocumentChanged += OnDocumentChanged;


            Reference reference = uidoc.Selection.PickObject(ObjectType.Element, new FilterPipeAccessory());
            Element elem = doc.GetElement(reference.ElementId);

            //
            _handlerDocumentChanged._selectedElement = elem;
            _externalEventDocumentChanged = ExternalEvent.Create(_handlerDocumentChanged);
            //

            List<FamilyInstance>  listFamilyInstance = Utils.GetPipeAccessory(doc, elem);
            if (listFamilyInstance.Count == 1)
            {
                TaskDialog.Show("Уведомление", $"Кран в единственном экземпляре.");
                _isWindowOpen = false;
                return Result.Succeeded;
            }

            List<Dictionary<string, object>>  listElement = new List<Dictionary<string, object>> {
                Utils.ConvertElemenetToDictionary(elem, Utils.GetPointXYZ(elem), 0),
                Utils.GetFarDictionary(elem, listFamilyInstance),
                Utils.GetNearDictionary(elem, listFamilyInstance)};

            PrintWindow(uiapp, listElement);

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
                    e.GetDocument().Application.DocumentChanged -= OnDocumentChanged;
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
        /// Обрабатывает событие закрытия окна и обновляет состояние флагов подписки и открытого окна
        /// </summary>
        private void OnClosed(bool param)
        {
            _isWindowOpen = param;
            _isSubscription = param;
        }

        /// <summary>
        /// Отображает модальное окно команды, устанавливая его владельцем главное окно Revit
        /// </summary>
        private void PrintWindow(UIApplication uiapp, List<Dictionary<string, object>> listElement)
        {
            var revitHandle = uiapp.MainWindowHandle;

            var window = new SecondCommandWindow();
            var viewModel = new SecondCommandViewModel(listElement);
            _handlerDocumentChanged._window = window;
            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.Owner = revitHandle;
            window.Topmost = false;
            viewModel = new SecondCommandViewModel(listElement);
            window.DataContext = viewModel;
            window.Show();
            window.Closed += (s, e) => OnClosed(false);
        }
    }
}
