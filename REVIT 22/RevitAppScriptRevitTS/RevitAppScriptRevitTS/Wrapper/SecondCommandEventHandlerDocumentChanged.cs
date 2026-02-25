using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitAppScriptRevitTS.Commands;
using RevitAppScriptRevitTS.Services;
using RevitAppScriptRevitTS.Services.SecondCommand;
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

        /// <summary>
        ///Выполняет обновление данных в окне при изменении документа
        /// Вызывается через ExternalEvent при срабатывании DocumentChanged
        /// </summary>
        public void Execute(UIApplication app)
        {

            Document doc = app.ActiveUIDocument.Document;
            try
            {
                List<FamilyInstance>  listFamilyInstance = Utils.GetPipeAccessory(doc, _selectedElement);
                if (listFamilyInstance.Count == 1)
                {
                    TaskDialog.Show("Уведомление", $"Кран в единственном экземпляре.");
                    _window.Close();
                }
                else
                {
                    List<Dictionary<string, object>> listElement = new List<Dictionary<string, object>> {
                        Utils.ConvertElemenetToDictionary(_selectedElement, Utils.GetPointXYZ(_selectedElement), 0),
                        Utils.GetFarDictionary(_selectedElement, listFamilyInstance),
                        Utils.GetNearDictionary(_selectedElement, listFamilyInstance)};

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
    }
}
