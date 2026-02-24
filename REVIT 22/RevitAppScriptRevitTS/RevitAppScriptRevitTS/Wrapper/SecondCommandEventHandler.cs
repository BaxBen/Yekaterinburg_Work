using Autodesk.Revit.Attributes;
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
    [Transaction(TransactionMode.Manual)]
    public class SecondCommandEventHandler : IExternalEventHandler
    {
        public Element _farElement { get; set; }
        public Element _nearElement { get; set; }
        public Element _selectedElement { get; set; }
        public View _activeView { get; set; }
        public bool _coloring {  get; set; }
        public void Execute(UIApplication app)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = app.ActiveUIDocument.Document;
            try
            {
                using (Transaction trans = new Transaction(doc, "Изменить цвет элементов"))
                {
                    trans.Start();

                    OverrideGraphicSettings overrideSettings = new OverrideGraphicSettings();

                    if (_coloring)
                    {
                        Color color = new Color(0, 255, 0);
                        overrideSettings.SetProjectionLineColor(color);
                        overrideSettings.SetSurfaceForegroundPatternColor(color);
                        overrideSettings.SetSurfaceBackgroundPatternColor(color);

                        _activeView.SetElementOverrides(_selectedElement.Id, overrideSettings);
                        _activeView.SetElementOverrides(_farElement.Id, overrideSettings);
                        _activeView.SetElementOverrides(_nearElement.Id, overrideSettings);
                    }
                    else 
                    {
                        _activeView.SetElementOverrides(_selectedElement.Id, overrideSettings);
                        _activeView.SetElementOverrides(_farElement.Id, overrideSettings);
                        _activeView.SetElementOverrides(_nearElement.Id, overrideSettings);
                    }
                    trans.Commit();
                }

            }
            catch (Exception ex) 
            {
                TaskDialog.Show("Error", ex.ToString());
            }
            

        }

        public string GetName()
        {
            return "Second Command EventHandler Coloring Elements";
        }
    }
}
